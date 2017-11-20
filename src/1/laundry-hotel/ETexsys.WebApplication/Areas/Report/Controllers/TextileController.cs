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
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
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
    public class TextileController : Controller
    {
        [Dependency]
        public IRepository<textile> i_textile { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }
        [Dependency]
        public IRepository<size> i_size { get; set; }

        // GET: Report/Textile
        //布草分布表
        public ActionResult TextileDistribution()
        {
            
            List<textile> list= i_textile.Entities.Where(t=>t.IsFlag==1).ToList();
            var l = from t in list
                    group t by new { t.BrandID };
            ViewBag.Bid = l.Count() == 1 ? l.First().Key.BrandID : 0;

            ViewBag.time = DateTime.Now.ToString("yyyy-MM-dd");

            return View();
        }
        //  /Report/Textile/Index
        public ActionResult TextileQuery(int brandId)
        {
            DataTable dt=new DataTable();
            dynamic ttl=null;
            if (brandId > 0)
            {
                #region 单个品牌

                List<textileclass> tl = i_textileclass.Entities.Where(b=>b.IsDelete==false).OrderBy(b=>b.Sort).ThenBy(b => b.ClassName).ToList();

                var classsize = (from t in tl
                                join s in i_classsize.Entities
                                on t.ID equals s.ClassID
                                join sz in i_size.Entities
                                on s.SizeID equals sz.ID
                                orderby t.Sort, t.ClassName
                                select new { t.ClassName, sz.ID }).ToList();
                
                List<C_PropertyItem> lp = new List<C_PropertyItem>()
                {
                    new C_PropertyItem("位置",typeof(string)),
                    new C_PropertyItem("合计",typeof(int?))
                };
                classsize.ForEach(t =>
                {
                    C_PropertyItem p = new C_PropertyItem(t.ClassName + "×" + t.ID , typeof(string));
                    lp.Add(p);
                });
                string sql1 = string.Format(@"SELECT
                                             GROUP_CONCAT(CONCAT('SUM(IF(classname=''',CONCAT(tc.classname,'×',cs.SizeID),'''',',result,0)) AS ',CONCAT(tc.classname,'×',cs.SizeID))
                                            ) INTO @EE FROM TextileClass tc left join classsize cs on tc.id=cs.ClassID  where tc.IsDelete=0 ORDER BY tc.Sort,tc.ClassName;
                                            SET @QQ=CONCAT('SELECT name as 位置,SUM(result) AS 合计,',LEFT(@EE,LENGTH(@EE)-1),' FROM (
                                            select H.RegionName as name,tc.Sort,CONCAT(tc.classname,''×'',s.ID) AS classname,
                                            S.ID,COUNT(t.ID) AS result from textile t 
                                            left join region as H on t.HotelID=H.ID
                                            left join textileclass tc on tc.ID=t.ClassID 
                                            left join size AS s on t.sizeId=s.ID
                                            where t.IsFlag=1 AND t.TextileState IN (2,9)   and tc.IsDelete=0 and s.IsDelete=0   and t.BrandID={0} 
                                            GROUP BY H.ID,tc.Sort,tc.ClassName,s.ID
                                            UNION
                                            select R.RegionName as name,tc.Sort,CONCAT(tc.classname,''×'',s.ID) AS classname,
                                            S.ID,COUNT(t.ID) AS result from textile t 
                                            LEFT JOIN region AS R on t.RegionID=R.ID
                                            left join textileclass tc on tc.ID=t.ClassID 
                                            left join size AS s on t.sizeId=s.ID
                                            where t.IsFlag=1 AND t.TextileState IN (0,3)  and tc.IsDelete=0 and s.IsDelete=0  and t.BrandID={1} 
                                            GROUP BY R.ID,tc.Sort,tc.ClassName,s.ID
                                            UNION
                                            select ''工厂内部'' as name,tc.Sort,CONCAT(tc.classname,''×'',s.ID) AS classname,
                                            S.ID,COUNT(t.ID) AS result from textile t 
                                            left join textileclass tc on tc.ID=t.ClassID 
                                            left join size AS s on t.sizeId=s.ID
                                            where t.IsFlag=1 AND t.TextileState NOT IN (0,2,3,9) and tc.IsDelete=0 and s.IsDelete=0  and t.BrandID={2} 
                                            GROUP BY tc.Sort,tc.ClassName,s.ID
                                            ORDER BY name,Sort,ClassName
                                            ) as a GROUP BY name');
                                            PREPARE stmt2 FROM @QQ;
                                            EXECUTE stmt2;
                                            ", brandId, brandId,brandId);
                laundry_hotelEntities entities = new laundry_hotelEntities();
                ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(lp), sql1, "");

                dynamic brandname= i_brandtype.Entities.Where(b => b.ID == brandId).FirstOrDefault().BrandName;

                List<dynamic> list = new List<dynamic>();
                list.Add(ttl);
                list.Add(i_size.Entities.Where(b => b.IsDelete == false).ToList());
                list.Add(brandname);
                return Json(list, JsonRequestBehavior.DenyGet);
                #endregion
            }
            else
            {
                #region 多个品牌
                string sql = string.Format(@"SELECT
                                            GROUP_CONCAT(CONCAT('SUM(IF(classname=''',CONCAT(tc.classname,'×',cs.SizeID),'''',',result,0)) AS ',CONCAT(tc.classname,'×',cs.SizeID))
                                            ) INTO @EE FROM TextileClass tc left join classsize cs on tc.id=cs.ClassID where tc.IsDelete=0  ;
                                            SET @QQ=CONCAT('SELECT ifnull(name,\'合计\') as 流通品牌,id,SUM(result) AS 合计,',LEFT(@EE,LENGTH(@EE)-1),' 
                                            FROM (select bt.id as id,bt.BrandName as name,CONCAT(tc.ClassName,''×'',s.ID) as classname,count(t.ClassID) as result from textile t left join brandtype bt  on t.BrandID=bt.ID
                                            left join textileclass tc on tc.ID=t.ClassID left join size s on t.sizeId=s.ID 
                                            where t.IsFlag=1 and s.IsDelete=0 and tc.IsDelete=0 and RegisterTime<=\'{0}\' 
                                            GROUP BY  bt.id,bt.BrandName,tc.ClassName,s.ID order by bt.Sort,bt.BrandName) as a GROUP BY id,name WITH ROLLUP');
                                            PREPARE stmt2 FROM @QQ;
                                            EXECUTE stmt2;", DateTime.Now.ToString("yyyy-MM-dd"));

                List<textileclass> tl= i_textileclass.Entities.Where(b=>b.IsDelete==false).OrderBy(b => b.Sort).ThenBy(b=>b.ClassName).ToList();

                var classsize = (from t in tl
                                 join s in i_classsize.Entities
                                 on t.ID equals s.ClassID
                                 join sz in i_size.Entities
                                 on s.SizeID equals sz.ID
                                 orderby t.Sort, t.ClassName
                                 select new { t.ClassName, sz.ID }).ToList();

                List<C_PropertyItem> lp = new List<C_PropertyItem>()
                {
                    new C_PropertyItem("Id",typeof(int?)),
                    new C_PropertyItem("流通品牌",typeof(string)),
                    new C_PropertyItem("合计",typeof(int?))
                };
               
                classsize.ForEach(t =>
                {
                    C_PropertyItem p = new C_PropertyItem(t.ClassName + "×" + t.ID , typeof(string));
                    lp.Add(p);
                });

                laundry_hotelEntities entities = new laundry_hotelEntities();
                ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(lp), sql,"");

                List<dynamic> list = new List<dynamic>();
                list.Add(ttl);list.Add(i_size.Entities.Where(b => b.IsDelete == false).ToList());
                return Json(list,JsonRequestBehavior.DenyGet);
                #endregion
            }
        }
        //租赁布草洗涤次数分析表																						
        public ActionResult TextileWashtimes()
        {
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();
            ViewData["BrandList"] = BrandList;

            ViewBag.time = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }
        public ActionResult TextileWashtimesQuery(int brandId, string Washtimes)
        {
            dynamic ttl = null;
            List<dynamic> list = new List<dynamic>();

            List<string> l = new List<string>();
            l.Add("品名"); l.Add("0次"); l.Add("1-20次"); l.Add("21-40次"); l.Add("41-60次"); l.Add("61-80次");
            l.Add("81-100次"); l.Add("101-120次"); l.Add("121-140次"); l.Add("140次以上");
            l.Add("最小洗涤次数"); l.Add("最大洗涤次数"); l.Add("平均已洗涤次数"); l.Add("预计寿命");

            List<C_PropertyItem> pis = new List<C_PropertyItem>()
            {
                new C_PropertyItem("品名",typeof(string)),
                new C_PropertyItem("ID",typeof(string)),
            };

            l.ForEach(t =>
            {
                if (t != "品名")
                {
                    C_PropertyItem pi = new C_PropertyItem(t, typeof(int?));
                    pis.Add(pi);
                }
            });
            if (brandId > 0)
            {
                #region 单个品牌
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format(@"select c.ID,c.ClassName as '品名',
                            sum(case when t.Washtimes=0  then 1 else 0 end) as '0次',
                            sum(case when t.Washtimes > 0 and t.Washtimes <= 20 then 1 else 0 end) as '1-20次',
                            sum(case when t.Washtimes > 20 and t.Washtimes <= 40 then 1 else 0 end) as '21-40次',
                            sum(case when t.Washtimes > 40 and t.Washtimes <= 60 then 1 else 0 end) as '41-60次',
                            sum(case when t.washtimes > 60 and t.Washtimes <= 80 then 1 else 0 end) as '61-80次',
                            sum(case when t.Washtimes > 80 and t.Washtimes <= 100 then 1 else 0 end) as '81-100次',
                            sum(case when t.Washtimes > 100 and t.Washtimes <= 120 then 1 else 0 end) as '101-120次',
                            sum(case when t.Washtimes > 120 and t.Washtimes <= 140 then 1 else 0 end) as '121-140次',
                            sum(case when t.Washtimes > 140  then 1 else 0 end) as '140次以上',
                            min(t.Washtimes) as '最小洗涤次数',
                            max(t.Washtimes) as '最大洗涤次数',
                            sum(t.Washtimes)/count(t.id) as '平均已洗涤次数',
                            sum(c.ClassLeft-t.Washtimes) as '预计寿命'
                            from textile as t left join textileclass as c on t.ClassID = c.ID where t.IsFlag = 1 and t.BrandID={0} and  t.RegisterTime<='{1}' and c.IsDelete=0", brandId, DateTime.Now.ToString("yyyy-MM-dd")));

                if (Washtimes == "0")
                {
                    sb.Append(" group by c.ClassName,c.Sort,c.ID order by c.Sort,c.ClassName");
                }
                else
                {
                    if (Washtimes== "140次以上")
                    {
                        sb.Append(" and Washtimes>=140");
                    }
                    else
                    {
                        string[] str = Washtimes.Split('-');
                        sb.Append(string.Format(" and Washtimes>={0} and Washtimes<={1} ", int.Parse(str[0]),int.Parse(str[1])));
                    }
                    sb.Append(" group by c.ClassName,c.Sort,c.ID order by c.Sort,c.ClassName");
                }

                // 如果最小洗涤数量等于0
                string sql =string.Format(@"select min(t.Washtimes) as '最小洗涤次数',t.ClassID AS ID from textile t left join textileclass tc on t.ClassID=tc.ID   WHERE  t.Washtimes!=0 and t.IsFlag = 1 and tc.IsDelete=0 and t.BrandID={0}  GROUP BY ClassID;",brandId);

                laundry_hotelEntities entities = new laundry_hotelEntities();

                ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(pis), sb.ToString(), "");
                dynamic ttl2 = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(pis), sql, "");

                list.Add(ttl); list.Add(ttl2);
                #endregion
            }
            else
            {
                #region 多个品牌

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format(@"select c.ID,c.ClassName as '品名',
                            sum(case when t.Washtimes=0  then 1 else 0 end) as '0次',
                            sum(case when t.Washtimes > 0 and t.Washtimes <= 20 then 1 else 0 end) as '1-20次',
                            sum(case when t.Washtimes > 20 and t.Washtimes <= 40 then 1 else 0 end) as '21-40次',
                            sum(case when t.Washtimes > 40 and t.Washtimes <= 60 then 1 else 0 end) as '41-60次',
                            sum(case when t.washtimes > 60 and t.Washtimes <= 80 then 1 else 0 end) as '61-80次',
                            sum(case when t.Washtimes > 80 and t.Washtimes <= 100 then 1 else 0 end) as '81-100次',
                            sum(case when t.Washtimes > 100 and t.Washtimes <= 120 then 1 else 0 end) as '101-120次',
                            sum(case when t.Washtimes > 120 and t.Washtimes <= 140 then 1 else 0 end) as '121-140次',
                            sum(case when t.Washtimes > 140  then 1 else 0 end) as '140次以上',
                            min(t.Washtimes) as '最小洗涤次数',
                            max(t.Washtimes) as '最大洗涤次数',
                            sum(t.Washtimes)/count(t.id) as '平均已洗涤次数',
                            sum(c.ClassLeft-t.Washtimes) as '预计寿命'
                            from textile as t left join textileclass as c on t.ClassID = c.ID   where IsFlag = 1 and c.IsDelete=0 and  t.RegisterTime<='{0}'", DateTime.Now.ToString("yyyy-MM-dd")));

                if (Washtimes == "0")
                {
                    sb.Append(" group by c.ClassName,c.Sort,c.ID order by c.Sort,c.ClassName");
                }
                else
                {
                    if (Washtimes == "140次以上")
                    {
                        sb.Append(" and t.Washtimes>=140");
                    }
                    else
                    {
                        string[] str = Washtimes.Split('-');
                        sb.Append(string.Format(" and t.Washtimes>={0} and t.Washtimes<={1}", int.Parse(str[0]), int.Parse(str[1])));
                    }
                    sb.Append(" group by c.ClassName,c.Sort,c.ID order by c.Sort,c.ClassName");
                }

                // 如果最小洗涤数量等于0
                string sql = @"select min(Washtimes) as '最小洗涤次数',ClassID AS ID from textile t left join textileclass tc on t.ClassID=tc.ID   WHERE  t.Washtimes!=0 and t.IsFlag = 1 and tc.IsDelete=0  GROUP BY ClassID;";

                laundry_hotelEntities entities = new laundry_hotelEntities();
                ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(pis), sb.ToString(), "");

                dynamic ttl2 = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(pis), sql, "");

                list.Add(ttl);list.Add(ttl2);
                #endregion
            }
            return Json(list, JsonRequestBehavior.DenyGet);
        }
      //租赁布草剩余价值分析表
        public ActionResult TextileSurplus()
        {
            List<brandtype> BrandList = i_brandtype.Entities.Where(v => v.IsDelete == false).OrderBy(v => v.Sort).ThenBy(v => v.BrandName).ToList();
            ViewData["BrandList"] = BrandList;

            ViewBag.time = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }
        public ActionResult TextileSurplusQuery(int brandId, string Washtimes)
        {
            dynamic ttl = null;

            List<string> l = new List<string>();
            l.Add("品名"); l.Add("140次以上"); l.Add("121-140次"); l.Add("101-120次");
            l.Add("81-100次"); l.Add("61-80次"); l.Add("41-60次"); l.Add("21-40次");
            l.Add("1-20次");l.Add("0次"); 
            l.Add("最小剩余"); l.Add("最大剩余"); l.Add("平均剩余"); l.Add("预计寿命");l.Add("超出寿命");

            List<C_PropertyItem> pis = new List<C_PropertyItem>()
            {
                new C_PropertyItem("品名",typeof(string))
            };

            l.ForEach(t =>
            {
                if (t != "品名")
                {
                    C_PropertyItem pi = new C_PropertyItem(t, typeof(int?));
                    pis.Add(pi);
                }
            });
            if (brandId > 0)
            { 
                #region 单个品牌
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format(@"select c.ClassName as '品名',
                            sum(case when c.ClassLeft-t.Washtimes >140 then 1 else 0 end) as '140次以上',  
                            sum(case when c.ClassLeft-t.Washtimes >120 and  c.ClassLeft-t.Washtimes <=140 then 1 else 0 end) as '121-140次',
                            sum(case when c.ClassLeft-t.Washtimes >100 and  c.ClassLeft-t.Washtimes <=120 then 1 else 0 end) as '101-120次',
                            sum(case when c.ClassLeft-t.Washtimes >80 and  c.ClassLeft-t.Washtimes <=100 then 1 else 0 end) as '81-100次',
                            sum(case when c.ClassLeft-t.Washtimes >60 and  c.ClassLeft-t.Washtimes <=80 then 1 else 0 end) as '61-80次',
							sum(case when c.ClassLeft-t.Washtimes >40 and  c.ClassLeft-t.Washtimes <=60 then 1 else 0 end) as '41-60次',
                            sum(case when c.ClassLeft-t.Washtimes >20 and  c.ClassLeft-t.Washtimes <=40 then 1 else 0 end) as '21-40次',
                            sum(case when c.ClassLeft-t.Washtimes >0 and  c.ClassLeft-t.Washtimes <=20 then 1 else 0 end) as '1-20次' ,
                            sum(case when c.ClassLeft-t.Washtimes=0 then 1 else 0 end) as '0次',
                            min(c.ClassLeft-t.Washtimes) as '最小剩余',
                            max(c.ClassLeft-t.Washtimes) as '最大剩余',
                            (sum(c.ClassLeft-t.Washtimes))/count(t.id) as '平均剩余',
                            sum(c.ClassLeft-t.Washtimes) as '预计寿命',
                            case  when sum(c.ClassLeft)-sum(t.Washtimes)>0 then 0 else sum(t.Washtimes)-sum(c.ClassLeft) end as '超出寿命'
                            from textile as t left join textileclass as c on t.ClassID = c.ID  where t.IsFlag = 1 and t.BrandID={0} and t.RegisterTime<='{1}' and c.IsDelete=0", brandId, DateTime.Now.ToString("yyyy-MM-dd")));

                if (Washtimes == "0")
                {
                    sb.Append(" group by c.Sort,c.ClassName order by c.Sort,c.ClassName");
                }
                else
                {
                    if (Washtimes == "140次以上")
                    {
                        sb.Append(" and c.ClassLeft-Washtimes>=140");
                    }
                    else
                    {
                        string[] str = Washtimes.Split('-');
                        sb.Append(string.Format(" and c.ClassLeft-t.Washtimes>={0} and c.ClassLeft-t.Washtimes<={1}", int.Parse(str[0]), int.Parse(str[1])));
                    }
                    sb.Append(" group by c.Sort,c.ClassName order by c.Sort,c.ClassName");
                }
                laundry_hotelEntities entities = new laundry_hotelEntities();
                ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(pis), sb.ToString(), "");
                #endregion
            }
            else
            {
                #region 多个品牌
                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format(@"select c.ClassName as '品名',
                            sum(case when c.ClassLeft-t.Washtimes >140 then 1 else 0 end) as '140次以上', 
                            sum(case when c.ClassLeft-t.Washtimes >120 and  c.ClassLeft-t.Washtimes <=140 then 1 else 0 end) as '121-140次',
                            sum(case when c.ClassLeft-t.Washtimes >100 and  c.ClassLeft-t.Washtimes <=120 then 1 else 0 end) as '101-120次',
                            sum(case when c.ClassLeft-t.Washtimes >80 and  c.ClassLeft-t.Washtimes <=100 then 1 else 0 end) as '81-100次',
                            sum(case when c.ClassLeft-t.Washtimes >60 and  c.ClassLeft-t.Washtimes <=80 then 1 else 0 end) as '61-80次',
                            sum(case when c.ClassLeft-t.Washtimes >40 and  c.ClassLeft-t.Washtimes <=60 then 1 else 0 end) as '41-60次',
                            sum(case when c.ClassLeft-t.Washtimes >20 and  c.ClassLeft-t.Washtimes <=40 then 1 else 0 end) as '21-40次',
                            sum(case when c.ClassLeft-t.Washtimes >0 and  c.ClassLeft-t.Washtimes <=20 then 1 else 0 end) as '1-20次' ,
                            sum(case when c.ClassLeft-t.Washtimes=0 then 1 else 0 end) as '0次' ,
                            min(c.ClassLeft-t.Washtimes) as '最小剩余',
                            max(c.ClassLeft-t.Washtimes) as '最大剩余',
                            (sum(c.ClassLeft-t.Washtimes))/count(t.id) as '平均剩余',
                            sum(c.ClassLeft-t.Washtimes) as '预计寿命',
                            case  when sum(c.ClassLeft)-sum(t.Washtimes)>0 then 0 else sum(t.Washtimes)-sum(c.ClassLeft) end as '超出寿命'
                            from textile as t left join textileclass as c on t.ClassID = c.ID  where t.IsFlag = 1 and t.RegisterTime<='{0}' and c.IsDelete=0", DateTime.Now.ToString("yyyy-MM-dd")));

                if (Washtimes == "0")
                {
                    sb.Append(" group by c.Sort,c.ClassName order by c.Sort,c.ClassName");
                }
                else
                {
                    if (Washtimes == "140次以上")
                    {
                        sb.Append(" and  c.ClassLeft-t.Washtimes>=140");
                    }
                    else
                    {
                        string[] str = Washtimes.Split('-');
                        sb.Append(string.Format(" and c.ClassLeft-Washtimes>={0} and c.ClassLeft-Washtimes<={1}", int.Parse(str[0]), int.Parse(str[1])));
                    }
                    sb.Append(" group by c.Sort,c.ClassName order by c.Sort,c.ClassName");
                }
                laundry_hotelEntities entities = new laundry_hotelEntities();
                ttl = entities.Database.SqlQuery(C_TextileTypeFactory.GetTextileType(pis), sb.ToString(), "");
                #endregion
            }
            return Json(ttl,JsonRequestBehavior.DenyGet);
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
                    colModel.Width = 108;
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