﻿using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.KANBAN.Models;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.KANBAN.Controllers
{
    public class WarehouseController : Controller
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<textile> i_textile { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<invoice> i_invoice { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }

        [Dependency]
        public IRepository<invoicerfid> i_invoicerfid { get; set; }

        // GET: KANBAN/Warehouse
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Infactory()
        {
            return View();
        }

        public ActionResult GetInfactory()
        {
            DateTime begin = DateTime.Now.Date;
            DateTime end = DateTime.Now.AddDays(1).Date;

            var query = from d in i_invoicedetail.Entities.Where(c => c.InvType == 1 && c.InvCreateTime >= begin
                        && c.InvCreateTime < end)
                        join r in i_region.Entities on d.HotelID equals r.ID into j_region
                        from r in j_region.DefaultIfEmpty()
                        group d by new { t0 = r.RegionName, t1 = d.ClassName, t2 = d.SizeName } into m
                        orderby m.Key.t0, m.Key.t1, m.Key.t2
                        select new
                        {
                            RegionName = m.Key.t0,
                            ClassName = m.Key.t1 + (string.IsNullOrEmpty(m.Key.t2) ? "" : "(" + m.Key.t2 + ")"),
                            Quantity = m.Sum(c => c.TextileCount),
                        };
            
            JsonResult jr = new JsonResult();
            jr.Data = query;

            return jr;
        }

        public ActionResult Stock()
        {
            return View();
        }

        public ActionResult GetStock(int regionid)
        {
            var query = from t in i_textile.Entities.Where(c => c.RegionID == regionid)
                        join r in i_region.Entities on t.HotelID equals r.ID into j_region
                        from r in j_region.DefaultIfEmpty()
                        join b in i_brandtype.Entities on t.BrandID equals b.ID
                        join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                        join s in i_size.Entities on t.SizeID equals s.ID into j_size
                        from s in j_size.DefaultIfEmpty()
                        where r.RegionType == 3 && r.RegionMode == 1
                        group t by new { t0 = b.Sort, t1 = b.BrandName, t2 = tc.Sort, t3 = tc.ClassName, t4 = s.Sort, t5 = s.SizeName } into m
                        orderby m.Key.t0, m.Key.t1, m.Key.t2, m.Key.t3, m.Key.t4, m.Key.t5
                        select new
                        {
                            m.Key.t1,
                            ClassName = m.Key.t3 + (m.Key.t5 == "" || m.Key.t5 == null ? "" : "(" + m.Key.t5 + ")"),
                            Quantity = m.Count()
                        };

            List<StockModel> list = new List<StockModel>();

            StockModel sm1 = null;

            var q = query.ToList();
            for (int i = 0; i < q.Count; i += 2)
            {
                var item1 = q[i];

                sm1 = new StockModel();
                sm1.BrandName1 = item1.t1;
                sm1.ClassName1 = item1.ClassName;
                sm1.Quantity1 = item1.Quantity;
                list.Add(sm1);

                if (i + 1 < q.Count - 1)
                {
                    var item2 = q[i + 1];
                    sm1.BrandName2 = item2.t1;
                    sm1.ClassName2 = item2.ClassName;
                    sm1.Quantity2 = item2.Quantity;
                }
                else
                {
                    sm1.BrandName2 = "";
                    sm1.ClassName2 = "";
                    sm1.Quantity2 = 0;
                }
            }

            JsonResult jr = new JsonResult();
            jr.Data = list;

            return jr;
        }
    }
}