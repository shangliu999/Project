
using ETexsys.APIRequestModel.Response;
using ETexsys.Cloud.API.Common;
using ETexsys.IDAL;
using ETexsys.Model;
using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ETexsys.Cloud.API.Controllers
{
    [SupportFilter]
    public class BusinessInvoiceController : ApiController
    {
        [Dependency]
        public IRepository<businessinvoice> i_businessinvoice { get; set; }

        [Dependency]
        public IRepository<businessdetail> i_bussinesdetail { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        public MsgModel DeliveryTask()
        {
            MsgModel msgModel = new MsgModel();

            var query = from t in i_businessinvoice.Entities
                        join d in i_bussinesdetail.Entities on t.BID equals d.BID
                        join r in i_region.Entities on t.HotelID equals r.ID
                        where t.Stated == 2 && d.TextileCount > d.ExecCount
                        group d by new { t0 = t.BID, t1 = t.HotelID, t2 = r.RegionName, t3 = t.CreateTime, t4 = t.BNo } into m
                        select new
                        {
                            invId = m.Key.t0,
                            hotelId = m.Key.t1,
                            hotelName = m.Key.t2,
                            createDate = m.Key.t3,
                            no = m.Key.t4,
                            count = m.Sum(v => v.TextileCount) - m.Sum(v => v.ExecCount)
                        };

            List<ResponseDeliveryTaskModel> taskList = new List<ResponseDeliveryTaskModel>();
            ResponseDeliveryTaskModel model = null;
            DeliveryTaskDetailModel detailModel = null;


            query.ToList().ForEach(q =>
            {
                model = new ResponseDeliveryTaskModel();
                model.DeliveryCount = q.count.Value;
                model.DeliveryDate = q.createDate;
                model.DeliveryNo = q.no;
                model.HotelID = q.hotelId;
                model.HotelName = q.hotelName;

                var qt = from t in i_region.Entities
                         join b in i_brandtype.Entities on t.BrandID equals b.ID into b_join
                         from b in b_join.DefaultIfEmpty()
                         where t.ID == q.hotelId
                         select new { brandId = b == null ? 0 : b.ID, brandName = b == null ? "" : b.BrandName };

                var qtModel = qt.FirstOrDefault();

                model.Detail = new List<DeliveryTaskDetailModel>();
                i_bussinesdetail.Entities.Where(v => v.BID == q.invId && v.TextileCount > v.ExecCount).ToList().ForEach(a =>
                {
                    detailModel = new DeliveryTaskDetailModel();
                    detailModel.BrandID = qtModel == null ? 0 : qtModel.brandId;
                    detailModel.BrandName = qtModel == null ? "" : qtModel.brandName;
                    detailModel.ClassID = a.ClassID;
                    detailModel.ClassName = a.ClassName;
                    detailModel.SizeID = a.SizeID;
                    detailModel.SizeName = a.SizeName;
                    detailModel.TaskCount = (a.TextileCount - a.ExecCount).Value;
                    model.Detail.Add(detailModel);
                });

                taskList.Add(model);
            });

            msgModel.Result = taskList;

            return msgModel;
        }
    }
}
