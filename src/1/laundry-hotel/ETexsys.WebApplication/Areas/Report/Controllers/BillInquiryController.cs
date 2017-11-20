using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Report.Models;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Report.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class BillInquiryController : Controller
    {
        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<invoice> i_invoice { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }

        [Dependency]
        public IRepository<invoiceattach> i_invoiceattach { get; set; }

        [Dependency]
        public IRepository<repeatop> i_repeatop { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<sys_user> i_user { get; set; }

        [Dependency]
        public IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }
        // GET: Report/BillInquiry
        public ActionResult Index()
        {
            List<region> hotelList = i_region.Entities.Where(v => v.RegionType == 1 && v.IsDelete == false).ToList();
            ViewData["HotelList"] = hotelList;
            return View();
        }

        public ActionResult GetBillInquiryList(SelectParamModel model)
        {
            var query = from i in i_invoice.Entities
                        join h in i_region.Entities on i.HotelID equals h.ID into i_hotel
                        from h in i_hotel.DefaultIfEmpty()
                        join r in i_region.Entities on i.RegionID equals r.ID into i_region
                        from r in i_region.DefaultIfEmpty()
                        select new
                        {
                            InvType = i.InvType,
                            InvNo = i.InvNo,
                            HotelName = h.RegionName,
                            Floorparam = r.RegionName,
                            CreateUserName = i.CreateUserName,
                            CreateTime = i.CreateTime,
                            Count = i.Quantity,
                        };
            if (model.InvType != 0)
            {
                query = query.Where(q => q.InvType == model.InvType);
            }
            if (model.Time != 0)
            {
                switch (model.Time)
                {
                    case 1:
                        DateTime end = DateTime.Now.AddDays(1);
                        DateTime begin = DateTime.Now.Date;
                        query = query.Where(q => q.CreateTime >= begin && q.CreateTime < end);
                        break;
                    case 2:
                        DateTime endWeek = DateTime.Now.AddDays(1);
                        DateTime beginWeek = DateTime.Now.AddDays(-7);
                        query = query.Where(q => q.CreateTime >= beginWeek && q.CreateTime < endWeek);
                        break;
                    case 3:
                        DateTime endMonth = DateTime.Now.AddDays(1);
                        DateTime beginMonth = DateTime.Now.AddMonths(-1);
                        query = query.Where(q => q.CreateTime > beginMonth && q.CreateTime < endMonth);
                        break;
                    case 4:
                        DateTime beginwhere = model.BeginTime;
                        DateTime endwhere = model.EndTime;
                        if (beginwhere == DateTime.MinValue)
                            beginwhere = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        if (endwhere == DateTime.MinValue)
                            endwhere = beginwhere.AddMonths(1).AddDays(-1);
                        if (model.HotelId == 0)
                        {
                            query = query.Where(q => q.CreateTime > beginwhere && q.CreateTime < endwhere);
                        }
                        else
                        {
                            if (model.Floor != null)
                            {
                                query = query.Where(q => q.CreateTime > beginwhere && q.CreateTime < endwhere && q.HotelName == model.Hotel && q.Floorparam == model.Floor);
                            }
                            else
                            {
                                query = query.Where(q => q.CreateTime > beginwhere && q.CreateTime < endwhere && q.HotelName == model.Hotel);
                            }
                        }
                        break;
                }
            }
            var totalItemCount = query.Count();
            var orderingQuery = query.OrderByDescending(v => v.CreateTime).Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();
            var JsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery
            };
            return Json(JsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFloor(int HotelId)
        {
            List<region> regionList = i_region.Entities.Where(p => p.ParentID == HotelId && p.IsDelete == false).ToList() as List<region>;
            return Json(regionList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSummaryList(SelectParamModel model)
        {
            var query = from d in i_invoicedetail.Entities
                        join i in i_invoice.Entities on d.InvNo equals i.InvNo into i_ti
                        from i in i_ti.DefaultIfEmpty()
                        join r in i_region.Entities on d.RegionID equals r.ID into i_tr
                        from r in i_tr.DefaultIfEmpty()
                        join h in i_region.Entities on d.HotelID equals h.ID into i_th
                        from h in i_th.DefaultIfEmpty()
                        select new
                        {
                            CreateTime = i.CreateTime,
                            InvType = i.InvType,
                            HotelName = h.RegionName,
                            FoolrName = r.RegionName,
                            ClassID = d.ClassID,
                            SizeId = d.SizeID,
                            TextileCount = d.TextileCount
                        };

            if (model.InvType != 0)
            {
                query = query.Where(q => q.InvType == model.InvType);
            }

            if (model.Time != 0)
            {
                switch (model.Time)
                {
                    case 1:
                        DateTime end = DateTime.Now;
                        DateTime begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        query = query.Where(q => q.CreateTime > begin && q.CreateTime < end);
                        break;
                    case 2:
                        DateTime endWeek = DateTime.Now.AddDays(1);
                        DateTime beginWeek = DateTime.Now.AddDays(-7);
                        query = query.Where(q => q.CreateTime >= beginWeek && q.CreateTime < endWeek);
                        break;
                    case 3:
                        DateTime endMonth = DateTime.Now.AddDays(1);
                        DateTime beginMonth = DateTime.Now.AddMonths(-1);
                        query = query.Where(q => q.CreateTime > beginMonth && q.CreateTime < endMonth);
                        break;
                    case 4:
                        DateTime beginwhere = model.BeginTime;
                        DateTime endwhere = model.EndTime;
                        if (beginwhere == DateTime.MinValue)
                            beginwhere = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        if (endwhere == DateTime.MinValue)
                            endwhere = beginwhere.AddMonths(1).AddDays(-1);
                        if (model.HotelId == 0)
                        {
                            query = query.Where(q => q.CreateTime > beginwhere && q.CreateTime < endwhere);
                        }
                        else
                        {
                            if (model.Floor != null)
                            {
                                query = query.Where(q => q.CreateTime > beginwhere && q.CreateTime < endwhere && q.HotelName == model.Hotel && q.FoolrName == model.Floor);
                            }
                            else
                            {
                                query = query.Where(q => q.CreateTime > beginwhere && q.CreateTime < endwhere && q.HotelName == model.Hotel);
                            }
                        }
                        break;
                }
            }
            var query1 = from q in query
                         join c in i_textileclass.Entities on q.ClassID equals c.ID into i_tc
                         from c in i_tc.DefaultIfEmpty()
                         join s in i_size.Entities on q.SizeId equals s.ID into i_ts
                         from s in i_ts.DefaultIfEmpty()
                         group q by new { t0 = c.Sort, t1 = c.ClassName, t2 = s.Sort, t3 = s.SizeName } into m
                         orderby m.Key.t0, m.Key.t1, m.Key.t2, m.Key.t3
                         select new
                         {
                             className = m.Key.t1,
                             sizeName = m.Key.t3,
                             textileCount = m.Sum(v => v.TextileCount)
                         };
            var totalItemCount = query1.Count();
            var orderingQuery = query1.OrderBy(v => v.textileCount).Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();
            var JsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery
            };
            return Json(JsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDetail(string InvNo)
        {
            MsgModel msg = new MsgModel();
            var list = from d in i_invoicedetail.Entities
                       join r in i_region.Entities on d.RegionID equals r.ID into j_region
                       from r in j_region.DefaultIfEmpty()
                       join i in i_invoice.Entities on d.InvID equals i.ID
                       where d.InvNo == InvNo
                       group d by new
                       {
                           d.InvNo,
                           d.InvSubType,
                           d.ClassName,
                           d.SizeName,
                           r.RegionName,
                           i.ConfirmTime,
                           i.ConfirmUserName,
                           i.Comfirmed,
                       }
                       into m
                       select new
                       {
                           InvNo = m.Key.InvNo,
                           ClassName = m.Key.ClassName,
                           SizeName = m.Key.SizeName,
                           RegionName = m.Key.RegionName,
                           m.Key.ConfirmTime,
                           m.Key.Comfirmed,
                           m.Key.ConfirmUserName,
                           Normal = m.Sum(c => c.InvSubType == 1 ? c.TextileCount : 0),
                           Dirty = m.Sum(c => c.InvSubType == 2 ? c.TextileCount : 0),
                           BackWash = m.Sum(c => c.InvSubType == 3 ? c.TextileCount : 0),
                           Guoshui = m.Sum(c => c.InvSubType == 4 ? c.TextileCount : 0),
                           TextileCount = m.Sum(c => c.TextileCount),
                       };
            msg.OtherResult = list.ToList().FirstOrDefault();
            msg.Result = list.ToList();
            return Json(msg, JsonRequestBehavior.AllowGet);
        }
    }
}