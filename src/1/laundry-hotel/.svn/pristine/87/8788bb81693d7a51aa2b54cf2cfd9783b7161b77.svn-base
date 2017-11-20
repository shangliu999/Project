using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.KANBAN.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.KANBAN.Controllers
{
    public class FactoryController : Controller
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
        public IRepository<C_TextileSummary> i_C_TextileSummary { get; set; }

        // GET: KANBAN/Factory
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

            //从租赁仓库过来就要显示出来
            var query = from t in i_invoicedetail.Entities.Where(c => ((c.InvType == 4 && c.InvSubType == 21) || (c.InvType == 3 && c.InvSubType == 24))
                        && c.InvCreateTime >= begin && c.InvCreateTime < end)
                        join tc in i_textileclass.Entities on t.ClassID equals tc.ID
                        join s in i_size.Entities on t.SizeID equals s.ID into j_size
                        from s in j_size.DefaultIfEmpty()
                        group t by new { t0 = tc.Sort, t1 = tc.ClassName, t2 = s.Sort, t3 = s.SizeName } into m
                        orderby m.Key.t0, m.Key.t1, m.Key.t2, m.Key.t3
                        select new
                        {
                            ClassName = m.Key.t1 + (m.Key.t3 == "" || m.Key.t3 == null ? "" : "(" + m.Key.t3 + ")"),
                            Quantity = m.Sum(c => c.InvType == 4 ? c.TextileCount : 0),
                            Washing = 0,
                            Washed = m.Sum(c => c.InvType == 3 ? c.TextileCount : 0),
                        };

            //laundry_hotelEntities entities = new laundry_hotelEntities();
            //entities.Database.SqlQuery();

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT CONCAT(tc.classname,CASE WHEN s.sizename is NULL THEN '' WHEN s.sizename IS NOT NULL THEN CONCAT('(',s.sizename,')') END) AS ClassName,");
            sql.Append("COUNT(t.ID) AS TextileCount FROM textile AS t LEFT JOIN textileclass AS tc ON t.classid=tc.id LEFT JOIN size as s on t.SizeID=s.id ");
            sql.AppendFormat("LEFT JOIN {0}.rfid_scan_record AS rsr ON t.tagno=rsr.epc ", System.Configuration.ConfigurationManager.AppSettings["FlowDBName"]);
            sql.AppendFormat("WHERE IsFlag=1 AND textilestate<>3 AND rsr.`Port`=1 AND rsr.ScanTime>='{1}' AND rsr.ScanTime<'{2}'",
                System.Configuration.ConfigurationManager.AppSettings["FlowDBName"], begin.ToString("yyyy/MM/dd"), end.ToString("yyyy/MM/dd"));
            sql.Append("GROUP BY tc.classname,s.sizename");

            List<C_TextileSummary> list = i_C_TextileSummary.SQLQuery(sql.ToString(), "").ToList();

            //匹配类型
            List<string> classnames = new List<string>();
            List<WashingModel> result = new List<WashingModel>();
            WashingModel wm = null;
            query.ToList().ForEach(c =>
            {
                wm = new WashingModel();
                wm.ClassName = c.ClassName;
                wm.Quantity = c.Quantity;

                C_TextileSummary cts = list.FirstOrDefault(k => k.ClassName == c.ClassName);
                if (cts != null)
                {
                    wm.Washing = c.Washing;
                    classnames.Add(c.ClassName);
                }
                wm.Washed = c.Washed;

                result.Add(wm);
            });

            List<C_TextileSummary> other = list.Where(c => !classnames.Contains(c.ClassName)).ToList();
            for (int i = 0; i < other.Count; i++)
            {
                wm = new WashingModel();
                wm.ClassName = other[i].ClassName;
                wm.Quantity = 0;
                wm.Washing = other[i].TextileCount;
                wm.Washed = 0;

                result.Add(wm);
            }

            JsonResult jr = new JsonResult();
            jr.Data = result;

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
                        where r.RegionType == 3 && r.RegionMode == 1
                        group t by new { t0 = b.Sort, t1 = b.BrandName, t2 = tc.Sort, t3 = tc.ClassName } into m
                        orderby m.Key.t0, m.Key.t1, m.Key.t2, m.Key.t3
                        select new
                        {
                            m.Key.t1,
                            m.Key.t3,
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
                sm1.ClassName1 = item1.t3;
                sm1.Quantity1 = item1.Quantity;
                list.Add(sm1);

                if (i + 1 < q.Count - 1)
                {
                    var item2 = q[i + 1];
                    sm1.BrandName2 = item2.t1;
                    sm1.ClassName2 = item2.t3;
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