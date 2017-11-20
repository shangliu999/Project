using ETexsys.Common.ExcelHelp;
using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Areas.Report.Models;
using ETexsys.WebApplication.Common;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Report.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class ParseTableController : Controller
    {
        #region 实体
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
        public IRepository<invoicerfid> i_invoicerfid { get; set; }

        [Dependency]
        public IRepository<invoicedetail> i_invoicedetail { get; set; }

        [Dependency]
        public IRepository<scrapdetail> i_scrapdetail { get; set; }


        [Dependency]
        public IRepository<scrap> i_scrap { get; set; }
        [Dependency]
        public IRepository<C_HzlkSummary> i_c_hzlksummary { get; set; }

        [Dependency]
        public IRepository<C_FBlkSummary> i_c_FBlksummary { get; set; }

        [Dependency]
        public IRepository<C_Index> i_c_indexsummary { get; set; }

        [Dependency]
        public IRepository<C_Index4> i_c_index4summary { get; set; }

        [Dependency]
        public IRepository<FBshow> i_c_fbshowsummary { get; set; }
        [Dependency]
        public IRepository<FBdis> i_c_fdissummary { get; set; }
        #endregion

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
                    colModel.Height = 104;
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
                if (i < 2)
                {
                    colModel = new ExcelTableColumnModel();
                    colModel.Height = 64;
                    colModel.Width = 150;
                    colModel.HeaderText = table.ContentTable.Columns[i].Caption;
                    colModel.ColumnAlignment = ColumnAlignments.Left;
                }
                else
                {
                    colModel = new ExcelTableColumnModel();
                    colModel.Height = 64;
                    colModel.Width = 150;
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


        #endregion

        #region 布草洗涤频率对比分析表
        // GET: Report/ParseTable
        public ActionResult Index()
        {
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();
            ViewData["BrandList"] = BrandList;
            return View();
        }
        public ActionResult HzlkQuery(int brandId, string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);

            List<C_HzlkSummary> list = null;

            if (brandId > 0)
            {

                #region 单个品牌
                string sql = string.Format(@"select b.RegionName as 'BrandName',count(a.InvID) as 'ds'
                        ,count(DISTINCT a.TextileID) as 'ds2' from  InvoiceRFID a 
                        join Region b on b.id=a.HotelID
                        where a.BrandID={2} and a.invType=1
                           and  (a.InvCreateTime>= '{0}' && a.InvCreateTime < '{1}')
                        group by b.RegionName", sDate, eDate, brandId);

                var query = i_c_hzlksummary.SQLQuery(sql.ToString(), "");
                list = new List<C_HzlkSummary>();
                C_HzlkSummary ch = null;
                query.ToList().ForEach(q =>
                {
                    ch = new C_HzlkSummary();
                    ch.BrandName = q.BrandName;
                    ch.ds = q.ds;
                    ch.ds2 = q.ds2;
                    list.Add(ch);
                });


                #endregion
            }
            else
            {
                #region 多个品牌
                string sql = string.Format(@"select a.BrandID,a.BrandName,count(a.InvID) as 'ds',count(DISTINCT a.TextileID) 
                    as 'ds2' from InvoiceRFID a where a.invType=1 and (a.InvCreateTime>= '{0}'
                    and a.InvCreateTime < '{1}') GROUP BY a.BrandID,a.BrandName", sDate, eDate);
                var query = i_c_hzlksummary.SQLQuery(sql.ToString(), "");
                list = new List<C_HzlkSummary>();
                C_HzlkSummary ch = null;



                query.ToList().ForEach(q =>
                {
                    ch = new C_HzlkSummary();
                    ch.BrandID = q.BrandID;
                    ch.BrandName = q.BrandName;
                    ch.ds = q.ds;
                    ch.ds2 = q.ds2;
                    list.Add(ch);
                });
                if (query.ToList().Count() == 1)
                {
                    return HzlkQuery(list[0].BrandID, startDate, endDate);
                }
                #endregion
            }



            JsonResult jr = new JsonResult() { Data = list, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;
        }
        #endregion

        #region 报废布草平均洗涤次数分析表
        public ActionResult Hzlk()
        {
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();
            ViewData["BrandList"] = BrandList;
            
            return View();
        }

        public ActionResult FBQuery(string startDate, string endDate)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(startDate))
                DateTime.TryParse(startDate, out sDate);
            if (!string.IsNullOrWhiteSpace(endDate))
                DateTime.TryParse(endDate, out eDate);

            eDate = eDate.AddDays(1);
            dynamic ttl = null;

          
            #region 新方法
            string sql1 = string.Format(@"SELECT
                        GROUP_CONCAT(CONCAT('SUM(IF(ScrapName=''',ScrapName,'''',',result,0)) AS ',ScrapName)
                        ) INTO @EE FROM scrap s where  s.IsDelete=0 and s.ScrapType=2;
                        SET @QQ=CONCAT('SELECT name as 品牌,ClassName,',LEFT(@EE,LENGTH(@EE)-1),',SUM(result) AS 报废合计,SUM(Washtimes) as 报废洗涤总数 FROM (SELECT BrandName as name,ClassName,SD.ScrapName,COUNT(*) AS result,SUM(Washtime) AS Washtimes FROM scrapdetail AS SD LEFT JOIN scrap AS S ON SD.ScrapID=S.ID 
                        WHERE S.ScrapType=2 and (SD.CreateTime>= \'{0}\' and SD.CreateTime < \'{1}\')
                        GROUP BY BrandName,ClassName,SD.ScrapName) as a GROUP BY name,ClassName');
                        PREPARE stmt2 FROM @QQ;
                        EXECUTE stmt2;", sDate, eDate);
            string sql2 = string.Format(@"select a.BrandName,a.ClassID,a.ClassName,b.FabricName,
                        count(*) as 'num',ClassLeft,sum(Washtime) as 'sums',
                        (sum(Washtime)/count(*)) as 'avgs'
                         from ScrapDetail a 
                        left join Fabric b on b.id=a.FabricID
                        join TextileClass c on a.ClassID=c.ID
                        where (a.CreateTime>= '{0}' and a.CreateTime < '{1}')
                        GROUP BY a.BrandName,a.ClassID,a.ClassName,b.FabricName ORDER BY c.sort", sDate, eDate);
            List<C_FBlkSummary> list2 = new List<C_FBlkSummary>();
            var cf = new C_FBlkSummary();
            var qu = i_c_FBlksummary.SQLQuery(sql2.ToString(), "");
            qu.ToList().ForEach(p =>
            {
                cf = new C_FBlkSummary();
                cf.avgs = p.avgs;
                cf.BrandName = p.BrandName;
                cf.ClassID = p.ClassID;
                cf.ClassLeft = p.ClassLeft;
                cf.ClassName = p.ClassName;
                cf.FabricName = p.FabricName;
                cf.num = p.num;
                cf.sums = p.sums;
                list2.Add(cf);
            });
            
            List<scrap> tl = i_scrap.Entities.Where(b => b.IsDelete == false&&b.ScrapType==2).OrderBy(b => b.ID).ToList();
            
            List<C_PropertyItem> lp = new List<C_PropertyItem>()
                {

                    new C_PropertyItem("品牌",typeof(string)),
                    new C_PropertyItem("ClassName",typeof(string)),
                    new C_PropertyItem("报废合计",typeof(int)),
                    new C_PropertyItem("报废洗涤总数",typeof(int))
                };
            tl.ForEach(t =>
            {
                C_PropertyItem p = new C_PropertyItem(t.ScrapName, typeof(string));
                lp.Add(p);
            });
            laundry_hotelEntities entities = new laundry_hotelEntities();
            ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(lp), sql1, "");
            var sql3 = string.Format("select BrandName as Cname4,count(DISTINCT ClassID) as Cid4 from ScrapDetail where CreateTime>= '{0}' and CreateTime < '{1}'  GROUP BY BrandName ", sDate, eDate);
            var myjson1 = "";
            if (ttl!=null)
            {
             
            var res = i_c_index4summary.SQLQuery(sql3.ToString(), "");
            var list4 =new List<C_Index4>();
            var l4 = new C_Index4();
            res.ToList().ForEach(p =>
            {
                l4 = new C_Index4();
                l4.Cid4 = p.Cid4;
                l4.Cname4 = p.Cname4;
                list4.Add(l4);
            });
           
            PropertyInfo[] props = null;
           
            var myjson2 = "";
            int j = 0;
            var pp = "";
            var cn = "";
            var cnames = "";
            foreach (var item in ttl)
            {
                 pp = "";
                 cn = "";
                 myjson2 = "";
                if (j == 0)
                {
                    props = item.GetType().GetProperties();
                }
                    
                    for (int i = 0; i < props.Length; i++)
                {
                    if (props[i].Name=="品牌")
                    {
                        pp = props[i].GetValue(item);
                    }
                    if (props[i].Name == "ClassName")
                    {
                        cn = props[i].GetValue(item);
                    }
                    if (props[i].Name != "品牌"&& props[i].Name != "ClassName")
                    {
                        myjson2 += "\"" + props[i].Name + "\":\"" + props[i].GetValue(item) + "\",";
                    }
                  
                }

                myjson2 = myjson2.Substring(0, myjson2.Length - 1)+"},";
                foreach (var i2 in list2)
                {
                    if (i2.BrandName==pp&&i2.ClassName==cn)
                    {
                        myjson1 += "{\"品牌\":\"" + i2.BrandName+ "\",";
                        if (cnames!= i2.BrandName)
                        {

                        for (int i = 0; i < list4.Count; i++)
                        {
                            if (list4[i].Cname4==i2.BrandName)
                            {
                                cnames = i2.BrandName;
                                myjson1 += "\"cnums\":\"" + list4[i].Cid4+"\",";
                            }
                           
                        }

                        }
                        else
                        {
                            myjson1 += "\"cnums\":\"0\",";
                        }
                        myjson1 += "\"品名\":\"" + i2.ClassName + "\",\"面料\":\"" + i2.FabricName + "\",\"数量\":\"" + i2.num + "\",\"标准寿命\":\"" + i2.ClassLeft + "\",\"洗涤总数\":\"" + i2.sums + "\",\"平均洗涤次数\":\"" + i2.avgs + "\",\"报废类型\":\"非正常报废\"," + myjson2;

                    }
                }
                j++;
            }
            if (myjson1.Length!=0)
            {
                myjson1 = "[" + myjson1.Substring(0, myjson1.Length - 1) + "]";
            }



            }

            JsonResult jr = new JsonResult() { Data = myjson1, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            return jr;



            #endregion

        }
        
        
        #endregion

        #region 布草返洗率分析表
        public ActionResult FBlk()
        {

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
            List<C_Index> list1 = null;
            eDate = eDate.AddDays(1);
            dynamic ttl = null;
            
            if (brandId > 0)
            {
                
                string sql1 = string.Format(@"SELECT
                                GROUP_CONCAT(CONCAT('SUM(IF(classname=''',classname,'''',',result,0)) AS ',classname)
                                ) INTO @EE FROM TextileClass tc where  IsDelete=0;
                                SET @QQ=CONCAT('SELECT name as 品牌,id,',LEFT(@EE,LENGTH(@EE)-1),',SUM(result) AS 合计 FROM (SELECT D.HotelID as id,R.RegionName as name, BrandName,ClassName,SUM(TextileCount) AS result FROM invoicedetail AS D LEFT JOIN region AS R ON D.HotelID=R.ID  
                                WHERE InvType=1 AND InvSubType IN(1,2)   AND (InvCreateTime >= \'{0}\' and InvCreateTime < \'{1}\')
                                GROUP BY D.HotelID,R.RegionName, BrandName,ClassName) as a GROUP BY name,id');
                                PREPARE stmt2 FROM @QQ;
                                EXECUTE stmt2;", sDate, eDate);
                string sql2 = string.Format(@"SELECT
                                GROUP_CONCAT(CONCAT('SUM(IF(classname=''',classname,'''',',result,0)) AS ',classname)
                                ) INTO @EE FROM TextileClass tc where  IsDelete=0;
                                SET @QQ=CONCAT('SELECT name as 品牌,id,',LEFT(@EE,LENGTH(@EE)-1),',SUM(result) AS 合计 FROM (SELECT D.HotelID as id,R.RegionName as name, BrandName,ClassName,SUM(TextileCount) AS result FROM invoicedetail AS D LEFT JOIN region AS R ON D.HotelID=R.ID  
                                WHERE InvType=1 AND InvSubType=3   AND (InvCreateTime >= \'{0}\' and InvCreateTime < \'{1}\')
                                GROUP BY D.HotelID,R.RegionName, BrandName,ClassName) as a GROUP BY name,id');
                                PREPARE stmt2 FROM @QQ;
                                EXECUTE stmt2;", sDate, eDate);

                List<textileclass> tl = i_textileclass.Entities.Where(b => b.IsDelete == false).OrderBy(b => b.Sort).ThenBy(b => b.ClassName).ToList();

                List<C_PropertyItem> lp = new List<C_PropertyItem>()
                {
                    new C_PropertyItem("id",typeof(int?)),
                    new C_PropertyItem("品牌",typeof(string)),
                    new C_PropertyItem("合计",typeof(int?))


                };
                tl.ForEach(t =>
                {
                    C_PropertyItem p = new C_PropertyItem(t.ClassName, typeof(string));
                    lp.Add(p);
                });
                laundry_hotelEntities entities = new laundry_hotelEntities();
                ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(lp), sql1, "");
                dynamic ttl2 = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(lp), sql2, "");
                List<dynamic> list = new List<dynamic>();
                    list.Add(ttl); list.Add(ttl2);
                return Json(list, JsonRequestBehavior.DenyGet);
            }
            else
            {
               
                string sql1 = string.Format(@"SELECT
                                GROUP_CONCAT(CONCAT('SUM(IF(classname=''',classname,'''',',result,0)) AS ',classname)
                                ) INTO @EE FROM TextileClass tc where  IsDelete=0;
                                SET @QQ=CONCAT('SELECT name as 品牌,id,',LEFT(@EE,LENGTH(@EE)-1),',SUM(result) AS 合计 FROM (SELECT BrandID as id,BrandName as name,ClassName,SUM(TextileCount) AS result FROM invoicedetail WHERE InvType=1 AND InvSubType IN(1,2) AND (InvCreateTime >=\'{0}\' and InvCreateTime < \'{1}\')
                                GROUP BY BrandID,BrandName,ClassName) as a GROUP BY name,id');
                                PREPARE stmt2 FROM @QQ;
                                EXECUTE stmt2;", sDate, eDate);
                string sql2 = string.Format(@"SELECT
                                GROUP_CONCAT(CONCAT('SUM(IF(classname=''',classname,'''',',result,0)) AS ',classname)
                                ) INTO @EE FROM TextileClass tc where  IsDelete=0;
                                SET @QQ=CONCAT('SELECT name as 品牌,id,',LEFT(@EE,LENGTH(@EE)-1),',SUM(result) AS 合计 FROM (SELECT BrandID as id,BrandName as name,ClassName,SUM(TextileCount) AS result FROM invoicedetail WHERE InvType=1 AND InvSubType=3   AND (InvCreateTime >=\'{0}\' and InvCreateTime < \'{1}\')
                                GROUP BY BrandID,BrandName,ClassName) as a GROUP BY name,id');
                                PREPARE stmt2 FROM @QQ;
                                EXECUTE stmt2;", sDate, eDate);

                List<textileclass> tl = i_textileclass.Entities.Where(b => b.IsDelete == false).OrderBy(b => b.Sort).ThenBy(b => b.ClassName).ToList();

                List<C_PropertyItem> lp = new List<C_PropertyItem>()
                {
                    new C_PropertyItem("id",typeof(int?)),
                    new C_PropertyItem("品牌",typeof(string)),
                    new C_PropertyItem("合计",typeof(int?))


                };
                tl.ForEach(t =>
                {
                    C_PropertyItem p = new C_PropertyItem(t.ClassName, typeof(string));
                    lp.Add(p);
                });
                laundry_hotelEntities entities = new laundry_hotelEntities();
                ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(lp), sql1, "");
                dynamic ttl2 = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(lp), sql2, "");
                List<dynamic> list = new List<dynamic>();
                list.Add(ttl); list.Add(ttl2);
                return Json(list, JsonRequestBehavior.DenyGet);
            }



            //JsonResult jr = new JsonResult() { Data = list1, JsonRequestBehavior = JsonRequestBehavior.DenyGet };

            //return jr;
        }
        #region 老方法
        //public List<C_Index2> IndexShow2(int bid,DateTime t1,DateTime t2,List<C_Index4> list4,int ids) {
        //    List<C_Index2> list2 = new List<C_Index2>();
        //    C_Index2 c2 = null;
        //    list4.ForEach(p=> {
        //        c2 = new C_Index2();
        //        if (ids == 0)
        //        {

        //          var ss = (from i in i_invoicedetail.Entities
        //                        where i.InvType == 1 && (i.InvSubType ==1 || i.InvSubType == 2) && i.BrandID == bid && i.ClassID == p.Cid4
        //                        && (i.InvCreateTime >= t1 && i.InvCreateTime < t2)
        //                        select i.TextileCount);
        //            var sums = 0;
        //            foreach (var item in ss)
        //            {
        //                if (item!=null)
        //                {
        //                    sums += item;
        //                }
        //            }
        //            c2.Cnum2= sums;
        //        }
        //        else
        //        {
        //            var ss = (from i in i_invoicedetail.Entities
        //                        where i.InvType == 1 && (i.InvSubType == 1 || i.InvSubType == 2) && i.BrandID == bid && i.ClassID == p.Cid4 && i.RegionID == ids
        //                        && i.InvCreateTime >= t1 && i.InvCreateTime < t2
        //                        select i.TextileCount);
        //            var sums = 0;
        //            foreach (var item in ss)
        //            {
        //                if (item != null)
        //                {
        //                    sums += item;
        //                }
        //            }
        //            c2.Cnum2 = sums;
        //        }
        //        c2.Cid2 = p.Cid4;
        //        list2.Add(c2);
        //    });

        //    return list2;
        //}
        //public List<C_Index3> IndexShow3(int bid, DateTime t1, DateTime t2, List<C_Index4> list4,int ids)
        //{
        //    List<C_Index3> list3 = new List<C_Index3>();
        //    C_Index3 c3 = null;
        //    list4.ForEach(p => {
        //        c3 = new C_Index3();
        //        if (ids==0)
        //        {
        //            var ss = (from i in i_invoicedetail.Entities
        //                        where i.InvType == 1 && i.InvSubType == 3 && i.BrandID == bid && i.ClassID == p.Cid4
        //                        && i.InvCreateTime >= t1 && i.InvCreateTime < t2
        //                        select i.TextileCount);
        //            var sums = 0;
        //            foreach (var item in ss)
        //            {
        //                if (item != null)
        //                {
        //                    sums += item;
        //                }
        //            }
        //            c3.Cnum3 = sums;
        //        }
        //        else
        //        {
        //            var ss = (from i in i_invoicedetail.Entities
        //                        where i.InvType == 1 && i.InvSubType == 3 && i.BrandID == bid && i.ClassID == p.Cid4 && i.RegionID==ids
        //                        && i.InvCreateTime >= t1 && i.InvCreateTime < t2
        //                        select i.TextileCount);
        //            var sums = 0;
        //            foreach (var item in ss)
        //            {
        //                if (item != null)
        //                {
        //                    sums += item;
        //                }
        //            }
        //            c3.Cnum3 = sums;
        //        }
        //        c3.Cid3 = p.Cid4;
        //        list3.Add(c3);
        //    });
        //    return list3;
        //}
        #endregion
        #endregion

    }
}