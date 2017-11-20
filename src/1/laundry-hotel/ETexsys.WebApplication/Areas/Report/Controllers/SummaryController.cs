﻿using ETexsys.Common.ExcelHelp;
using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Report.Models;
using ETexsys.WebApplication.Common;
using Microsoft.Practices.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Report.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class SummaryController : Controller
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
        public IRepository<fabric> i_fabric { get; set; }

        [Dependency]
        public IRepository<businessinvoice> i_bus_invoice { get; set; }

        [Dependency]
        public IRepository<businessdetail> i_bus_detail { get; set; }

        [Dependency]
        public IRepository<invoice> i_invoice { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }

        [Dependency]
        public IRepository<scrapdetail> i_scrapdetail { get; set; }

        [Dependency]
        public IRepository<C_AutoRFIDCollectReader> i_auto_reader { get; set; }

        [Dependency]
        public IRepository<C_TextileSummary> i_c_textilesummary { get; set; }

        [Dependency]
        public IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }

        // GET: Report/Summary
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Reg()
        {
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();
            ViewData["BrandList"] = BrandList;

            return View();
        }

        public ActionResult RegQuery(int brandId, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);

            DataTable dt;
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();

            if (brandId > 0)
            {
                #region 单个品牌

                var query = from t in i_textile.Entities
                            join b in i_brandtype.Entities on t.BrandID equals b.ID
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            join f in i_fabric.Entities on t.FabricID equals f.ID into f_join
                            from f in f_join.DefaultIfEmpty()
                            where t.LogoutType != 2 && t.RegisterTime >= sDate && t.RegisterTime < eDate && t.BrandID == brandId
                            group t by new { b.ID, b.BrandName, classId = c.ID, c.ClassName, c.Sort, t.SizeID, s.SizeName, sizeSort = s.Sort, fabricId = t.FabricID, f.FabricName } into m
                            orderby m.Key.BrandName, m.Key.Sort, m.Key.ClassName, m.Key.FabricName
                            select new
                            {
                                brandID = m.Key.ID,
                                brandName = m.Key.BrandName,
                                classID = m.Key.classId,
                                className = m.Key.ClassName,
                                classSort = m.Key.Sort,
                                sizeID = m.Key.SizeID,
                                sizeName = m.Key.SizeName,
                                fabricId = m.Key.fabricId,
                                fabricName = m.Key.FabricName,
                                count = m.Count()
                            };

                List<size> sizeList = i_size.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.SizeName).ToList();

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "面料";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;
                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classID, t.className, t.classSort, t.fabricId, t.fabricName } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classID,
                            className = m.Key.className,
                            fabricId = m.Key.fabricId,
                            fabricName = m.Key.fabricName,
                            count = m.Sum(v => v.count)
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["面料"] = t.fabricName;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < sizeList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = sizeList[i].SizeName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.sizeID == sizeList[i].ID && v.classID == t.classId && v.fabricId == t.fabricId).Sum(v => v.count).ToString("N0");
                        row[sizeList[i].SizeName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }
            else
            {
                #region 多个品牌


                var query = from t in i_textile.Entities
                            join b in i_brandtype.Entities on t.BrandID equals b.ID
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            join f in i_fabric.Entities on t.FabricID equals f.ID into f_join
                            from f in f_join.DefaultIfEmpty()
                            where t.LogoutType != 2 && t.RegisterTime >= sDate && t.RegisterTime < eDate
                            group t by new { b.ID, b.BrandName, classId = c.ID, c.ClassName, c.Sort, s.SizeName, sizeSort = s.Sort, fabricId = t.FabricID, f.FabricName } into m
                            orderby m.Key.BrandName, m.Key.Sort, m.Key.ClassName, m.Key.FabricName
                            select new
                            {
                                brandID = m.Key.ID,
                                brandName = m.Key.BrandName,
                                classID = m.Key.classId,
                                className = m.Key.ClassName,
                                classSort = m.Key.Sort,
                                sizeName = m.Key.SizeName,
                                fabricId = m.Key.fabricId,
                                fabricName = m.Key.FabricName,
                                count = m.Count()
                            };


                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "面料";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;

                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classID, t.className, t.classSort, t.fabricId, t.fabricName } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classID,
                            className = m.Key.className,
                            fabricId = m.Key.fabricId,
                            fabricName = m.Key.fabricName,
                            count = m.Sum(v => v.count)
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["面料"] = t.fabricName;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < BrandList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = BrandList[i].BrandName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.brandID == BrandList[i].ID && v.classID == t.classId && v.fabricId == t.fabricId).Sum(v => v.count).ToString("N0");
                        row[BrandList[i].BrandName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult Demand()
        {
            List<region> HotelList = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 1).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();
            ViewData["HotelList"] = HotelList;
            return View();
        }

        public ActionResult DemandQuery(int hotelId, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);
            eDate = eDate.AddDays(1);

            DataTable dt;

            if (hotelId > 0)
            {
                #region 按尺寸显示

                int[] state = new int[4] { 2, 3, 4, 5 };

                var query = from i in i_bus_invoice.Entities
                            join d in i_bus_detail.Entities on i.BID equals d.BID
                            join c in i_textileclass.Entities on d.ClassID equals c.ID
                            where i.InvType == 1 && state.Contains(i.Stated) && i.HotelID == hotelId && i.CreateTime >= sDate && i.CreateTime < eDate
                            group d by new { c.Sort, d.ClassID, d.ClassName, d.SizeID, d.SizeName } into m
                            select new
                            {
                                classId = m.Key.ClassID,
                                classSort = m.Key.Sort,
                                className = m.Key.ClassName,
                                sizeId = m.Key.SizeID,
                                sizeName = m.Key.SizeName,
                                count = m.Sum(v => v.TextileCount)
                            };

                List<size> sizeList = i_size.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.SizeName).ToList();

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;
                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classId, t.className, t.classSort } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classId,
                            className = m.Key.className,
                            count = m.Sum(v => v.count)
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < sizeList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = sizeList[i].SizeName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.sizeId == sizeList[i].ID && v.classId == t.classId).Sum(v => v.count).ToString("N0");
                        row[sizeList[i].SizeName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }
            else
            {
                #region 按流通显示

                int[] state = new int[4] { 2, 3, 4, 5 };

                var query = from i in i_bus_invoice.Entities
                            join d in i_bus_detail.Entities on i.BID equals d.BID
                            join c in i_textileclass.Entities on d.ClassID equals c.ID
                            join h in i_region.Entities on i.HotelID equals h.ID
                            where i.InvType == 1 && state.Contains(i.Stated) && i.CreateTime >= sDate && i.CreateTime < eDate
                            group d by new { h.BrandID, c.Sort, d.ClassID, d.ClassName } into m
                            select new
                            {
                                brandId = m.Key.BrandID,
                                classId = m.Key.ClassID,
                                classSort = m.Key.Sort,
                                className = m.Key.ClassName,
                                count = m.Sum(v => v.TextileCount)
                            };

                List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;
                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classId, t.className, t.classSort } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classId,
                            className = m.Key.className,
                            count = m.Sum(v => v.count)
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < BrandList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = BrandList[i].BrandName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.brandId == BrandList[i].ID && v.classId == t.classId).Sum(v => v.count).ToString("N0");
                        row[BrandList[i].BrandName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult Delivery()
        {
            List<region> HotelList = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 1).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();
            ViewData["HotelList"] = HotelList;
            return View();
        }

        public ActionResult DeliveryQuery(int hotelId, int hotelId2, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);
            eDate = eDate.AddDays(1);

            DataTable dt;


            if (hotelId > 0)
            {
                #region 按尺寸显示

                int[] state = new int[4] { 2, 3, 4, 5 };

                var query1 = from i in i_invoicedetail.Entities
                             join c in i_textileclass.Entities on i.ClassID equals c.ID
                             where i.InvType == 4 && i.HotelID == hotelId && i.InvCreateTime >= sDate && i.InvCreateTime < eDate
                             //group i by new { c.Sort, i.ClassID, i.ClassName, i.SizeID, i.SizeName } into m
                             select new
                             {
                                 classId = i.ClassID,
                                 classSort = c.Sort,
                                 className = i.ClassName,
                                 sizeId = i.SizeID,
                                 sizeName = i.SizeName,
                                 InvSubType = i.InvSubType,
                                 TextileCount = i.TextileCount
                                 //count = m.Sum(v => v.TextileCount)
                             };

                if (hotelId2 == 3)
                {
                    query1 = query1.Where(a => a.InvSubType != 21 && a.InvSubType != 20);
                }
                else if (hotelId2 == 0)
                {

                }
                else
                {
                    query1 = query1.Where(a => a.InvSubType == hotelId2);
                }
                var query = from s in query1
                            group s by new { s.classSort, s.classId, s.className, s.sizeId, s.sizeName } into m
                            select new
                            {
                                classId = m.Key.classId,
                                classSort = m.Key.classSort,
                                className = m.Key.className,
                                sizeId = m.Key.sizeId,
                                sizeName = m.Key.sizeName,
                                count = m.Sum(v => v.TextileCount)
                            };




                List<size> sizeList = i_size.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.SizeName).ToList();

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;
                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classId, t.className, t.classSort } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classId,
                            className = m.Key.className,
                            count = m.Sum(v => v.count)
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < sizeList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = sizeList[i].SizeName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.sizeId == sizeList[i].ID && v.classId == t.classId).Sum(v => v.count).ToString("N0");
                        row[sizeList[i].SizeName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }
            else
            {
                #region 按流通显示

                int[] state = new int[4] { 2, 3, 4, 5 };

                var query1 = from i in i_invoicedetail.Entities
                             join c in i_textileclass.Entities on i.ClassID equals c.ID
                             where i.InvType == 4 && i.InvCreateTime >= sDate && i.InvCreateTime < eDate
                             //group i by new { i.BrandName, i.BrandID, c.Sort, i.ClassID, i.ClassName } into m
                             select new
                             {
                                 brandId = i.BrandID,
                                 brandName = i.BrandName,
                                 classId = i.ClassID,
                                 classSort = c.Sort,
                                 className = i.ClassName,
                                 InvSubType = i.InvSubType,
                                 TextileCount = i.TextileCount
                                 //count = m.Sum(v => v.TextileCount)
                             };


                if (hotelId2 == 3)
                {
                    query1 = query1.Where(a => a.InvSubType != 21 && a.InvSubType != 20);
                }
                else if (hotelId2 == 0)
                {

                }
                else
                {
                    query1 = query1.Where(a => a.InvSubType == hotelId2);
                }
                var query = from s in query1
                            group s by new { s.brandName, s.brandId, s.classSort, s.classId, s.className } into m
                            select new
                            {
                                brandId = m.Key.brandId,
                                brandName = m.Key.brandName,
                                classId = m.Key.classId,
                                classSort = m.Key.classSort,
                                className = m.Key.className,
                                count = m.Sum(v => v.TextileCount)
                            };

                List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;
                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classId, t.className, t.classSort } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classId,
                            className = m.Key.className,
                            count = m.Sum(v => v.count)
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < BrandList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        { 
                            col = new DataColumn();
                            col.ColumnName = BrandList[i].BrandName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.brandId == BrandList[i].ID && v.classId == t.classId).Sum(v => v.count).ToString("N0");
                        row[BrandList[i].BrandName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult InStorage()
        {
            List<region> storageList = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 3 && (v.RegionMode == 2 || v.RegionMode == 1)).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();
            ViewData["StorageList"] = storageList;
            return View();
        }

        public ActionResult InStorageQuery(int storageId, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);
            DataTable dt;

            if (storageId > 0)
            {
                #region 按尺寸显示

                var query = from i in i_invoicedetail.Entities
                            join c in i_textileclass.Entities on i.ClassID equals c.ID
                            where i.InvType == 3 && i.RegionID == storageId && i.InvCreateTime >= sDate && i.InvCreateTime < eDate && (i.InvSubType == 22 || i.InvSubType == 24)
                            group i by new { c.Sort, i.ClassID, i.ClassName, i.SizeID, i.SizeName } into m
                            select new
                            {
                                classId = m.Key.ClassID,
                                classSort = m.Key.Sort,
                                className = m.Key.ClassName,
                                sizeId = m.Key.SizeID,
                                sizeName = m.Key.SizeName,
                                count = m.Sum(v => v.TextileCount)
                            };

                List<size> sizeList = i_size.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.SizeName).ToList();

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;
                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classId, t.className, t.classSort } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classId,
                            className = m.Key.className,
                            count = m.Sum(v => v.count)
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < sizeList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = sizeList[i].SizeName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.sizeId == sizeList[i].ID && v.classId == t.classId).Sum(v => v.count).ToString("N0");
                        row[sizeList[i].SizeName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }
            else
            {
                #region 按流通显示 

                var query = from i in i_invoicedetail.Entities
                            join c in i_textileclass.Entities on i.ClassID equals c.ID
                            where i.InvType == 3 && i.InvCreateTime >= sDate && i.InvCreateTime < eDate && (i.InvSubType == 22 || i.InvSubType == 24)
                            group i by new { i.BrandID, c.Sort, i.ClassID, i.ClassName } into m
                            select new
                            {
                                brandId = m.Key.BrandID,
                                classId = m.Key.ClassID,
                                classSort = m.Key.Sort,
                                className = m.Key.ClassName,
                                count = m.Sum(v => v.TextileCount)
                            };

                List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;
                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classId, t.className, t.classSort } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classId,
                            className = m.Key.className,
                            count = m.Sum(v => v.count)
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < BrandList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = BrandList[i].BrandName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.brandId == BrandList[i].ID && v.classId == t.classId).Sum(v => v.count).ToString("N0");
                        row[BrandList[i].BrandName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult Scrap()
        {
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();
            ViewData["BrandList"] = BrandList;

            return View();
        }

        public ActionResult ScrapQuery(int brandId, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);

            DataTable dt;
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();

            if (brandId > 0)
            {
                #region 单个品牌

                var query = from t in i_scrapdetail.Entities
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join f in i_fabric.Entities on t.FabricID equals f.ID into f_join
                            from f in f_join.DefaultIfEmpty()
                            where t.CreateTime >= sDate && t.CreateTime < eDate && t.BrandID == brandId
                            select new
                            {
                                classId = t.ClassID,
                                className = t.ClassName,
                                classSort = c.Sort,
                                sizeId = t.SizeID,
                                sizeName = t.SizeName,
                                fabricId = t.FabricID,
                                fabricName = f.FabricName
                            };

                List<size> sizeList = i_size.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.SizeName).ToList();

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "面料";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;
                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classId, t.className, t.classSort, t.fabricId, t.fabricName } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classId,
                            className = m.Key.className,
                            fabricId = m.Key.fabricId,
                            fabricName = m.Key.fabricName,
                            count = m.Count()
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["面料"] = t.fabricName;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < sizeList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = sizeList[i].SizeName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.sizeId == sizeList[i].ID && v.classId == t.classId && v.fabricId == t.fabricId).Count().ToString("N0");
                        row[sizeList[i].SizeName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }
            else
            {
                #region 多个品牌

                var query = from t in i_scrapdetail.Entities
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join f in i_fabric.Entities on t.FabricID equals f.ID into f_join
                            from f in f_join.DefaultIfEmpty()
                            where t.CreateTime >= sDate && t.CreateTime < eDate
                            select new
                            {
                                brandId = t.BrandID,
                                classId = t.ClassID,
                                className = t.ClassName,
                                classSort = c.Sort,
                                sizeId = t.SizeID,
                                sizeName = t.SizeName,
                                fabricId = t.FabricID,
                                fabricName = f.FabricName
                            };

                dt = new DataTable();
                DataColumn col = new DataColumn();

                col.ColumnName = "品名";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "面料";
                col.DataType = typeof(string);
                dt.Columns.Add(col);
                col = new DataColumn();
                col.ColumnName = "合计";
                col.DataType = typeof(string);
                dt.Columns.Add(col);

                DataRow row = null;

                var data = query.ToList();

                var q = from t in data
                        group t by new { t.classId, t.className, t.classSort, t.fabricId, t.fabricName } into m
                        orderby m.Key.classSort, m.Key.className
                        select new
                        {
                            classId = m.Key.classId,
                            className = m.Key.className,
                            fabricId = m.Key.fabricId,
                            fabricName = m.Key.fabricName,
                            count = m.Count()
                        };

                bool isAppendColHeader = false;

                q.ToList().ForEach(t =>
                {
                    row = dt.NewRow();
                    row["品名"] = t.className;
                    row["面料"] = t.fabricName;
                    row["合计"] = t.count.ToString("N0");
                    for (int i = 0; i < BrandList.Count; i++)
                    {
                        if (!isAppendColHeader)
                        {
                            col = new DataColumn();
                            col.ColumnName = BrandList[i].BrandName;
                            col.DataType = typeof(string);
                            dt.Columns.Add(col);
                        }

                        string count = data.Where(v => v.brandId == BrandList[i].ID && v.classId == t.classId && v.fabricId == t.fabricId).Count().ToString("N0");
                        row[BrandList[i].BrandName] = count;
                    }
                    isAppendColHeader = true;
                    dt.Rows.Add(row);
                });

                #endregion
            }

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult ScrapDetail()
        {
            List<region> HotelList = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 1).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();
            ViewData["HotelList"] = HotelList;
            return View();
        }

        public ActionResult ScrapDetailQuery(int responsible, int hotelId, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);

            DataTable dt;
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();

            var query = from t in i_scrapdetail.Entities
                        join c in i_textileclass.Entities on t.ClassID equals c.ID
                        join f in i_fabric.Entities on t.FabricID equals f.ID into f_join
                        from f in f_join.DefaultIfEmpty()
                        where t.CreateTime >= sDate && t.CreateTime < eDate && t.ResponsibleType == responsible
                        orderby t.BrandID, t.ClassName, t.SizeName, t.CreateTime
                        select new
                        {
                            t.RFIDTagNo,
                            brandId = t.BrandID,
                            brandName = t.BrandName,
                            classId = t.ClassID,
                            className = t.ClassName,
                            classSort = c.Sort,
                            sizeId = t.SizeID,
                            sizeName = t.SizeName,
                            fabricId = t.FabricID,
                            fabricName = f.FabricName,
                            responsibleName = t.ResponsibleType == 1 ? "工厂" : t.ResponsibleName,
                            t.ScrapName,
                            t.CreateUserName,
                            t.CreateTime,
                            t.Washtime,
                            t.ResponsibleID,
                            t.LastUsingArea,
                            t.LastUsingTime
                        };
            if (responsible == 2 && hotelId > 0)
            {
                query = query.Where(v => v.ResponsibleID == hotelId);
            }

            dt = new DataTable();
            DataColumn col = new DataColumn();

            col.ColumnName = "EPC编码";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "品牌流通";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "品名";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "尺寸";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "面料";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "责任方";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "报废原因";
            col.DataType = typeof(string);
            dt.Columns.Add(col); col = new DataColumn();
            col.ColumnName = "报废人";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "最后使用酒店";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "最后使用时间";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "操作时间";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "洗涤次数";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            DataRow row = null;

            query.ToList().ForEach(t =>
            {
                row = dt.NewRow();
                row["EPC编码"] = t.RFIDTagNo;
                row["品牌流通"] = t.brandName;
                row["品名"] = t.className;
                row["尺寸"] = t.sizeName;
                row["面料"] = t.fabricName;
                row["责任方"] = t.responsibleName;
                row["报废原因"] = t.ScrapName;
                row["报废人"] = t.CreateUserName;
                row["最后使用酒店"] = t.LastUsingArea;
                row["最后使用时间"] = t.LastUsingTime.ToString("yyyy/MM/dd HH:mm");
                row["操作时间"] = t.CreateTime.ToString("yyyy/MM/dd HH:mm");
                row["洗涤次数"] = t.Washtime;

                dt.Rows.Add(row);
            });



            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult Receive()
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

        public ActionResult ReceiveQuery(int hotelId, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);

            List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false).ToList();

            var query = from t in i_invoicedetail.Entities
                        join r in i_region.Entities on t.HotelID equals r.ID
                        join h in i_brandtype.Entities on r.BrandID equals h.ID
                        where t.InvType == 1 && t.InvSubType == 1 && t.InvCreateTime >= sDate && t.InvCreateTime < eDate
                        select new { t.HotelID, r.BrandID, h.BrandName, t.InvCreateTime, t.ClassID, t.ClassName, t.SizeID, t.SizeName, t.TextileCount };

            if (hotelId > 0)
            {
                query = query.Where(v => v.HotelID == hotelId);
            }
            else
            {
                int userId = LoginUserManage.GetInstance().GetLoginUserId();
                var userQuery = from t in i_region.Entities
                                join v in i_sys_user_dataview.Entities on t.ID equals v.RegionID
                                where t.IsDelete == false && v.IsDelete == false && t.RegionType == 1 && v.UserID == userId
                                orderby t.BrandID, t.Sort, t.RegionName
                                select new { t.ID, t.RegionName };

                List<int> hotelList = new List<int>();
                userQuery.ToList().ForEach(q =>
                {
                    hotelList.Add(q.ID);
                });

                query = query.Where(v => hotelList.Contains(v.HotelID));
            }

            var data = query.ToList();

            DataTable dt = new DataTable();

            #region 表头

            DataColumn col = new DataColumn();
            col.ColumnName = "流通品牌";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "品名";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "尺寸";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

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

            #endregion

            var brandQuery = from t in data group t by new { t.BrandID, t.BrandName } into m select new { m.Key.BrandName, m.Key.BrandID };
            var classQuery = from t in data join c in classList on t.ClassID equals c.ID group t by new { c.Sort, t.ClassID, t.ClassName } into m orderby m.Key.Sort, m.Key.ClassName select new { m.Key.ClassID, m.Key.ClassName };

            DataRow row = null;

            int[] total = new int[dt.Columns.Count - 3];

            brandQuery.ToList().ForEach(b =>
                {
                    int[] brandTotal = new int[dt.Columns.Count - 3];
                    classQuery.ToList().ForEach(c =>
                {
                    var sizeQuery = from t in data where t.BrandID == b.BrandID && t.ClassID == c.ClassID group t by new { t.SizeID, t.SizeName } into m select new { m.Key.SizeID, m.Key.SizeName };

                    sizeQuery.ToList().ForEach(s =>
                    {
                        row = dt.NewRow();
                        row["流通品牌"] = b.BrandName;
                        row["品名"] = c.ClassName;
                        row["尺寸"] = s.SizeName;

                        int k = 0;
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
                                           where t.BrandID == b.BrandID && t.ClassID == c.ClassID && t.SizeID == s.SizeID && t.InvCreateTime >= d1 && t.InvCreateTime < d2
                                           group t by new { t.BrandID } into m
                                           select new { count = m.Sum(v => v.TextileCount) };
                            int count = 0;
                            if (tempData.FirstOrDefault() != null)
                                count = tempData.FirstOrDefault().count;

                            row[px[m_index] + d1.Day.ToString()] = count;
                            brandTotal[k] += count;
                            total[k] += count;
                            k++;

                            d1 = d2;
                        }

                        dt.Rows.Add(row);
                    });
                });

                    row = dt.NewRow();
                    row["流通品牌"] = b.BrandName;
                    row["品名"] = "小计";
                    row["尺寸"] = "";

                    for (int i = 3; i < dt.Columns.Count; i++)
                    {
                        row[dt.Columns[i].ColumnName] = brandTotal[i - 3];
                    }
                    dt.Rows.Add(row);
                });

            row = dt.NewRow();
            row["流通品牌"] = "全部合计";
            row["品名"] = "全部合计";
            row["尺寸"] = "全部合计";

            for (int i = 3; i < dt.Columns.Count; i++)
            {
                row[dt.Columns[i].ColumnName] = total[i - 3];
            }
            dt.Rows.Add(row);

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult Send()
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

        public ActionResult SendQuery(int hotelId, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);

            List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false).ToList();

            var query = from t in i_invoicedetail.Entities
                        join r in i_region.Entities on t.HotelID equals r.ID
                        join h in i_brandtype.Entities on r.BrandID equals h.ID
                        where t.InvType == 2 && t.InvCreateTime >= sDate && t.InvCreateTime < eDate
                        select new { t.HotelID, r.BrandID, h.BrandName, t.InvCreateTime, t.ClassID, t.ClassName, t.SizeID, t.SizeName, t.TextileCount };

            if (hotelId > 0)
            {
                query = query.Where(v => v.HotelID == hotelId);
            }
            else
            {
                int userId = LoginUserManage.GetInstance().GetLoginUserId();
                var userQuery = from t in i_region.Entities
                                join v in i_sys_user_dataview.Entities on t.ID equals v.RegionID
                                where t.IsDelete == false && v.IsDelete == false && t.RegionType == 1 && v.UserID == userId
                                orderby t.BrandID, t.Sort, t.RegionName
                                select new { t.ID, t.RegionName };

                List<int> hotelList = new List<int>();
                userQuery.ToList().ForEach(q =>
                {
                    hotelList.Add(q.ID);
                });

                query = query.Where(v => hotelList.Contains(v.HotelID));
            }

            var data = query.ToList();

            DataTable dt = new DataTable();

            #region 表头

            DataColumn col = new DataColumn();
            col.ColumnName = "流通品牌";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "品名";
            col.DataType = typeof(string);
            dt.Columns.Add(col);
            col = new DataColumn();
            col.ColumnName = "尺寸";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

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

            #endregion

            var brandQuery = from t in data group t by new { t.BrandID, t.BrandName } into m select new { m.Key.BrandName, m.Key.BrandID };
            var classQuery = from t in data join c in classList on t.ClassID equals c.ID group t by new { c.Sort, t.ClassID, t.ClassName } into m orderby m.Key.Sort, m.Key.ClassName select new { m.Key.ClassID, m.Key.ClassName };

            DataRow row = null;

            int[] total = new int[dt.Columns.Count - 3];

            brandQuery.ToList().ForEach(b =>
            {
                int[] brandTotal = new int[dt.Columns.Count - 3];
                classQuery.ToList().ForEach(c =>
                {
                    var sizeQuery = from t in data where t.BrandID == b.BrandID && t.ClassID == c.ClassID group t by new { t.SizeID, t.SizeName } into m select new { m.Key.SizeID, m.Key.SizeName };

                    sizeQuery.ToList().ForEach(s =>
                    {
                        row = dt.NewRow();
                        row["流通品牌"] = b.BrandName;
                        row["品名"] = c.ClassName;
                        row["尺寸"] = s.SizeName;

                        int k = 0;
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
                                           where t.BrandID == b.BrandID && t.ClassID == c.ClassID && t.SizeID == s.SizeID && t.InvCreateTime >= d1 && t.InvCreateTime < d2
                                           group t by new { t.BrandID } into m
                                           select new { count = m.Sum(v => v.TextileCount) };
                            int count = 0;
                            if (tempData.FirstOrDefault() != null)
                                count = tempData.FirstOrDefault().count;

                            row[px[m_index] + d1.Day.ToString()] = count;
                            brandTotal[k] += count;
                            total[k] += count;
                            k++;

                            d1 = d2;
                        }

                        dt.Rows.Add(row);
                    });
                });

                row = dt.NewRow();
                row["流通品牌"] = b.BrandName;
                row["品名"] = "小计";
                row["尺寸"] = "";

                for (int i = 3; i < dt.Columns.Count; i++)
                {
                    row[dt.Columns[i].ColumnName] = brandTotal[i - 3];
                }
                dt.Rows.Add(row);
            });

            row = dt.NewRow();
            row["流通品牌"] = "全部合计";
            row["品名"] = "全部合计";
            row["尺寸"] = "全部合计";

            for (int i = 3; i < dt.Columns.Count; i++)
            {
                row[dt.Columns[i].ColumnName] = total[i - 3];
            }
            dt.Rows.Add(row);

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        public ActionResult RSDaily()
        {
            List<region> HotelList = i_region.Entities.Where(v => v.IsDelete == false && v.RegionType == 1).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ThenBy(v => v.RegionName).ToList();
            ViewData["HotelList"] = HotelList;
            return View();
        }

        public ActionResult RSDailyQuery(int hotelId, string time, int type)
        {
            DateTime date = DateTime.MinValue;
            DateTime.TryParse(time, out date);
            DateTime end = date.AddDays(1);

            var query = from c in i_invoicedetail.Entities
                        where (c.InvType == 1 || c.InvType == 2) && c.InvCreateTime >= date && c.InvCreateTime < end
                        select new
                        {
                            c.HotelID,
                            InvCreateTime = time,
                            c.ClassID,
                            c.DataType,
                            c.InvType,
                            c.InvSubType,
                            c.TextileCount,
                        };

            if (hotelId != 0)
            {
                query = query.Where(c => c.HotelID == hotelId);
            }
            if (type != 0)
            {
                query = query.Where(c => c.DataType == type);
            }

            var q = from c in query.ToList()
                    group c by new
                    {
                        hotelID = c.HotelID,
                        date = c.InvCreateTime,
                        classID = c.ClassID,
                        subType = c.InvSubType
                    } into m
                    select new
                    {
                        m.Key.hotelID,
                        m.Key.date,
                        m.Key.classID,
                        normal = (from d in m where d.InvSubType == 1 select d.TextileCount).DefaultIfEmpty(0).Sum(),
                        dirty = (from d in m where d.InvSubType == 2 select d.TextileCount).DefaultIfEmpty(0).Sum(),
                        backwash = (from d in m where d.InvSubType == 3 select d.TextileCount).DefaultIfEmpty(0).Sum(),
                        guoshui = (from d in m where d.InvSubType == 4 select d.TextileCount).DefaultIfEmpty(0).Sum(),
                        sendcount = (from d in m where d.InvSubType == 0 select d.TextileCount).DefaultIfEmpty(0).Sum(),
                    };

            var query1 = from c in q
                         join h in i_region.Entities on c.hotelID equals h.ID
                         join tc in i_textileclass.Entities on c.classID equals tc.ID
                         group c by new
                         {
                             h.RegionName,
                             c.date,
                             tc.ClassName,
                             tc.ClassCode,
                             tc.Sort
                         } into m
                         select new
                         {
                             HotelName = m.Key.RegionName,
                             CreateTime = m.Key.date,
                             ClassName = m.Key.ClassName,
                             Code = m.Key.ClassCode,
                             Sort = m.Key.Sort,
                             normal = m.Sum(c => c.normal),
                             dirty = m.Sum(c => c.dirty),
                             backwash = m.Sum(c => c.backwash),
                             guoshui = m.Sum(c => c.guoshui),
                             sendcount = m.Sum(c => c.sendcount),
                         };

            List<RSDailyModel> result = new List<RSDailyModel>();
            RSDailyModel rsdm = null;
            RSDailyItemModel rsdim = null;

            var query2 = from t in query1
                         group t by new { t0 = t.HotelName } into m
                         select new { HotelName = m.Key.t0 };

            query2.ToList().ForEach(qq =>
            {
                rsdm = new RSDailyModel() { ShortName = qq.HotelName, CreateTime = time };
                rsdm.Data = new List<RSDailyItemModel>();

                var list = query1.Where(c => c.HotelName.Equals(qq.HotelName));

                list.ToList().ForEach(c =>
                {
                    rsdim = new RSDailyItemModel();

                    rsdim.Code = c.Code;
                    rsdim.TextileName = c.ClassName;
                    rsdim.Normal = c.normal;
                    rsdim.Dirty = c.dirty;
                    rsdim.BackWash = c.backwash;
                    rsdim.GuoShui = c.guoshui;
                    rsdim.SendCount = c.sendcount;

                    rsdm.Data.Add(rsdim);
                });
                result.Add(rsdm);
            });

            JsonResult jr = new JsonResult() { Data = result };

            return jr;
        }

        public ActionResult AutoRFIDCollect()
        {
            return View();
        }

        public ActionResult AutoRFIDCollectQuery(string startDate, string endDate)
        {
            DateTime sDate = DateTime.Now;
            DateTime eDate = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);

            string rsql = string.Format("SELECT ReaderName,ReaderCode FROM {0}.rfid_reader", System.Configuration.ConfigurationManager.AppSettings["FlowDBName"]);
            List<C_AutoRFIDCollectReader> ReaderList = i_auto_reader.SQLQuery(rsql, "").ToList();

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT SiteCode,ClassID,COUNT(ID) AS TextileCount FROM (");
            sql.AppendFormat("SELECT SiteCode,EPC FROM {0}.rfid_scan_record WHERE ScanTime>='{1} 00:00:00' AND ScanTime<'{2} 00:00:00' GROUP BY SiteCode,EPC) AS V LEFT JOIN textile AS T ON V.EPC=T.TagNO ", System.Configuration.ConfigurationManager.AppSettings["FlowDBName"], sDate.ToString("yyyy/MM/dd"), eDate.ToString("yyyy/MM/dd"));
            sql.AppendFormat("WHERE IsFlag=1 ");
            sql.Append("GROUP BY SiteCode,ClassID");

            List<C_TextileSummary> list = i_c_textilesummary.SQLQuery(sql.ToString(), "").ToList();
            List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false).ToList();

            var query = from t in list join c in classList on t.ClassID equals c.ID select new { c.ClassName, t.ClassID, t.TextileCount, t.SiteCode };

            DataTable dt = new DataTable();

            #region 表头

            DataColumn col = new DataColumn();
            col.ColumnName = "品名";
            col.DataType = typeof(string);
            dt.Columns.Add(col);

            ReaderList.ForEach(q =>
            {
                col = new DataColumn();
                col.ColumnName = q.ReaderName;
                col.DataType = typeof(string);
                dt.Columns.Add(col);
            });

            #endregion 
            DataRow row = null;
            ReaderList.ForEach(r =>
            {
                var q = query.Where(v => v.SiteCode == r.ReaderCode).ToList();
                q.ForEach(_ =>
                {
                    string filter = string.Format(" 品名='{0}'", _.ClassName);
                    DataRow[] rows = dt.Select(filter);
                    if (rows.Length == 0)
                    {
                        row = dt.NewRow();
                        row["品名"] = _.ClassName;
                        row[r.ReaderName] = _.TextileCount;
                        dt.Rows.Add(row);
                    }
                    else
                    {
                        row = rows[0];
                        row[r.ReaderName] = _.TextileCount;
                    }
                });
            });

            int[] total = new int[ReaderList.Count];

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                for (int j = 0; j < total.Length; j++)
                {
                    if (!(dt.Rows[i][j + 1] is DBNull))
                    {
                        total[j] += Convert.ToInt32(dt.Rows[i][j + 1]);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                row = dt.NewRow();
                row["品名"] = "合计";
                for (int j = 0; j < total.Length; j++)
                {
                    row[j + 1] = total[j];
                }
                dt.Rows.Add(row);
            }

            string jsonData = JSONTable.ToJson(dt);
            JsonResult jr = new JsonResult() { Data = jsonData, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }

        #region Excel 导出

        public FileResult SingleExportExcel(FormCollection f)
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

            for (int i = 0; i < table.ContentTable.Columns.Count; i++)
            {
                if (i < px)
                {
                    colModel = new ExcelTableColumnModel();
                    colModel.Height = 34;
                    colModel.Width = 150;
                    colModel.HeaderText = table.ContentTable.Columns[i].Caption;
                    colModel.ColumnAlignment = ColumnAlignments.Left;
                }
                else
                {
                    colModel = new ExcelTableColumnModel();
                    colModel.Height = 34;
                    colModel.Width = 80;
                    colModel.HeaderText = table.ContentTable.Columns[i].Caption;
                    colModel.ColumnAlignment = ColumnAlignments.Right;
                }
                colList.Add(colModel);
            }
            table.Column = colList;

            list.Add(table);

            MemoryStream ms = ExcelRender.RenderToExcel(list);

            return File(ms, "application/ms-excel", title + ".xls");
        }

        public FileResult Receive_SendExportExcel(FormCollection f)
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

            for (int i = 0; i < table.ContentTable.Columns.Count; i++)
            {
                if (i < 3)
                {
                    colModel = new ExcelTableColumnModel();
                    colModel.Height = 34;
                    colModel.Width = 100;
                    colModel.HeaderText = table.ContentTable.Columns[i].Caption;
                    colModel.ColumnAlignment = ColumnAlignments.Left;
                }
                else
                {
                    colModel = new ExcelTableColumnModel();
                    colModel.Height = 34;
                    colModel.Width = 40;
                    colModel.HeaderText = table.ContentTable.Columns[i].Caption.Substring(1);
                    colModel.ColumnAlignment = ColumnAlignments.Right;
                }
                colList.Add(colModel);
            }
            table.Column = colList;

            list.Add(table);

            MemoryStream ms = ExcelRender.RenderToExcel(list);

            return File(ms, "application/ms-excel", title + ".xls");
        }

        public FileResult RSDailyExportExcel(FormCollection f)
        {
            string source = f["hddData"];
            string title = f["hddTitle"];
            string begin = f["hddBegin"];

            Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();

            List<RSDailyModel> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RSDailyModel>>(source);

            string createtime = string.Empty;
            List<string> strlist = new List<string>() { "编号", "品名", "正常", "重污", "返洗", "过水", "发货" };

            for (int i = 0; i < list.Count; i++)
            {
                if (string.IsNullOrEmpty(createtime))
                {
                    createtime = list[i].CreateTime;
                }

                DataTable table = ListToDataTable(list[i].Data);
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    table.Columns[j].ColumnName = strlist[j];
                }
                if (!dic.Keys.Contains(list[i].ShortName))
                {
                    dic.Add(list[i].ShortName, table);
                }
            }

            if (dic.Count > 0)
            {
                MemoryStream ms1 = ExcelRender.ReanderToExcel(dic, createtime);

                return File(ms1, "application/ms-excel", "SFD.xls");
            }
            else
            {
                return File(new MemoryStream(), "application/ms-excel", "SFD.xls");
            }
        }

        private DataTable ListToDataTable(IList list)
        {
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    //获取类型
                    Type colType = pi.PropertyType;
                    //当类型为Nullable<>时
                    if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        colType = colType.GetGenericArguments()[0];
                    }
                    result.Columns.Add(pi.Name, colType);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        object obj = pi.GetValue(list[i], null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }

        #endregion
    }
}