using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Customer.Controllers
{
    /// <summary>
    /// 客户需求单
    /// </summary>
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class DemandController : Controller
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<sys_user> i_sys_user { get; set; }

        [Dependency]
        public IRepository<businessinvoice> i_businessinvoice { get; set; }

        [Dependency]
        public IRepository<businessdetail> i_bussinesdetail { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }

        // GET: Customer/CusDemand
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            List<region> hotelList = LoginUserManage.GetInstance().GetCurrentDataView().Where(v => v.RegionType == 1 && v.IsDelete == false).ToList();
            ViewData["HotelList"] = hotelList;

            return View();
        }

        public ActionResult GetDemandList(ParamModel model)
        {
            int hotelId = 0;
            int.TryParse(model.qCondition1, out hotelId);
            int state = 0;
            int.TryParse(model.qCondition2, out state);
            DateTime stratDate = DateTime.MinValue;
            DateTime.TryParse(model.qCondition3, out stratDate);
            DateTime endDate = DateTime.MinValue;
            DateTime.TryParse(model.qCondition4, out endDate);

            var query = i_businessinvoice.Entities.Where(v => v.InvType == 1);
            if (hotelId != 0)
            {
                query = query.Where(v => v.HotelID == hotelId);
            }
            if (state != 0)
            {
                query = query.Where(v => v.Stated == state);
            }
            if (stratDate == DateTime.MinValue)
                stratDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (endDate == DateTime.MinValue)
                endDate = stratDate.AddMonths(1).AddDays(-1);

            endDate = endDate.AddDays(1);
            int userId = LoginUserManage.GetInstance().GetLoginUserId();
            query = query.Where(v => v.CreateTime >= stratDate && v.CreateTime < endDate && v.CreateUserID == userId);


            var totalItemCount = query.Count();
            var orderingQuery = query.OrderByDescending(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from a in orderingQuery
                       join u in i_sys_user.Entities on a.CreateUserID equals u.UserID
                       join h in i_region.Entities on a.HotelID equals h.ID
                       join u1 in i_sys_user.Entities on a.ReviewUserID equals u1.UserID
                        into u1_join
                       from x in u1_join.DefaultIfEmpty()
                       select new
                       {
                           a.BID,
                           a.BNo,
                           a.CreateTime,
                           CreateUserName = u.UName,
                           HotelName = h.RegionName,
                           a.ReviewTime,
                           ReviewUserName = x == null ? "" : x.UName,
                           a.ExecTime,
                           a.Stated,
                           StateName = a.Stated == 1 ? "待审核" : (a.Stated == 2 ? "待出库" : (a.Stated == 3 ? "已出库" : (a.Stated == 4 ? "完成" : (a.Stated == 5 ? "终止" : "审核未通过"))))
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

        public ActionResult Apply()
        {
            List<region> hotelList = LoginUserManage.GetInstance().GetCurrentDataView().Where(v => v.RegionType == 1 && v.IsDelete == false).ToList();
            ViewData["HotelList"] = hotelList;

            List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false && v.IsRFID == true).ToList();
            ViewData["ClassList"] = classList;

            ViewData["BID"] = Request.Params["bid"];

            return View();
        }

        public ActionResult GetSizeByClassId(int classId)
        {
            MsgModel msgModel = new MsgModel();

            var query = from s in i_size.Entities
                        join sc in i_classsize.Entities on s.ID equals sc.SizeID
                        where sc.ClassID == classId
                        orderby s.Sort
                        select new
                        {
                            s.ID,
                            s.SizeName,
                            s.Sort
                        };

            msgModel.Result = query.ToList();

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult AddApply(FormCollection f)
        {
            MsgModel msgModel = new MsgModel();

            string no = LoginUserManage.GetInstance().GetBusInvoiceNuber().ToString().PadLeft(4, '0');

            no = string.Format("XQ{0}{1}", DateTime.Now.ToString("yyyyMMdd"), no);

            Guid guid = Guid.NewGuid();
            businessinvoice busModel = new businessinvoice();
            busModel.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            busModel.CreateTime = DateTime.Now;
            busModel.HotelID = Convert.ToInt32(f["HotelID"]);
            busModel.InvType = 1;
            busModel.SubType = Convert.ToInt16(f["SubType"]);
            busModel.Note = f["Note"];
            busModel.Stated = 1;
            busModel.BID = guid.ToString();
            busModel.BNo = no;

            List<businessdetail> invData = new List<businessdetail>();
            businessdetail detailModel = null;

            int dataLength = Convert.ToInt32(f["DataLength"]);
            for (int i = 0; i < dataLength; i++)
            {
                detailModel = new businessdetail();
                detailModel.ClassID = Convert.ToInt32(f[string.Format("Data[{0}][ClassID]", i)]);
                detailModel.ClassName = f[string.Format("Data[{0}][ClassName]", i)];
                detailModel.SizeID = f[string.Format("Data[{0}][SizeID]", i)] == "" ? 0 : Convert.ToInt32(f[string.Format("Data[{0}][SizeID]", i)]);
                detailModel.SizeName = f[string.Format("Data[{0}][SizeName]", i)];
                detailModel.TextileCount = Convert.ToInt32(f[string.Format("Data[{0}][TextileCount]", i)]);
                detailModel.BID = guid.ToString();
                detailModel.BNo = no;
                detailModel.HotelID = busModel.HotelID;
                detailModel.InvType = 1;
                detailModel.SubType = busModel.SubType;
                detailModel.ExecCount = 0;

                invData.Add(detailModel);
            }

            i_businessinvoice.Insert(busModel, false);
            i_bussinesdetail.Insert(invData);


            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult UpdateApply(FormCollection f)
        {
            MsgModel msgModel = new MsgModel();

            businessinvoice busModel = i_businessinvoice.GetByKey(f["BID"]);
            if (busModel != null)
            {
                if (busModel.Stated == 1)
                {
                    busModel.HotelID = Convert.ToInt32(f["HotelID"]);
                    busModel.SubType = Convert.ToInt16(f["SubType"]);
                    busModel.Note = f["Note"];

                    List<businessdetail> invData = new List<businessdetail>();
                    businessdetail detailModel = null;

                    int dataLength = Convert.ToInt32(f["DataLength"]);
                    for (int i = 0; i < dataLength; i++)
                    {
                        detailModel = new businessdetail();
                        detailModel.ClassID = Convert.ToInt32(f[string.Format("Data[{0}][ClassID]", i)]);
                        detailModel.ClassName = f[string.Format("Data[{0}][ClassName]", i)];
                        detailModel.SizeID = f[string.Format("Data[{0}][SizeID]", i)] == "" ? 0 : Convert.ToInt32(f[string.Format("Data[{0}][SizeID]", i)]);
                        detailModel.SizeName = f[string.Format("Data[{0}][SizeName]", i)];
                        detailModel.TextileCount = Convert.ToInt32(f[string.Format("Data[{0}][TextileCount]", i)]);
                        detailModel.BID = busModel.BID;
                        detailModel.BNo = busModel.BNo;
                        detailModel.HotelID = busModel.HotelID;
                        detailModel.InvType = 1;
                        detailModel.SubType = busModel.SubType;
                        detailModel.ExecCount = 0;

                        invData.Add(detailModel);
                    }

                    i_businessinvoice.Update(busModel, false);
                    i_bussinesdetail.Delete(p => p.BID == busModel.BID, false);
                    i_bussinesdetail.Insert(invData);

                }
                else
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "当前申请当状态已经发生改变，无法进行修改.";
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "请勿非法操作.";
            }


            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult DelApply(string bid)
        {
            MsgModel msgModel = new MsgModel();

            businessinvoice invModel = i_businessinvoice.GetByKey(bid);
            if (invModel != null)
            {
                if (invModel.Stated == 1)
                {
                    i_bussinesdetail.Delete(v => v.BID == bid, false);
                    i_businessinvoice.Delete(invModel);
                    msgModel.ResultCode = 0;
                }
                else
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "当前申请单状态发生改变，无法删除.";
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "非法请求.";
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult GetApplyById(string bid)
        {
            MsgModel msgModel = new MsgModel();

            businessinvoice invModel = i_businessinvoice.GetByKey(bid);
            List<businessdetail> invDetail = i_bussinesdetail.Entities.Where(p => p.BID == bid).ToList();

            var data = from i in i_businessinvoice.Entities
                       join r in i_region.Entities on i.HotelID equals r.ID
                       join u in i_sys_user.Entities on i.CreateUserID equals u.UserID
                       where i.BID == bid
                       select new
                       {
                           i.BID,
                           i.BNo,
                           i.CreateTime,
                           i.CreateUserID,
                           u.UName,
                           i.ExecTime,
                           i.HotelID,
                           r.RegionName,
                           i.InvType,
                           i.Note,
                           i.ReviewTime,
                           i.Stated,
                           StateName = i.Stated == 1 ? "待审核" : (i.Stated == 2 ? "待出库" : (i.Stated == 3 ? "已出库" : (i.Stated == 4 ? "完成" : "终止"))),
                           i.SubType
                       };

            msgModel.Result = data.FirstOrDefault();
            msgModel.OtherResult = invDetail;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;

        }

        public ActionResult ReviewList()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            List<region> hotelList = LoginUserManage.GetInstance().GetCurrentDataView().Where(v => v.RegionType == 1 && v.IsDelete == false).ToList();
            ViewData["HotelList"] = hotelList;

            return View();
        }

        public ActionResult GetReviewList(ParamModel model)
        {
            int hotelId = 0;
            int.TryParse(model.qCondition1, out hotelId);
            int state = 0;
            int.TryParse(model.qCondition2, out state);
            DateTime stratDate = DateTime.MinValue;
            DateTime.TryParse(model.qCondition3, out stratDate);
            DateTime endDate = DateTime.MinValue;
            DateTime.TryParse(model.qCondition4, out endDate);

            var query = i_businessinvoice.Entities.Where(v => v.InvType == 1);
            if (hotelId != 0)
            {
                query = query.Where(v => v.HotelID == hotelId);
            }
            else
            {
                List<int> hotelList = LoginUserManage.GetInstance().GetCurrentDataView().Where(v => v.RegionType == 1 && v.IsDelete == false).Select(v => v.ID).ToList();
                query = query.Where(v => hotelList.Contains(v.HotelID));
            }
            if (state != 0)
            {
                query = query.Where(v => v.Stated == state);
            }
            if (stratDate == DateTime.MinValue)
                stratDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (endDate == DateTime.MinValue)
                endDate = stratDate.AddMonths(1).AddDays(-1);

            endDate = endDate.AddDays(1);
            query = query.Where(v => v.CreateTime >= stratDate && v.CreateTime < endDate);


            var totalItemCount = query.Count();
            var orderingQuery = query.OrderByDescending(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from a in orderingQuery
                       join u in i_sys_user.Entities on a.CreateUserID equals u.UserID
                       join h in i_region.Entities on a.HotelID equals h.ID
                       select new
                       {
                           a.BID,
                           a.BNo,
                           a.CreateTime,
                           CreateUserName = u.UName,
                           HotelName = h.RegionName,
                           a.ReviewTime,
                           a.ExecTime,
                           a.Stated,
                           StateName = a.Stated == 1 ? "待审核" : (a.Stated == 2 ? "待出库" : (a.Stated == 3 ? "已出库" : (a.Stated == 4 ? "完成" : (a.Stated == 5 ? "终止" : "审核未通过"))))
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

        public ActionResult PassApply(string bid)
        {
            MsgModel msgModel = new MsgModel();

            businessinvoice invModel = i_businessinvoice.GetByKey(bid);
            if (invModel != null)
            {
                if (invModel.Stated == 1)
                {
                    invModel.Stated = 2;
                    invModel.ReviewTime = DateTime.Now;
                    invModel.ReviewUserID = LoginUserManage.GetInstance().GetLoginUserId();
                    i_businessinvoice.Update(invModel);
                    msgModel.ResultCode = 0;
                }
                else
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "当前申请单状态发生改变，无法审核.";
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "非法请求.";
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }


        public ActionResult BackApply(string bid)
        {
            MsgModel msgModel = new MsgModel();

            businessinvoice invModel = i_businessinvoice.GetByKey(bid);
            if (invModel != null)
            {
                if (invModel.Stated == 1)
                {
                    invModel.Stated = 6;
                    invModel.ReviewTime = DateTime.Now;
                    invModel.ReviewUserID = LoginUserManage.GetInstance().GetLoginUserId();
                    i_businessinvoice.Update(invModel);
                    msgModel.ResultCode = 0;
                }
                else
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "当前申请单状态发生改变，无法审核.";
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "非法请求.";
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }
    }
}