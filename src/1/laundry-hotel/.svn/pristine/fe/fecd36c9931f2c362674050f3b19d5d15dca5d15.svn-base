using ETexsys.Common.Rabbit;
using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Customer.Models;
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
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class OrderController : Controller
    {
        private static readonly object invLcker = new object();

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<sys_user> i_sys_user { get; set; }

        [Dependency]
        public IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }

        [Dependency]
        public IRepository<invoice> i_invoice { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }


        [Dependency]
        public IRepository<businessdetail> i_bussinesdetail { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        [Dependency]
        public IRepository<sys_customer> i_sys_customer { get; set; }

        // GET: Customer/Order
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            List<region> hotelList = LoginUserManage.GetInstance().GetCurrentDataView().Where(v => v.RegionType == 1 && v.IsDelete == false).ToList();
            ViewData["HotelList"] = hotelList;

            return View();
        }

        public ActionResult GetOrderList(ParamModel model)
        {
            int hotelId = 0;
            int.TryParse(model.qCondition1, out hotelId);

            int userId = LoginUserManage.GetInstance().GetLoginUserId();

            DateTime stratDate = DateTime.MinValue;
            DateTime.TryParse(model.qCondition3, out stratDate);
            DateTime endDate = DateTime.MinValue;
            DateTime.TryParse(model.qCondition4, out endDate);

            var userquery = i_sys_user_dataview.Entities.Where(c => c.UserID == userId);
            var query = i_invoice.Entities.Where(v => v.InvType == 6);
            if (hotelId != 0)
            {
                query = query.Where(v => v.HotelID == hotelId);
                userquery = userquery.Where(c => c.RegionID == hotelId);
            }
            if (stratDate == DateTime.MinValue)
                stratDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (endDate == DateTime.MinValue)
                endDate = stratDate.AddMonths(1).AddDays(-1);

            endDate = endDate.AddDays(1);

            var regionids = userquery.Select(c => c.RegionID);
            query = query.Where(v => v.CreateTime >= stratDate && v.CreateTime < endDate && regionids.Contains(v.HotelID));

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderByDescending(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from a in orderingQuery
                       join u in i_sys_user.Entities on a.CreateUserID equals u.UserID
                       join h in i_region.Entities on a.HotelID equals h.ID
                       join r in i_region.Entities on a.RegionID equals r.ID
                       select new
                       {
                           a.ID,
                           a.InvNo,
                           a.InvState,
                           a.CreateTime,
                           CreateUserName = u.UName,
                           HotelName = h.RegionName,
                           RegionName = r.RegionName,
                           a.Quantity
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

        public ActionResult GetOrderById(string bid)
        {
            MsgModel msgModel = new MsgModel();

            invoice invModel = i_invoice.GetByKey(bid);
            List<invoicedetail> invDetail = i_invoicedetail.Entities.Where(p => p.InvID == bid).ToList();

            var data = from i in i_invoice.Entities
                       join r in i_region.Entities on i.HotelID equals r.ID
                       join u in i_sys_user.Entities on i.CreateUserID equals u.UserID
                       where i.ID == bid
                       select new
                       {
                           i.ID,
                           i.InvNo,
                           i.CreateTime,
                           i.CreateUserID,
                           u.UName,
                           i.HotelID,
                           r.RegionName,
                           i.InvType,
                           i.Remrak,
                       };

            msgModel.Result = data.FirstOrDefault();
            msgModel.OtherResult = invDetail;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult Create(string id)
        {
            List<OrderDetailsModel> list = new List<OrderDetailsModel>();

            IQueryable<invoicedetail> details = null;
            if (!string.IsNullOrEmpty(id))
            {
                details = i_invoicedetail.Entities.Where(c => c.InvID == id);
            }

            var data = from a in i_textileclass.Entities.Where(c => c.IsRFID == true && c.IsDelete == false)
                       join cs in i_classsize.Entities on a.ID equals cs.ClassID
                       into cs_join
                       from y in cs_join.DefaultIfEmpty()
                       join s in i_size.Entities.Where(c => c.IsDelete == false) on y.SizeID equals s.ID
                       into u1_join
                       from x in u1_join.DefaultIfEmpty()
                       orderby a.Sort, a.ClassName, x.SizeName
                       select new
                       {
                           a.ID,
                           a.ClassName,
                           SizeID = x == null ? 0 : x.ID,
                           SizeName = x == null ? "" : "(" + x.SizeName + ")"
                       };

            int hotelid, regionid;
            hotelid = regionid = 0;

            OrderDetailsModel item = null;
            invoicedetail idetail = null;
            data.ToList().ForEach(c =>
            {
                item = new OrderDetailsModel();
                item.AreaID = c.ID;
                item.SizeID = c.SizeID;
                item.TextileName = c.ClassName;
                item.SizeName = c.SizeName;

                if (details != null)
                {
                    idetail = details.SingleOrDefault(v => v.ClassID == c.ID && v.SizeID == c.SizeID);
                    if (idetail != null && idetail.TextileCount > 0)
                    {
                        item.Count = idetail.TextileCount.ToString();
                    }
                    else
                    {
                        item.Count = "";
                    }

                    if (hotelid == 0 || regionid == 0)
                    {
                        hotelid = details.FirstOrDefault().HotelID;
                        regionid = details.FirstOrDefault().RegionID;
                    }
                }

                list.Add(item);
            });

            List<region> hotelList = LoginUserManage.GetInstance().GetCurrentDataView().Where(v => v.RegionType == 1 && v.IsDelete == false).ToList();
            ViewData["HotelList"] = hotelList;

            ViewData["ID"] = id;
            ViewData["HotelID"] = hotelid;
            ViewData["RegionID"] = regionid;

            return View(list);
        }

        public ActionResult Add(OrderModel model)
        {
            MsgModel msgModel = new MsgModel();

            string px = GetInvoicePX(6);
            int nubmer = GetInvoiceNubmer();
            string invNo = string.Format("{0}{1}{2}", px, DateTime.Now.ToString("yyyyMMdd"), nubmer.ToString().PadLeft(6, '0'));
            DateTime time = DateTime.Now;

            invoice inv = new invoice();
            Guid id = Guid.NewGuid();
            inv.ID = id.ToString();
            inv.InvNo = invNo;
            inv.InvType = 6;
            inv.InvSubType = 0;
            inv.TaskType = 0;
            inv.DataType = 2;
            inv.HotelID = model.HotelID;
            inv.RegionID = model.RegionID;
            inv.Quantity = model.Quantity;
            inv.Remrak = model.Remrak;
            inv.InvState = 1;     // 1：新建 2：审核通过 3：审核未通过 4：已配送
            inv.Comfirmed = 0;
            inv.Processed = false;
            inv.PaymentStatus = 0;

            sys_user user = LoginUserManage.GetInstance().GetLoginUser();

            inv.CreateUserID = user.UserID;
            inv.CreateUserName = string.IsNullOrEmpty(user.UName) ? "" : user.UName;
            inv.CreateTime = time;
            inv.MacCode = "Web";
            inv.Comfirmed = 0;
            inv.Processed = false;

            var q = from t in i_brandtype.Entities
                    join h in i_region.Entities on t.ID equals h.BrandID
                    into h_join
                    from h in h_join.DefaultIfEmpty()
                    where h.ID == model.HotelID
                    select new { t.ID, t.BrandName };

            var brandModel = q.FirstOrDefault();

            List<invoicedetail> detailList = new List<invoicedetail>();
            invoicedetail detailModel = null;

            OrderDetailsModel item = null;
            for (int i = 0; i < model.Data.Count; i++)
            {
                item = model.Data[i];

                detailModel = new invoicedetail();
                detailModel.InvID = inv.ID;
                detailModel.InvNo = invNo;
                detailModel.InvType = 6;
                detailModel.InvSubType = 0;
                detailModel.HotelID = model.HotelID;
                detailModel.RegionID = model.RegionID;
                detailModel.ClassID = item.AreaID;
                detailModel.ClassName = item.TextileName;
                detailModel.SizeID = item.SizeID;
                detailModel.SizeName = item.SizeID == 0 ? "" : item.SizeName.Trim('(', ')');
                detailModel.TextileCount = int.Parse(item.Count);
                detailModel.InvCreateTime = time;
                detailModel.BrandID = brandModel == null ? 0 : brandModel.ID;
                detailModel.BrandName = brandModel == null ? "" : brandModel.BrandName;

                detailList.Add(detailModel);
            }

            i_invoice.Insert(inv, false);
            i_invoicedetail.Insert(detailList);

            JsonResult jr = new JsonResult();
            jr.Data = msgModel;

            return jr;
        }

        public ActionResult Update(OrderModel model)
        {
            MsgModel msgModel = new MsgModel();

            invoice invModel = i_invoice.GetByKey(model.ID);

            if (invModel != null)
            {
                if (invModel.InvState != 1)
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "该单据已审核，无法修改。";
                }
                else
                {
                    DateTime time = DateTime.Now;

                    var q = from t in i_brandtype.Entities
                            join h in i_region.Entities on t.ID equals h.BrandID
                            into h_join
                            from h in h_join.DefaultIfEmpty()
                            where h.ID == invModel.HotelID
                            select new { t.ID, t.BrandName };

                    var brandModel = q.FirstOrDefault();

                    List<invoicedetail> detailList = new List<invoicedetail>();
                    invoicedetail detailModel = null;

                    OrderDetailsModel item = null;
                    for (int i = 0; i < model.Data.Count; i++)
                    {
                        item = model.Data[i];

                        detailModel = new invoicedetail();
                        detailModel.InvID = invModel.ID;
                        detailModel.InvNo = invModel.InvNo;
                        detailModel.InvType = 6;
                        detailModel.InvSubType = 0;
                        detailModel.HotelID = invModel.HotelID;
                        detailModel.RegionID = invModel.RegionID;
                        detailModel.ClassID = item.AreaID;
                        detailModel.ClassName = item.TextileName;
                        detailModel.SizeID = item.SizeID;
                        detailModel.SizeName = item.SizeID == 0 ? "" : item.SizeName.Trim('(', ')');
                        detailModel.TextileCount = int.Parse(item.Count);
                        detailModel.InvCreateTime = time;
                        detailModel.BrandID = brandModel == null ? 0 : brandModel.ID;
                        detailModel.BrandName = brandModel == null ? "" : brandModel.BrandName;

                        detailList.Add(detailModel);
                    }
                    i_invoicedetail.Delete(c => c.InvID == invModel.ID, false);

                    i_invoicedetail.Insert(detailList, false);
                    invModel.Quantity = model.Quantity;
                    invModel.UpdateTime = time;
                    invModel.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                    i_invoice.Update(invModel);
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "非法请求.";
            }

            JsonResult jr = new JsonResult();
            jr.Data = msgModel;

            return jr;
        }

        public ActionResult DelOrder(string bid)
        {
            MsgModel msgModel = new MsgModel();

            invoice invModel = i_invoice.GetByKey(bid);
            if (invModel != null)
            {
                if (invModel.InvState != 1)
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "该单据已审核，无法删除。";
                }
                else
                {
                    i_invoicedetail.Delete(v => v.InvID == bid, false);
                    i_invoice.Delete(invModel);
                    msgModel.ResultCode = 0;
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

            int userId = LoginUserManage.GetInstance().GetLoginUserId();

            DateTime stratDate = DateTime.MinValue;
            DateTime.TryParse(model.qCondition3, out stratDate);
            DateTime endDate = DateTime.MinValue;
            DateTime.TryParse(model.qCondition4, out endDate);

            var userquery = i_sys_user_dataview.Entities.Where(c => c.UserID == userId);
            var query = i_invoice.Entities.Where(v => v.InvType == 6);
            if (hotelId != 0)
            {
                query = query.Where(v => v.HotelID == hotelId);
                userquery = userquery.Where(c => c.RegionID == hotelId);
            }
            if (stratDate == DateTime.MinValue)
                stratDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (endDate == DateTime.MinValue)
                endDate = stratDate.AddMonths(1).AddDays(-1);

            endDate = endDate.AddDays(1);

            var regionids = userquery.Select(c => c.RegionID);
            query = query.Where(v => v.CreateTime >= stratDate && v.CreateTime < endDate && regionids.Contains(v.HotelID));

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderByDescending(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from a in orderingQuery
                       join u in i_sys_user.Entities on a.CreateUserID equals u.UserID
                       join h in i_region.Entities on a.HotelID equals h.ID
                       join r in i_region.Entities on a.RegionID equals r.ID
                       select new
                       {
                           a.ID,
                           a.InvNo,
                           a.InvState,
                           a.CreateTime,
                           CreateUserName = u.UName,
                           HotelName = h.RegionName,
                           RegionName = r.RegionName,
                           a.Quantity,
                           a.ConfirmUserName,
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


        public ActionResult PassApply(string id)
        {
            MsgModel msgModel = new MsgModel();

            invoice invModel = i_invoice.GetByKey(id);
            if (invModel != null)
            {
                if (invModel.InvState == 1)
                {
                    invModel.InvState = 2;
                    invModel.ConfirmTime = DateTime.Now;
                    invModel.ConfirmUserID = LoginUserManage.GetInstance().GetLoginUserId();
                    invModel.ConfirmUserName = LoginUserManage.GetInstance().GetLoginUser().UName;
                    i_invoice.Update(invModel);

                    //审核通过才生成任务
                    sys_customer sc = i_sys_customer.Entities.FirstOrDefault();
                    string code = sc != null ? sc.Code : "A";

                    RabbitMQModel mqModel = new RabbitMQModel();
                    mqModel.Type = "Invoice";
                    mqModel.Value = id.ToString();
                    mqModel.Code = code;
                    RabbitMQHelper.Enqueue<RabbitMQModel>(mqModel);

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

            invoice invModel = i_invoice.GetByKey(bid);
            if (invModel != null)
            {
                if (invModel.InvState == 1)
                {
                    invModel.InvState = 3;
                    invModel.ConfirmUserID = LoginUserManage.GetInstance().GetLoginUserId();
                    invModel.ConfirmUserName = LoginUserManage.GetInstance().GetLoginUser().UName;
                    i_invoice.Update(invModel);
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

        /// <summary>
        /// 根据酒店ID获取楼层信息
        /// </summary>
        /// <param name="hotelID">酒店ID</param>
        /// <returns></returns>
        public JsonResult GetRegionList(int hotelID)
        {
            List<region> regionList = LoginUserManage.GetInstance().GetCurrentDataView().Where(v => v.ParentID == hotelID && v.RegionType == 2 && v.IsDelete == false).ToList();

            JsonResult jr = new JsonResult();
            jr.Data = regionList;

            return jr;
        }

        private string GetInvoicePX(int invType)
        {
            string px = string.Empty;
            switch (invType)
            {
                case 1:
                    px = "SH";
                    break;
                case 2:
                    px = "PH";
                    break;
                case 3:
                    px = "RK";
                    break;
                case 4:
                    px = "CK";
                    break;
                case 5:
                    px = "RC";
                    break;
                case 6:
                    px = "DD";
                    break;
                case 7:
                    px = "TH";
                    break;
                default: break;
            }
            return px;
        }

        private int GetInvoiceNubmer()
        {
            lock (invLcker)
            {
                int rtn = 1;

                if (MvcApplication.InvoiceTime.ToString("yyyy/MM/dd") != DateTime.Now.ToString("yyyy/MM/dd"))
                {
                    MvcApplication.InvoiceTime = DateTime.Now;
                    MvcApplication.InvoiceNubmer = rtn;
                }
                else
                {
                    rtn = MvcApplication.InvoiceNubmer + 1;
                    MvcApplication.InvoiceNubmer = rtn;
                }
                return rtn;
            }
        }
    }
}