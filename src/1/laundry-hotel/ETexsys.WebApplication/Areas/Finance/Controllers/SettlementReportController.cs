﻿using ETexsys.Common.ExcelHelp;
using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Finance.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class SettlementReportController : Controller
    {
        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        [Dependency]
        public IRepository<textile> i_textile { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }


        [Dependency]
        public IRepository<invoice> i_invoice { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }

        [Dependency]
        public IRepository<region_setting> i_region_setting { get; set; }

        [Dependency]
        public IRepository<pricetemplate> i_pricetemplate { get; set; }

        [Dependency]
        public IRepository<pricedetail> i_pricedetail { get; set; }

        [Dependency]
        public IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }

        // GET: Finance/SettlementReport
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WashingSettlment()
        {
            int userId = LoginUserManage.GetInstance().GetLoginUserId();
            var query = from t in i_region.Entities
                        join v in i_sys_user_dataview.Entities on t.ID equals v.RegionID
                        where t.IsDelete == false && v.IsDelete == false && t.RegionType == 1 && v.UserID == userId
                        orderby t.BrandID, t.Sort, t.RegionName
                        select new { t.ID, t.RegionName };
            //List<region> HotelList = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 1).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();
            List<region> HotelList = new List<region>();
            query.ToList().ForEach(q =>
            {
                HotelList.Add(new region { ID = q.ID, RegionName = q.RegionName });
            });
            ViewData["HotelList"] = HotelList;
            return View();
        }

        public ActionResult WashingSettlmentQuery(int hotelId, string startDate, string endDate)
        {
            pricetemplate priceModel = null;
            region_setting regionModel = i_region_setting.GetByKey(hotelId);
            if (regionModel != null)
            {
                priceModel = i_pricetemplate.GetByKey(regionModel.WashID);
            }

            if (priceModel == null)
            {
                return new JsonResult { Data = "fail", JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }

            List<pricedetail> PriceList = i_pricedetail.Entities.Where(v => v.TemplateID == priceModel.ID).ToList();

            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);

            List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false).ToList();

            int invtype = priceModel.DataSources;
            int subType = priceModel.DataSources == 1 ? 1 : 0;

            var query = from t in i_invoicedetail.Entities
                        join r in i_region.Entities on t.HotelID equals r.ID
                        where t.InvType == invtype && t.HotelID == hotelId && t.InvSubType == subType && t.InvCreateTime >= sDate && t.InvCreateTime < eDate
                        select new { t.HotelID, t.InvCreateTime, t.ClassID, t.ClassName, t.SizeID, t.SizeName, t.TextileCount };

            var data = query.ToList();

            DataTable dt = new DataTable();

            #region 表头

            DataColumn col = new DataColumn();
            col.ColumnName = "品名";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            if (priceModel.SubType == 1)
            {
                col = new DataColumn();
                col.ColumnName = "尺寸";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
            }

            string[] px = new string[10] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            int _month = sDate.Month;
            int m_index = 0;
            DateTime d = sDate;
            while (d < eDate)
            {
                if (d.Month != _month)
                {
                    m_index++;
                    _month = d.Month;
                }
                col = new DataColumn();
                col.ColumnName = px[m_index] + d.Day.ToString();
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                d = d.AddDays(1);
            }

            col = new DataColumn();
            col.ColumnName = "小计";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "单价";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            col = new DataColumn();
            col.ColumnName = "洗涤金额";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            #endregion

            var classQuery = from t in data join c in classList on t.ClassID equals c.ID group t by new { c.Sort, t.ClassID, t.ClassName } into m orderby m.Key.Sort, m.Key.ClassName select new { m.Key.ClassID, m.Key.ClassName };

            DataRow row = null;

            int colIndex = priceModel.SubType == 1 ? 3 : 2;
            int[] total = new int[dt.Columns.Count - colIndex];
            decimal totalprice = 0;

            classQuery.ToList().ForEach(c =>
            {
                if (priceModel.SubType == 1)
                {
                    var sizeQuery = from t in data where t.ClassID == c.ClassID group t by new { t.SizeID, t.SizeName } into m select new { m.Key.SizeID, m.Key.SizeName };

                    sizeQuery.ToList().ForEach(s =>
                    {
                        row = dt.NewRow();
                        row["品名"] = c.ClassName;
                        row["尺寸"] = s.SizeName;

                        int k = 0;
                        int rowtotal = 0;
                        DateTime d1 = sDate;
                        _month = sDate.Month;
                        m_index = 0;
                        while (d1 < eDate)
                        {
                            if (d1.Month != _month)
                            {
                                m_index++;
                                _month = d1.Month;
                            }
                            DateTime d2 = d1.AddDays(1);
                            var tempData = from t in data
                                           where t.ClassID == c.ClassID && t.SizeID == s.SizeID && t.InvCreateTime >= d1 && t.InvCreateTime < d2
                                           group t by new { t.ClassID, t.SizeID } into m
                                           select new { count = m.Sum(v => v.TextileCount) };
                            int count = 0;
                            if (tempData.FirstOrDefault() != null)
                                count = tempData.FirstOrDefault().count;

                            row[px[m_index] + d1.Day.ToString()] = count;
                            total[k] += count;
                            rowtotal += count;
                            k++;

                            d1 = d2;
                        }
                        var pricem = PriceList.Where(v => v.ClassID == c.ClassID && v.SizeID == s.SizeID).FirstOrDefault();
                        if (pricem != null)
                        {
                            totalprice += pricem.Price * rowtotal;
                            row["单价"] = pricem.Price;
                            row["洗涤金额"] = pricem.Price * rowtotal;
                            row["小计"] = rowtotal;
                        }

                        dt.Rows.Add(row);
                    });
                }
                else
                {
                    row = dt.NewRow();
                    row["品名"] = c.ClassName;

                    int k = 0; int rowtotal = 0;
                    DateTime d1 = sDate;
                    _month = sDate.Month;
                    m_index = 0;
                    while (d1 < eDate)
                    {
                        if (d1.Month != _month)
                        {
                            m_index++;
                            _month = d1.Month;
                        }
                        DateTime d2 = d1.AddDays(1);
                        var tempData = from t in data
                                       where t.ClassID == c.ClassID && t.InvCreateTime >= d1 && t.InvCreateTime < d2
                                       group t by new { t.ClassID } into m
                                       select new { count = m.Sum(v => v.TextileCount) };
                        int count = 0;
                        if (tempData.FirstOrDefault() != null)
                            count = tempData.FirstOrDefault().count;

                        row[px[m_index] + d1.Day.ToString()] = count;
                        total[k] += count;
                        rowtotal += count;
                        k++;

                        d1 = d2;
                    }

                    var pricem = PriceList.Where(v => v.ClassID == c.ClassID).FirstOrDefault();
                    if (pricem != null)
                    {
                        totalprice += pricem.Price * rowtotal;
                        row["单价"] = pricem.Price;
                        row["洗涤金额"] = pricem.Price * rowtotal;
                        row["小计"] = rowtotal;
                    }

                    dt.Rows.Add(row);

                }

            });


            row = dt.NewRow();
            row["品名"] = "合计";
            if (priceModel.SubType == 1)
            {
                row["尺寸"] = "";
            }
            row["洗涤金额"] = totalprice;
            for (int i = colIndex; i < dt.Columns.Count - 3; i++)
            {
                row[dt.Columns[i - 1].ColumnName] = total[i - colIndex];
            }
            dt.Rows.Add(row);

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult RentSettlment()
        {
            int userId = LoginUserManage.GetInstance().GetLoginUserId();
            var query = from t in i_region.Entities
                        join v in i_sys_user_dataview.Entities on t.ID equals v.RegionID
                        where t.IsDelete == false && v.IsDelete == false && t.RegionType == 1 && v.UserID == userId
                        orderby t.BrandID, t.Sort, t.RegionName
                        select new { t.ID, t.RegionName };
            //List<region> HotelList = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 1).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();
            List<region> HotelList = new List<region>();
            query.ToList().ForEach(q =>
            {
                HotelList.Add(new region { ID = q.ID, RegionName = q.RegionName });
            });
            ViewData["HotelList"] = HotelList;
            return View();
        }

        public ActionResult RentSettlmentQuery(int hotelId, string startDate, string endDate)
        {
            pricetemplate priceModel = null;
            region_setting regionModel = i_region_setting.GetByKey(hotelId);
            if (regionModel != null)
            {
                priceModel = i_pricetemplate.GetByKey(regionModel.RentID);
            }

            if (priceModel == null)
            {
                return new JsonResult { Data = "fail", JsonRequestBehavior = JsonRequestBehavior.DenyGet };
            }

            List<pricedetail> PriceList = i_pricedetail.Entities.Where(v => v.TemplateID == priceModel.ID).ToList();

            DataTable dt = new DataTable();
            //按照污物送洗&净物配送
            if (priceModel.DataSources == 3 || priceModel.DataSources == 4)
            {
                #region 
                DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime eDate = sDate.AddMonths(1).AddDays(-1);

                if (!string.IsNullOrWhiteSpace(startDate))
                    DateTime.TryParse(startDate, out sDate);
                if (!string.IsNullOrWhiteSpace(endDate))
                    DateTime.TryParse(endDate, out eDate);

                eDate = eDate.AddDays(1);

                List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false).ToList();

                int invtype = priceModel.DataSources - 2;
                int subType = priceModel.DataSources == 3 ? 1 : 0;

                var query = from t in i_invoicedetail.Entities
                            join r in i_region.Entities on t.HotelID equals r.ID
                            where t.InvType == invtype && t.HotelID == hotelId && t.InvSubType == subType && t.InvCreateTime >= sDate && t.InvCreateTime < eDate
                            select new { t.HotelID, t.InvCreateTime, t.ClassID, t.ClassName, t.SizeID, t.SizeName, t.TextileCount };

                var data = query.ToList();


                #region 表头

                DataColumn col = new DataColumn();
                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                if (priceModel.SubType == 1)
                {
                    col = new DataColumn();
                    col.ColumnName = "尺寸";
                    col.DataType = typeof(string);
                    dt.Columns.Add(col);
                }

                string[] px = new string[10] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
                int _month = sDate.Month;
                int m_index = 0;
                DateTime d = sDate;
                while (d < eDate)
                {
                    if (d.Month != _month)
                    {
                        m_index++;
                        _month = d.Month;
                    }
                    col = new DataColumn();
                    col.ColumnName = px[m_index] + d.Day.ToString();
                    col.DataType = typeof(string);
                    dt.Columns.Add(col);

                    d = d.AddDays(1);
                }

                col = new DataColumn();
                col.ColumnName = "小计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                col = new DataColumn();
                col.ColumnName = "单价";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                col = new DataColumn();
                col.ColumnName = "租赁金额";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                #endregion

                var classQuery = from t in data join c in classList on t.ClassID equals c.ID group t by new { c.Sort, t.ClassID, t.ClassName } into m orderby m.Key.Sort, m.Key.ClassName select new { m.Key.ClassID, m.Key.ClassName };

                DataRow row = null;

                int colIndex = priceModel.SubType == 1 ? 3 : 2;
                int[] total = new int[dt.Columns.Count - colIndex];
                decimal totalprice = 0;

                classQuery.ToList().ForEach(c =>
                {
                    if (priceModel.SubType == 1)
                    {
                        var sizeQuery = from t in data where t.ClassID == c.ClassID group t by new { t.SizeID, t.SizeName } into m select new { m.Key.SizeID, m.Key.SizeName };

                        sizeQuery.ToList().ForEach(s =>
                        {
                            row = dt.NewRow();
                            row["品名"] = c.ClassName;
                            row["尺寸"] = s.SizeName;

                            int k = 0;
                            int rowtotal = 0;
                            DateTime d1 = sDate;
                            _month = sDate.Month;
                            m_index = 0;
                            while (d1 < eDate)
                            {
                                if (d1.Month != _month)
                                {
                                    m_index++;
                                    _month = d1.Month;
                                }
                                DateTime d2 = d1.AddDays(1);
                                var tempData = from t in data
                                               where t.ClassID == c.ClassID && t.SizeID == s.SizeID && t.InvCreateTime >= d1 && t.InvCreateTime < d2
                                               group t by new { t.ClassID, t.SizeID } into m
                                               select new { count = m.Sum(v => v.TextileCount) };
                                int count = 0;
                                if (tempData.FirstOrDefault() != null)
                                    count = tempData.FirstOrDefault().count;

                                row[px[m_index] + d1.Day.ToString()] = count;
                                total[k] += count;
                                rowtotal += count;
                                k++;

                                d1 = d2;
                            }
                            var pricem = PriceList.Where(v => v.ClassID == c.ClassID && v.SizeID == s.SizeID).FirstOrDefault();
                            if (pricem != null)
                            {
                                totalprice += pricem.Price * rowtotal;
                                row["单价"] = pricem.Price;
                                row["租赁金额"] = pricem.Price * rowtotal;
                                row["小计"] = rowtotal;
                            }

                            dt.Rows.Add(row);
                        });
                    }
                    else
                    {
                        row = dt.NewRow();
                        row["品名"] = c.ClassName;

                        int k = 0; int rowtotal = 0;
                        DateTime d1 = sDate;
                        _month = sDate.Month;
                        m_index = 0;
                        while (d1 < eDate)
                        {
                            if (d1.Month != _month)
                            {
                                m_index++;
                                _month = d1.Month;
                            }
                            DateTime d2 = d1.AddDays(1);
                            var tempData = from t in data
                                           where t.ClassID == c.ClassID && t.InvCreateTime >= d1 && t.InvCreateTime < d2
                                           group t by new { t.ClassID } into m
                                           select new { count = m.Sum(v => v.TextileCount) };
                            int count = 0;
                            if (tempData.FirstOrDefault() != null)
                                count = tempData.FirstOrDefault().count;

                            row[px[m_index] + d1.Day.ToString()] = count;
                            total[k] += count;
                            rowtotal += count;
                            k++;

                            d1 = d2;
                        }

                        var pricem = PriceList.Where(v => v.ClassID == c.ClassID).FirstOrDefault();
                        if (pricem != null)
                        {
                            totalprice += pricem.Price * rowtotal;
                            row["单价"] = pricem.Price;
                            row["租赁金额"] = pricem.Price * rowtotal;
                            row["小计"] = rowtotal;
                        }

                        dt.Rows.Add(row);

                    }

                });


                row = dt.NewRow();
                row["品名"] = "合计";
                if (priceModel.SubType == 1)
                {
                    row["尺寸"] = "";
                }
                row["租赁金额"] = totalprice;
                for (int i = colIndex; i < dt.Columns.Count - 3; i++)
                {
                    row[dt.Columns[i - 1].ColumnName] = total[i - colIndex];
                }
                dt.Rows.Add(row);

                #endregion
            }
            else
            {
                //按照配置数&下单

            }

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        #region Excel 导出

        public FileResult ExportExcel(FormCollection f)
        {
            string title = f["hddTitle"];
            string data = f["hddData"];
            string condition = f["hddCondition"];
            int px = int.Parse(f["hddpx"]);

            List<ExcelTableModel> list = new List<ExcelTableModel>();
            ExcelTableModel table = new ExcelTableModel();
            table.Title = title;
            table.SheetName = title;
            table.Condition = condition.Split('|').ToList();
            table.ContentTable = JSONTable.ToDataTable(data);
            List<ExcelTableColumnModel> colList = new List<ExcelTableColumnModel>();
            ExcelTableColumnModel colModel = null;
            string[] pxarray = new string[10] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            for (int i = 0; i < table.ContentTable.Columns.Count; i++)
            {
                if (table.ContentTable.Columns[i].ColumnName == "品名" || table.ContentTable.Columns[i].ColumnName == "尺寸")
                {
                    colModel = new ExcelTableColumnModel();
                    colModel.Height = 34;
                    colModel.Width = 100;
                    colModel.HeaderText = table.ContentTable.Columns[i].Caption;
                    colModel.ColumnAlignment = ColumnAlignments.Left;
                }
                else
                {
                    string p = table.ContentTable.Columns[i].Caption.Substring(0, 1);
                    if (pxarray.Contains(p))
                    {
                        colModel = new ExcelTableColumnModel();
                        colModel.Height = 34;
                        colModel.Width = 40;
                        colModel.HeaderText = table.ContentTable.Columns[i].Caption.Substring(1);
                        colModel.ColumnAlignment = ColumnAlignments.Right;
                    }
                    else
                    {
                        colModel = new ExcelTableColumnModel();
                        colModel.Height = 34;
                        colModel.Width = 40;
                        colModel.HeaderText = table.ContentTable.Columns[i].Caption;
                        colModel.ColumnAlignment = ColumnAlignments.Right;
                    }

                }
                colList.Add(colModel);
            }
            table.Column = colList;

            list.Add(table);

            MemoryStream ms = ExcelRender.RenderToExcel(list);

            return File(ms, "application/ms-excel", title + ".xls");
        }

        #endregion
    }
}