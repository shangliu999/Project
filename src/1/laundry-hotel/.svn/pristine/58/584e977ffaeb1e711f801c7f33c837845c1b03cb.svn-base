using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Shop.Models;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Shop.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class OrderController : Controller
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<goodsorder> i_goodsorder { get; set; }

        [Dependency]
        public IRepository<goodsorderdetail> i_goodsorderdetail { get; set; }

        // GET: Shop/Order
        public ActionResult Index()
        {
            List<region> hotelList = i_region.Entities.Where(v => v.RegionType == 1 && v.IsDelete == false).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();
            ViewData["HotelList"] = hotelList;

            return View();
        }

        public ActionResult GetOrderList(ParamModel model)
        {
            string no = model.qCondition1;
            int state = 0;
            int.TryParse(model.qCondition2, out state);
            int hotelId = 0;
            int.TryParse(model.qCondition3, out hotelId);
            int payType = 0;
            int.TryParse(model.qCondition4, out payType);
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            if (model.qCondition5 != null && model.qCondition6 != null)
            {
                DateTime.TryParse(model.qCondition5, out startDate);
                DateTime.TryParse(model.qCondition6, out endDate);
            }
            endDate = endDate.AddDays(1);

            var query = i_goodsorder.Entities;

            if (string.IsNullOrWhiteSpace(no))
            {
                if (state > 0)
                {
                    query = query.Where(v => v.OrderState == state);
                }
                if (hotelId > 0)
                {
                    query = query.Where(v => v.HotelID == hotelId);
                }
                if (payType > 0)
                {
                    query = query.Where(v => v.PaymentType == payType);
                }

                query = query.Where(v => v.CreateTime >= startDate && v.CreateTime < endDate);
            }
            else
            {
                query = query.Where(v => v.OrderNo.Contains(no));
            }

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderByDescending(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from t in orderingQuery
                       join h in i_region.Entities on t.HotelID equals h.ID
                       select new
                       {
                           t.ID,
                           t.OrderNo,
                           HotelName = h.RegionName,
                           t.OrderState,
                           t.PaymentType,
                           t.CreateTime,
                           t.DeliveryTime,
                           t.DeliveryDate,
                           t.DistributionTime,
                           t.SignTime,
                           t.Distributor
                       };
            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = data
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrderDetail(string id)
        {
            MsgModel msgModel = new MsgModel();

            var query = from t in i_goodsorder.Entities
                        join h in i_region.Entities on t.HotelID equals h.ID
                        where t.ID == id
                        select new { t.OrderNo, HotelName = h.RegionName, t.ID, t.OrderState, t.PaymentType, t.Remarks, t.SignTime, t.TotalMoney, t.TotalQuantity, t.ComfirmTime, t.CreateTime, t.DeliveryDate, t.DeliveryTime, t.DistributionTime, t.Distributor, t.DistributorTel };

            OrderModel model = new OrderModel();
            var data = query.FirstOrDefault();
            if (data != null)
            {
                model.OrderId = data.ID;
                model.OrderNo = data.OrderNo;
                model.State = data.OrderState;
                model.StateName = Enum.GetName(typeof(OrderState), data.OrderState);
                model.PaymentName = Enum.GetName(typeof(Payment), data.PaymentType);
                model.HotelName = data.HotelName;
                model.GoodsTotal = data.TotalQuantity.ToString();
                model.GoodsTotalMoney = data.TotalQuantity.ToString("N2");
                model.CreateTime = data.CreateTime.ToString("yyyy/MM/dd HH:mm");
                model.ComfirmTime = data.ComfirmTime.HasValue ? data.ComfirmTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                model.DistributonTime = data.DistributionTime.HasValue ? data.DistributionTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                model.SignTime = data.SignTime.HasValue ? data.SignTime.Value.ToString("yyyy/MM/dd HH:mm") : "";
                model.Delivery = data.DeliveryDate.ToString("yyyy/MM/dd") + " " + data.DeliveryTime;
                model.Distributor = data.Distributor;
                model.DistributorTel = data.DistributorTel;

                List<OrderDetailModel> list = new List<OrderDetailModel>();
                OrderDetailModel detailModel = null;
                i_goodsorderdetail.Entities.Where(v => v.OrderID == data.ID).ToList().ForEach(q =>
                {
                    detailModel = new OrderDetailModel();
                    detailModel.GoodsName = q.GoodsName;
                    detailModel.GoodsCount = q.GoodsCount.ToString();
                    detailModel.GoodsPrice = (q.GoodsCount * q.GoodMoney).ToString("N2");
                    list.Add(detailModel);
                });

                model.Detail = list;
            }
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult OrderConfirm(string id)
        {
            MsgModel msgModel = new MsgModel();

            goodsorder model = i_goodsorder.GetByKey(id);

            if (model != null)
            {
                model.ComfirmTime = DateTime.Now;
                model.ComfirmUserID = LoginUserManage.GetInstance().GetLoginUserId();
                if (model.OrderState == 1)
                {
                    model.OrderState = 2;
                }

                i_goodsorder.Update(model);
            }

            msgModel.ResultCode = 0;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult OrderDistribution(string id, string name, string tel)
        {
            MsgModel msgModel = new MsgModel();

            goodsorder model = i_goodsorder.GetByKey(id);

            if (model != null)
            {
                if (model.OrderState < 3)
                {
                    model.OrderState = 3;
                    model.Distributor = name;
                    model.DistributorTel = tel;
                    model.DistributionTime = DateTime.Now;


                    i_goodsorder.Update(model);
                }

                i_goodsorder.Update(model);
            }

            msgModel.ResultCode = 0;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult OrderSign(string id)
        {
            MsgModel msgModel = new MsgModel();

            goodsorder model = i_goodsorder.GetByKey(id);

            if (model != null)
            {
                if (model.OrderState == 3)
                {
                    model.OrderState = 4;
                    model.SignTime = DateTime.Now;
                    i_goodsorder.Update(model);
                }

                i_goodsorder.Update(model);
            }

            msgModel.ResultCode = 0;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }
    }
}