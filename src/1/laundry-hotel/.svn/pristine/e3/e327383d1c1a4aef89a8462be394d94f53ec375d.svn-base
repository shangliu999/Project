using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Dashboard.Models;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Dashboard.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class FactoryController : Controller
    {
        [Dependency]
        public IRepository<bag> i_bag { get; set; }

        [Dependency]
        public IRepository<truck> i_truck { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<textile> i_textile { get; set; }

        [Dependency]
        public IRepository<scrapdetail> i_scrapdetail { get; set; }

        [Dependency]
        public IRepository<scrap> i_scrap { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<category> i_category { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }

        // GET: Dashboard/Factory
        public ActionResult Index()
        {
            DateTime time = DateTime.Now.AddDays(-60);
            DateTime dtime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            DashboardBaseModel dashboard = new DashboardBaseModel();
            dashboard.BagTotal = i_bag.Entities.Where(v => v.IsDelete == false).Count();
            dashboard.TruckTotal = i_truck.Entities.Count();
            dashboard.TextileTotal = i_textile.Entities.Where(v => v.IsFlag == 1).Count();
            dashboard.AbnormalTextileTotal = i_textile.Entities.Where(v => v.IsFlag == 1 && v.UpdateTime < time).Count();
            dashboard.HotelTotal = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 1).Count();
            dashboard.CurrentMonthHotelTotal = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 1 && v.CreateTime >= dtime).Count();

            return View(dashboard);
        }

        public ActionResult ScrapChatData(int type)
        {
            MsgModel msgModel = new MsgModel();

            DateTime time = DateTime.Now;
            if (type == 1)
            {
                time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            else
            {
                time = new DateTime(DateTime.Now.Year, 1, 1);
            }

            var query = from d in i_scrapdetail.Entities
                        join s in i_scrap.Entities on d.ScrapID equals s.ID
                        where d.CreateTime >= time
                        group d by new { d.ClassID, d.ClassName, s.ScrapType } into m
                        select new { m.Key.ScrapType, m.Key.ClassName, m.Key.ClassID, TextileCount = m.Count() };

            var classQuery = i_textileclass.Entities.Where(v => v.IsDelete == false && v.IsRFID == true).OrderBy(v => v.Sort).ThenBy(v => v.ClassName);

            List<string> className = new List<string>();
            List<int> scrapTotal = new List<int>();
            List<int> abScrapTotal = new List<int>();
            classQuery.ToList().ForEach(q =>
            {
                className.Add(q.ClassName);

                var t = query.Where(v => v.ClassID == q.ID);
                int total = 0;
                int abtotal = 0;
                t.ToList().ForEach(_ =>
                {
                    if (_.ScrapType == 2)
                    {
                        abtotal += _.TextileCount;
                    }
                    total += _.TextileCount;
                });
                scrapTotal.Add(total);
                abScrapTotal.Add(abtotal);
            });

            DashboardScrapModel model = new DashboardScrapModel();
            model.ClassName = className;
            model.ScrapTotal = scrapTotal;
            model.ABScrapTotal = abScrapTotal;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult TextileDistrbution()
        {
            MsgModel msgModel = new MsgModel();
            List<DashboardTextileDistrbutionModel> list = new List<DashboardTextileDistrbutionModel>();
            DashboardTextileDistrbutionModel model = null;

            List<category> cateList = i_category.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.CateName).Take(2).ToList();


            foreach (var item in cateList)
            {
                model = new DashboardTextileDistrbutionModel();
                model.CateName = item.CateName;
                model.LegendList = new List<string>();
                model.TextileData = new List<PieChatModel>();

                List<int> classIds = i_textileclass.Entities.Where(v => v.IsDelete == false && v.CateID == item.ID).Select(v => v.ID).ToList();

                List<int> state = new List<int>();
                state.Add(2);
                state.Add(9);
                int hotelTotal = i_textile.Entities.Where(t => t.IsFlag == 1 && classIds.Contains(t.ClassID) && state.Contains(t.TextileState)).Count();
                //酒店中
                model.TextileData.Add(new PieChatModel { name = "酒店", value = hotelTotal.ToString() });
                model.LegendList.Add("酒店");


                List<region> regionList = i_region.Entities.Where(v => v.IsDelete == false).ToList();
                state = new List<int>();
                state.Add(0);
                state.Add(3);
                var query = from t in i_textile.Entities
                            where t.IsFlag == 1 && classIds.Contains(t.ClassID) && state.Contains(t.TextileState)
                            group t by new { t.RegionID } into m
                            select new { m.Key.RegionID, TextileCount = m.Count() };

                query.ToList().ForEach(q =>
                {
                    var storeModel = regionList.Where(v => v.ID == q.RegionID).FirstOrDefault();
                    model.LegendList.Add(storeModel.RegionName);
                    model.TextileData.Add(new Models.PieChatModel { name = storeModel.RegionName, value = q.TextileCount.ToString() });
                });

                state = new List<int>();
                state.Add(0);
                state.Add(3);
                state.Add(2);
                state.Add(9);

                int factoryTotal = i_textile.Entities.Where(t => t.IsFlag == 1 && classIds.Contains(t.ClassID) && state.Contains(t.TextileState)).Count();

                model.TextileData.Add(new PieChatModel { name = "工厂", value = factoryTotal.ToString() });
                model.LegendList.Add("工厂");

                list.Add(model);
            }

            msgModel.Result = list;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult WashingData(int type)
        {
            MsgModel msgModel = new MsgModel();

            DashboardWashingModel model = new DashboardWashingModel();
            model.xAxis = new List<string>();
            model.LegendList = new List<string>();

            DateTime time = DateTime.Now;

            if (type == 1)
            {
                DateTime temptime = time.AddDays(-7);

                while (temptime.Date < time)
                {
                    model.xAxis.Add(temptime.Day.ToString());
                    temptime = temptime.AddDays(1);
                }
                time = DateTime.Now.AddDays(-7);
            }
            else
            {
                DateTime temptime = time.AddMonths(-1);

                while (temptime.Date < time)
                {
                    model.xAxis.Add(temptime.Day.ToString());
                    temptime = temptime.AddDays(1);
                }
                time = DateTime.Now.AddMonths(-1);
            }

            List<invoicedetail> InvoiceList = i_invoicedetail.Entities.Where(v => v.InvType == 1 && v.InvCreateTime >= time.Date).ToList();

            List<category> cateList = i_category.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.CateName).Take(2).ToList();

            ChatSeriesModel seriesModel = null;

            int[] total = new int[model.xAxis.Count];

            foreach (var item in cateList)
            {
                model.LegendList.Add(item.CateName);
                model.Series = new List<ChatSeriesModel>();
                seriesModel = new ChatSeriesModel();
                seriesModel.name = item.CateName;

                List<int> classIds = i_textileclass.Entities.Where(v => v.IsDelete == false && v.CateID == item.ID).Select(v => v.ID).ToList();

                DateTime temptime = time;
                List<double> d = new List<double>();

                int i = 0;
                while (temptime.Date < DateTime.Now)
                {
                    var q = InvoiceList.Where(v => classIds.Contains(v.ClassID) && v.InvCreateTime >= temptime.Date && v.InvCreateTime < temptime.AddDays(1).Date).Sum(v => v.TextileCount);
                    total[i] += q;
                    d.Add(q);

                    temptime = temptime.AddDays(1);
                    i++;
                }

                seriesModel.data = d;
                model.Series.Add(seriesModel);
            }

            #region 返洗率

            model.LegendList.Add("返洗率");
            seriesModel = new ChatSeriesModel();
            seriesModel.name = "返洗率";


            DateTime ttime = time;
            List<double> bd = new List<double>();
            int index = 0;
            while (ttime.Date < DateTime.Now)
            {
                var q = InvoiceList.Where(v => v.InvSubType == 3 && v.InvCreateTime >= ttime.Date && v.InvCreateTime < ttime.AddDays(1).Date).Sum(v => v.TextileCount);

                if (total[index] != 0 && q != 0)
                {
                    bd.Add(Math.Round(((double)q / (double)total[index]) * 100, 2));
                }
                else
                {
                    bd.Add(0);
                }

                ttime = ttime.AddDays(1);
                index++;
            }

            seriesModel.data = bd;
            model.Series.Add(seriesModel);

            #endregion

            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

    }
}