using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using ETexsys.Cloud.API.Common;
using ETexsys.IDAL;
using ETexsys.Model;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ETexsys.Cloud.API.Controllers
{
    [SupportFilter]
    public class BillInquiryController : ApiController
    {
        [Dependency]
        public IRepository<invoice> ibr { get; set; }

        [Dependency]
        public IRepository<invoicedetail> ibc { get; set; }

        [Dependency]
        public IRepository<invoiceattach> i_invoiceattach { get; set; }

        [Dependency]
        public IRepository<repeatop> i_repeatop { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<sys_user> i_user { get; set; }

        [Dependency]
        public IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }

        public MsgModel SelectInvoice([FromBody]BillInquiryParamModel model)
        {
            MsgModel msgModel = new MsgModel();
            DateTime endTime = model.CreateTime.AddDays(1);

            int invtype = model.InvType;
            int invsubtype = 0;

            switch (model.InvType)
            {
                case 22:
                    invtype = 3;
                    invsubtype = model.InvType;
                    break;
                case 21:
                    invtype = 4;
                    invsubtype = model.InvType;
                    break;
                case 24:
                    invtype = 3;
                    invsubtype = model.InvType;
                    break;
                case 20:
                    invtype = 4;
                    invsubtype = model.InvType;
                    break;
                case 50:
                    invtype = 5;
                    invsubtype = 0;
                    break;
                case 51:
                    invtype = 5;
                    invsubtype = 1;
                    break;
                case 80:
                    invtype = 8;
                    invsubtype = 0;
                    break;
                case 81:
                    invtype = 8;
                    invsubtype = 1;
                    break;
            }

            if (model.InvType == 2)
            {
                var hotelModel = i_region.Entities.Where(v => v.ID == model.HotelId).FirstOrDefault();
                if (hotelModel != null)
                {
                    if (hotelModel.RegionMode == 1)
                    {
                        model.RegionID = 0;
                    }
                }
            }

            var query = from t in ibr.Entities
                         join h in i_region.Entities on t.HotelID equals h.ID into h_join
                         from h in h_join.DefaultIfEmpty()
                         join r in i_region.Entities on t.RegionID equals r.ID into r_join
                         from r in r_join.DefaultIfEmpty()
                         where t.InvType == invtype && t.CreateTime >= model.CreateTime && t.CreateTime < endTime
                         select new
                         {
                             t.InvNo,
                             t.InvSubType,
                             t.CreateUserName,
                             t.HotelID,
                             HotelName = h.RegionName,
                             r.RegionName,
                             t.RegionID,
                             t.Quantity,
                             t.CreateTime
                         };

            if (invtype == 1 || invtype == 2)
            {
                if (model.HotelId != 0)
                {
                    query = query.Where(c => c.HotelID == model.HotelId);
                }
                if (model.RegionID != 0)
                {
                    query = query.Where(c => c.RegionID == model.RegionID);
                }
            }
            else
            {
                query = query.Where(c => c.InvSubType == invsubtype);
            }
            
            List<ResponseBillInquiry> list1 = new List<ResponseBillInquiry>();
            ResponseBillInquiry BImodel = new ResponseBillInquiry();
            query.ToList().ForEach(q =>
            {
                BImodel = new ResponseBillInquiry();
                BImodel.InvNo = q.InvNo;
                BImodel.CreateUserName = q.CreateUserName;
                BImodel.HotelID = q.HotelID;
                BImodel.HotelName = q.HotelName;
                BImodel.RegionID = q.RegionID;
                BImodel.RegionName = q.RegionName;
                BImodel.Quantity = q.Quantity;
                BImodel.CreateTime = q.CreateTime;
                list1.Add(BImodel);
            });
            msgModel.Result = list1;
            return msgModel;
        }

        public MsgModel Details([FromBody]DetailsParamModel model)
        {
            MsgModel msgModel = new MsgModel();
            var list = ibc.Entities.Where(p => p.InvNo == model.OrderNumber).OrderBy(v => v.ClassName).ThenBy(v => v.SizeName).ToList();
            List<ResponseDetail> list1 = new List<ResponseDetail>();
            ResponseDetail resposdetail = new ResponseDetail();
            list.ForEach(q =>
            {
                resposdetail = new ResponseDetail();
                resposdetail.ClassName = q.ClassName;
                resposdetail.Number = q.TextileCount;
                resposdetail.Size = q.SizeName;
                list1.Add(resposdetail);
            });

            if (list.Count > 0)
            {
                invoicedetail dModel = list.FirstOrDefault();
                if (dModel != null)
                {
                    if (dModel.InvType == 1 || dModel.InvType == 2)
                    {
                        List<string> BagList = i_invoiceattach.Entities.Where(v => v.InvNo == model.OrderNumber && v.ParamType == "Bag").Select(v => v.ParamValue).ToList();

                        msgModel.OtherResult = BagList;
                    }
                }
            }
            msgModel.Result = list1;
            return msgModel;
        }

        public MsgModel Summary([FromBody]SummaryParamModel model)
        {
            MsgModel msgModel = new MsgModel();
            DateTime Time = model.CreateTime.AddDays(1);

            int invtype = model.InvType;
            int invsubtype = 0;

            if (model.InvType == 2)
            {
                var hotelModel = i_region.Entities.Where(v => v.ID == model.HotelID).FirstOrDefault();
                if (hotelModel != null)
                {
                    if (hotelModel.RegionMode == 1)
                    {
                        model.RegionID = 0;
                    }
                }
            }

            switch (model.InvType)
            {
                case 22:
                    invtype = 3;
                    invsubtype = model.InvType;
                    break;
                case 21:
                    invtype = 4;
                    invsubtype = model.InvType;
                    break;
                case 24:
                    invtype = 3;
                    invsubtype = model.InvType;
                    break;
                case 20:
                    invtype = 4;
                    invsubtype = model.InvType;
                    break;
                case 50:
                    invtype = 5;
                    invsubtype = 0;
                    break;
                case 51:
                    invtype = 5;
                    invsubtype = 1;
                    break;
                case 80:
                    invtype = 8;
                    invsubtype = 0;
                    break;
                case 81:
                    invtype = 8;
                    invsubtype = 1;
                    break;
            }

            var query1 = from t in ibc.Entities
                         join c in i_textileclass.Entities on t.ClassID equals c.ID
                         join s in i_size.Entities on t.SizeID equals s.ID into s_join
                         from s in s_join.DefaultIfEmpty()
                         where t.InvType == invtype && t.InvCreateTime >= model.CreateTime && t.InvCreateTime < Time
                         select new
                         {
                             t.HotelID,
                             t.RegionID,
                             t.InvSubType,
                             ClassSort = c.Sort,
                             c.ClassName,
                             SizeSort = s.Sort,
                             s.SizeName,
                             t.TextileCount,
                         };

            if (invtype == 1 || invtype == 2)
            {
                if (model.HotelID != 0)
                {
                    query1 = query1.Where(c => c.HotelID == model.HotelID);
                }
                if (model.RegionID != 0)
                {
                    query1 = query1.Where(c => c.RegionID == model.RegionID);
                }
            }
            else
            {
                query1 = query1.Where(c => c.InvSubType == invsubtype);
            }

            var query = from c in query1
                        group c by new { t0 = c.ClassSort, t1 = c.ClassName, t2 = c.SizeSort, t3 = c.SizeName } into m
                        orderby m.Key.t0, m.Key.t1, m.Key.t2, m.Key.t3
                        select new
                        {
                            className = m.Key.t1,
                            sizeName = m.Key.t3,
                            textileCount = m.Sum(v => v.TextileCount),
                        };

            List<ResponseSummary> list1 = new List<ResponseSummary>();
            ResponseSummary response = new ResponseSummary();
            query.ToList().ForEach(q =>
            {
                response = new ResponseSummary();
                response.Number = q.textileCount;
                response.ProductName = q.className;
                response.Size = q.sizeName;
                list1.Add(response);
            });

            if (model.InvType == 1 || model.InvType == 2)
            {
                List<string> InvNoList = ibc.Entities.Where(t => t.InvType == invtype && t.InvSubType == invsubtype
                  && t.InvCreateTime >= model.CreateTime && t.InvCreateTime < Time && t.HotelID == model.HotelID && t.RegionID == model.RegionID).Select(v => v.InvNo).ToList();

                List<string> BagList = i_invoiceattach.Entities.Where(v => InvNoList.Contains(v.InvNo) && v.ParamType == "Bag").Select(v => v.ParamValue).ToList();

                msgModel.OtherResult = BagList;
            }

            msgModel.Result = list1;
            return msgModel;
        }

        #region android端

        public MsgModel SearchInvoice([FromBody]InvoiceQueryParamModel model)
        {
            MsgModel msgModel = new MsgModel();

            var query = from i in ibr.Entities
                        join h in i_region.Entities on i.HotelID equals h.ID into h_join
                        from h in h_join.DefaultIfEmpty()
                        join r in i_region.Entities on i.RegionID equals r.ID into r_join
                        from r in r_join.DefaultIfEmpty()
                        join dv in i_sys_user_dataview.Entities on i.HotelID equals dv.RegionID
                        orderby i.CreateTime descending
                        where i.InvType == model.InvType && dv.UserID == model.CreateUserID
                        select new
                        {
                            i.ID,
                            i.InvNo,
                            i.InvState,
                            i.InvSubType,
                            i.CreateUserName,
                            i.HotelID,
                            HotelName = h.RegionName,
                            r.RegionName,
                            i.RegionID,
                            i.Quantity,
                            i.CreateTime
                        };

            if (!string.IsNullOrEmpty(model.CreateTime))
            {
                DateTime beginTime = DateTime.Parse(model.CreateTime);
                DateTime endTime = beginTime.AddDays(1);
                query = query.Where(c => c.CreateTime > beginTime && c.CreateTime < endTime);
            }

            if (model.HotelID == 0)
            {
                query = query.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize);
            }
            else
            {
                if (model.RegionID == 0)
                {
                    query = query.Where(c => c.HotelID == model.HotelID)
                        .Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize);
                }
                else
                {
                    query = query.Where(c => c.HotelID == model.HotelID && c.RegionID == model.RegionID)
                        .Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize);
                }
            }

            List<ResponseInvoiceModel> list = new List<ResponseInvoiceModel>();
            ResponseInvoiceModel rim = null;
            query.ToList().ForEach(c =>
            {
                rim = new ResponseInvoiceModel();
                rim.ID = c.ID;
                rim.InvNo = c.InvNo;
                rim.InvSubType = c.InvSubType;
                //rim.InvState = c.InvState==1;
                if (c.InvState == 1)
                {
                    rim.InvState = model.InvType == 1 ? "已收货" : "已签收";
                }

                var bagquery = from t in i_invoiceattach.Entities
                               where t.InvNo.Equals(c.InvNo) && t.ParamType == "Bag"
                               select new { bagNo = t.ParamValue };
                rim.Bag = string.Join(",", bagquery.ToList().Select(x => x.bagNo));

                rim.CreateTime = c.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                rim.CreateUserName = c.CreateUserName;
                rim.HotelName = c.HotelName;
                rim.Quantity = c.Quantity;
                rim.RegionName = c.RegionName;
                rim.Remark = "";

                rim.Data = new List<ResponseInvoiceDetailModel>();

                //明细
                var detailquery = from t in ibc.Entities
                                  join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                                  join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                  from s in s_join.DefaultIfEmpty()
                                  where t.InvID == rim.ID
                                  group t by new
                                  {
                                      tc.Sort,
                                      t.ClassName,
                                      SizeSort = s.Sort,
                                      t.SizeName,
                                  } into m
                                  orderby m.Key.Sort, m.Key.ClassName, m.Key.SizeSort, m.Key.SizeName
                                  select new
                                  {
                                      className = m.Key.ClassName,
                                      sizeName = m.Key.SizeName,
                                      count = m.Sum(v => v.TextileCount),
                                  };

                ResponseInvoiceDetailModel ridm = null;
                detailquery.ToList().ForEach(y =>
                {
                    ridm = new ResponseInvoiceDetailModel();

                    ridm.ClassName = y.className;
                    ridm.SizeName = y.sizeName;
                    ridm.TextileCount = y.count;

                    rim.Data.Add(ridm);
                });

                rim.RepeatOperators = new List<ResponseReapetOpModel>();
                //明细
                var repeatopquery = from t in i_repeatop.Entities
                                    join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                                    join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                    from s in s_join.DefaultIfEmpty()
                                    where t.InvID == rim.ID
                                    select new
                                    {
                                        className = tc.ClassName,
                                        sizeName = s.SizeName,
                                        repeatTime = t.PreCreateTime,
                                    };

                ResponseReapetOpModel rrom = null;
                repeatopquery.ToList().ForEach(r =>
                {
                    rrom = new ResponseReapetOpModel();
                    rrom.ShortName = "";
                    rrom.RegionName = "";
                    rrom.ClassName = r.className;
                    rrom.SizeName = r.sizeName;
                    rrom.RepeatInvCreateTime = r.repeatTime;

                    rim.RepeatOperators.Add(rrom);
                });

                list.Add(rim);
            });

            msgModel.ResultCode = 0;
            msgModel.Result = list;

            return msgModel;
        }

        public MsgModel SummaryInvoice([FromBody]InvoiceQueryParamModel model)
        {
            MsgModel msgModel = new MsgModel();

            var query = from i in ibc.Entities
                        join dv in i_sys_user_dataview.Entities on i.HotelID equals dv.RegionID into dv_join
                        from dv in dv_join.DefaultIfEmpty()
                        join h in i_region.Entities on i.HotelID equals h.ID into h_join
                        from h in h_join.DefaultIfEmpty()
                        join tc in i_textileclass.Entities on i.ClassID equals tc.ID
                        join s in i_size.Entities on i.SizeID equals s.ID into s_join
                        from s in s_join.DefaultIfEmpty()
                        orderby i.InvCreateTime descending
                        where i.InvType == model.InvType && dv.UserID == model.CreateUserID
                        select new
                        {
                            i.InvID,
                            i.InvSubType,
                            i.HotelID,
                            HotelName = h.RegionName,
                            i.InvCreateTime,
                            tc.Sort,
                            i.ClassName,
                            SizeSort = s.Sort,
                            i.SizeName,
                            i.TextileCount,
                            CreateTime = i.InvCreateTime.Year + "-" + i.InvCreateTime.Month + "-" + i.InvCreateTime.Day,
                        };

            if (!string.IsNullOrEmpty(model.CreateTime))
            {
                DateTime beginTime = DateTime.Parse(model.CreateTime);
                DateTime endTime = beginTime.AddDays(1);
                query = query.Where(c => c.InvCreateTime > beginTime && c.InvCreateTime < endTime);
            }

            if (model.HotelID == 0)
            {
            }
            else
            {
                query = query.Where(c => c.HotelID == model.HotelID);
            }

            List<ResponseInvoiceModel> list = new List<ResponseInvoiceModel>();
            ResponseInvoiceModel rim = null;

            query.GroupBy(v => new { v.HotelID, v.HotelName, createtime = v.CreateTime }).OrderByDescending(c => c.Key.createtime)
                  .Select(x => new { x.Key.HotelID, x.Key.HotelName, x.Key.createtime, TextileCount = x.Sum(y => y.TextileCount) })
                  .Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList().ForEach(v =>
                  {
                      rim = new ResponseInvoiceModel();
                      rim.HotelID = v.HotelID;
                      rim.HotelName = v.HotelName;
                      rim.CreateTime = v.createtime;
                      rim.Quantity = v.TextileCount;

                      rim.Data = new List<ResponseInvoiceDetailModel>();

                      ResponseInvoiceDetailModel ridm = null;

                      query.Where(x => x.HotelID == v.HotelID && x.CreateTime == v.createtime)
                      .GroupBy(x => new { x.InvSubType, x.Sort, x.ClassName, x.SizeSort, x.SizeName }).OrderBy(x => x.Key.Sort)
                      .ThenBy(x => x.Key.ClassName).ThenBy(x => x.Key.SizeSort).ThenBy(x => x.Key.SizeName)
                      .Select(x => new
                      {
                          x.Key.ClassName,
                          x.Key.SizeName,
                          normal = x.Sum(c => c.InvSubType == 1 ? c.TextileCount : 0),
                          dirty = x.Sum(c => c.InvSubType == 2 ? c.TextileCount : 0),
                          backwash = x.Sum(c => c.InvSubType == 3 ? c.TextileCount : 0),
                          guoshui = x.Sum(c => c.InvSubType == 4 ? c.TextileCount : 0),
                          sendcount = x.Sum(c => c.InvSubType == 0 ? c.TextileCount : 0),
                      })
                      .ToList().ForEach(y =>
                      {
                          ridm = new ResponseInvoiceDetailModel();

                          ridm.ClassName = y.ClassName + (string.IsNullOrEmpty(y.SizeName) ? "" : "(" + y.SizeName + ")");
                          ridm.SizeName = y.SizeName;
                          ridm.Normal = y.normal;
                          ridm.Dirty = y.dirty;
                          ridm.BackWash = y.backwash;
                          ridm.GuoShui = y.guoshui;
                          ridm.SendCount = y.sendcount;

                          rim.Data.Add(ridm);
                      });

                      List<string> ids = query.Select(c => c.InvID).ToList();

                      rim.RepeatOperators = new List<ResponseReapetOpModel>();
                      //明细
                      var repeatopquery = from t in i_repeatop.Entities
                                          join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                                          join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                          from s in s_join.DefaultIfEmpty()
                                          where ids.Contains(t.InvID) && t.HotelID == v.HotelID
                                          select new
                                          {
                                              className = tc.ClassName,
                                              sizeName = s.SizeName,
                                              repeatTime = t.PreCreateTime,
                                          };

                      ResponseReapetOpModel rrom = null;
                      repeatopquery.ToList().ForEach(r =>
                      {
                          rrom = new ResponseReapetOpModel();
                          rrom.ShortName = "";
                          rrom.RegionName = "";
                          rrom.ClassName = r.className;
                          rrom.SizeName = r.sizeName;
                          rrom.RepeatInvCreateTime = r.repeatTime;

                          rim.RepeatOperators.Add(rrom);
                      });

                      list.Add(rim);
                  });

            msgModel.ResultCode = 0;
            msgModel.Result = list;

            return msgModel;
        }

        public MsgModel OweInvoice([FromBody]InvoiceQueryParamModel model)
        {
            MsgModel msgModel = new MsgModel();

            DateTime beginTime = DateTime.Parse(model.CreateTime);
            DateTime endTime = beginTime.AddDays(1);

            var send = ibc.Entities.Where(c => c.InvType == 2 && c.DataType == 2 && c.InvCreateTime > beginTime && c.InvCreateTime < endTime);
            var recieve = ibc.Entities.Where(c => c.InvType == 1 && c.DataType == 2 && c.InvCreateTime > beginTime && c.InvCreateTime < endTime);
            if (model.HotelID != 0)
            {
                send = send.Where(c => c.HotelID == model.HotelID);
                recieve = recieve.Where(c => c.HotelID == model.HotelID);
            }
            if (model.RegionID != 0)
            {
                send = send.Where(c => c.RegionID == model.RegionID);
                recieve = recieve.Where(c => c.RegionID == model.RegionID);
            }

            var query1 = from t in send
                         join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                         group t by new
                         {
                             tc.Sort,
                             t.ClassName,
                         } into m
                         orderby m.Key.Sort, m.Key.ClassName
                         select new
                         {
                             className = m.Key.ClassName,
                             count = m.Sum(v => v.TextileCount),
                         };

            var query2 = from t in recieve
                         join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                         group t by new
                         {
                             tc.Sort,
                             t.ClassName,
                         } into m
                         orderby m.Key.Sort, m.Key.ClassName
                         select new
                         {
                             className = m.Key.ClassName,
                             count = m.Sum(v => v.TextileCount),
                         };

            List<ResponseOweModel> list = new List<ResponseOweModel>();
            ResponseOweModel rom = null;
            query2.ToList().ForEach(c =>
            {
                rom = new ResponseOweModel();
                rom.ClassName = c.className;
                rom.Rec = c.count;
                rom.Owe = -c.count;
                list.Add(rom);
            });

            query1.ToList().ForEach(c =>
            {
                ResponseOweModel rm = list.FirstOrDefault(x => x.ClassName == c.className);
                if (rm != null)
                {
                    rm.Send = c.count;
                    rm.Owe = rm.Send - rm.Rec;
                }
                else
                {
                    rom = new ResponseOweModel();
                    rom.ClassName = c.className;
                    rom.Rec = 0;
                    rom.Send = c.count;
                    rom.Owe = rom.Send - 0;
                    list.Add(rom);
                }
            });
            msgModel.ResultCode = 0;
            msgModel.Result = list;

            return msgModel;
        }

        #endregion
    }
}
