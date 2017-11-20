using ETexsys.Common.ExcelHelp;
using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Dictionary.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class TextileController : Controller
    {
        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }

        [Dependency]
        public IRepository<textile> i_textile { get; set; }

        [Dependency]
        public IRepository<brandtype> i_brandtype { get; set; }

        [Dependency]
        public IRepository<textilebrandarea> i_textilebrandarea { get; set; }

        [Dependency]
        public IRepository<fabric> i_fabric { get; set; }

        [Dependency]
        public IRepository<color> i_color { get; set; }

        // GET: Dictionary/Textile
        public ActionResult Index()
        {
            List<textileclass> classList = i_textileclass.Entities.Where(v => v.IsDelete == false && v.IsRFID == true).ToList();
            ViewData["ClassList"] = classList;
            List<textilebrandarea> TextileBrandList = i_textilebrandarea.Entities.Where(v => v.IsDelete == false).ToList();
            ViewData["TextileBrandList"] = TextileBrandList;
            List<fabric> FabricList = i_fabric.Entities.Where(v => v.IsDelete == false).ToList();
            ViewData["FabricList"] = FabricList;
            List<color> ColorList = i_color.Entities.Where(v => v.IsDelete == false).ToList();
            ViewData["ColorList"] = ColorList;
            return View();
        }

        public ActionResult GetSizeListByClass(int classId)
        {
            var query = from s in i_size.Entities
                        join cs in i_classsize.Entities on s.ID equals cs.SizeID
                        where cs.ClassID == classId && s.IsDelete == false
                        orderby s.Sort, s.SizeName
                        select new
                        {
                            s.ID,
                            s.SizeName
                        };

            return Json(query, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTextileList(ParamModel model)
        {
            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(model.qCondition3))
                DateTime.TryParse(model.qCondition3, out sDate);
            if (!string.IsNullOrWhiteSpace(model.qCondition4))
                DateTime.TryParse(model.qCondition4, out eDate);

            eDate = eDate.AddDays(1);

            int classId = 0;
            int.TryParse(model.qCondition1, out classId);
            int sizeId = 0;
            int.TryParse(model.qCondition2, out sizeId);

            if (classId > 0)
            {
                if (sizeId > 0)
                {
                    var query = from t in i_textile.Entities
                                join tb in i_textilebrandarea.Entities on t.TextileBrandID equals tb.ID into tb_join
                                from tb in tb_join.DefaultIfEmpty()
                                join b in i_brandtype.Entities on t.BrandID equals b.ID
                                join c in i_textileclass.Entities on t.ClassID equals c.ID
                                join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                from s in s_join.DefaultIfEmpty()
                                join tc in i_color.Entities on t.ColorID equals tc.ID into tc_join
                                from tc in tc_join.DefaultIfEmpty()
                                join f in i_fabric.Entities on t.FabricID equals f.ID into f_join
                                from f in f_join.DefaultIfEmpty()
                                where t.IsFlag == 1 && t.RegisterTime >= sDate && t.RegisterTime < eDate && t.ClassID == classId && t.SizeID == sizeId
                                group t by new
                                {
                                    tb.BrandAreaName,
                                    b.BrandName,
                                    c.ClassName,
                                    s.SizeName,
                                    tc.ColorName,
                                    f.FabricName
                                } into m
                                orderby m.Key.BrandAreaName, m.Key.BrandName, m.Key.ClassName, m.Key.SizeName, m.Key.ColorName
                                select new
                                {
                                    BrandAreaName = m.Key.BrandAreaName,
                                    BrandName = m.Key.BrandName,
                                    ClassName = m.Key.ClassName,
                                    SizeName = m.Key.SizeName,
                                    ColorName = m.Key.ColorName,
                                    FabricName = m.Key.FabricName,
                                    TextileCount = m.Count()
                                };

                    var totalItemCount = query.Count();

                    var jsonData = new
                    {
                        sEcho = model.sEcho,
                        iTotalRecords = totalItemCount,
                        iTotalDisplayRecords = totalItemCount,
                        aaData = query
                    };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var query = from t in i_textile.Entities
                                join tb in i_textilebrandarea.Entities on t.TextileBrandID equals tb.ID into tb_join
                                from tb in tb_join.DefaultIfEmpty()
                                join b in i_brandtype.Entities on t.BrandID equals b.ID
                                join c in i_textileclass.Entities on t.ClassID equals c.ID
                                join s in i_size.Entities on t.SizeID equals s.ID into s_join
                                from s in s_join.DefaultIfEmpty()
                                join tc in i_color.Entities on t.ColorID equals tc.ID into tc_join
                                from tc in tc_join.DefaultIfEmpty()
                                join f in i_fabric.Entities on t.FabricID equals f.ID into f_join
                                from f in f_join.DefaultIfEmpty()
                                where t.IsFlag == 1 && t.RegisterTime >= sDate && t.RegisterTime < eDate && t.ClassID == classId
                                group t by new
                                {
                                    tb.BrandAreaName,
                                    b.BrandName,
                                    c.ClassName,
                                    s.SizeName,
                                    tc.ColorName,
                                    f.FabricName
                                } into m
                                orderby m.Key.BrandAreaName, m.Key.BrandName, m.Key.ClassName, m.Key.SizeName, m.Key.ColorName
                                select new
                                {
                                    BrandAreaName = m.Key.BrandAreaName,
                                    BrandName = m.Key.BrandName,
                                    ClassName = m.Key.ClassName,
                                    SizeName = m.Key.SizeName,
                                    ColorName = m.Key.ColorName,
                                    FabricName = m.Key.FabricName,
                                    TextileCount = m.Count()
                                };

                    var totalItemCount = query.Count();

                    var jsonData = new
                    {
                        sEcho = model.sEcho,
                        iTotalRecords = totalItemCount,
                        iTotalDisplayRecords = totalItemCount,
                        aaData = query
                    };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var query = from t in i_textile.Entities
                            join tb in i_textilebrandarea.Entities on t.TextileBrandID equals tb.ID into tb_join
                            from tb in tb_join.DefaultIfEmpty()
                            join b in i_brandtype.Entities on t.BrandID equals b.ID
                            join c in i_textileclass.Entities on t.ClassID equals c.ID
                            join s in i_size.Entities on t.SizeID equals s.ID into s_join
                            from s in s_join.DefaultIfEmpty()
                            join tc in i_color.Entities on t.ColorID equals tc.ID into tc_join
                            from tc in tc_join.DefaultIfEmpty()
                            join f in i_fabric.Entities on t.FabricID equals f.ID into f_join
                            from f in f_join.DefaultIfEmpty()
                            where t.IsFlag == 1 && t.RegisterTime >= sDate && t.RegisterTime < eDate
                            group t by new
                            {
                                tb.BrandAreaName,
                                b.BrandName,
                                c.ClassName,
                                s.SizeName,
                                tc.ColorName,
                                f.FabricName
                            } into m
                            orderby m.Key.BrandAreaName, m.Key.BrandName, m.Key.ClassName, m.Key.SizeName, m.Key.ColorName
                            select new
                            {
                                BrandAreaName = m.Key.BrandAreaName,
                                BrandName = m.Key.BrandName,
                                ClassName = m.Key.ClassName,
                                SizeName = m.Key.SizeName,
                                ColorName = m.Key.ColorName,
                                FabricName = m.Key.FabricName,
                                TextileCount = m.Count()
                            };

                var totalItemCount = query.Count();

                var jsonData = new
                {
                    sEcho = model.sEcho,
                    iTotalRecords = totalItemCount,
                    iTotalDisplayRecords = totalItemCount,
                    aaData = query
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateTextile(FormCollection f)
        {
            MsgModel msgModel = new MsgModel();

            DateTime sDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime eDate = sDate.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrWhiteSpace(f["beginTime"]))
                DateTime.TryParse(f["beginTime"], out sDate);
            if (!string.IsNullOrWhiteSpace(f["endTime"]))
                DateTime.TryParse(f["endTime"], out eDate);

            eDate = eDate.AddDays(1);

            int classId = 0, sizeId = 0, textileBrandId = 0, colorId = 0, fabricId = 0;
            int.TryParse(f["classId"], out classId);
            int.TryParse(f["sizeId"], out sizeId);
            int.TryParse(f["textileBrandId"], out textileBrandId);
            int.TryParse(f["colorId"], out colorId);
            int.TryParse(f["fabricId"], out fabricId);

            if (textileBrandId > 0 || colorId > 0 || fabricId > 0)
            {
                bool isAppend = false;

                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE textile SET ");

                if (textileBrandId > 0)
                {
                    isAppend = true;
                    sb.AppendFormat(" TextileBrandID={0} ", textileBrandId);
                }

                if (colorId > 0)
                {
                    if (isAppend)
                    {
                        sb.AppendFormat(",ColorID={0} ", colorId);
                    }
                    else
                    {
                        isAppend = true;
                        sb.AppendFormat(" ColorID={0} ", colorId);
                    }
                }

                if (fabricId > 0)
                {
                    if (isAppend)
                    {
                        sb.AppendFormat(",FabricID={0} ", fabricId);
                    }
                    else
                    {
                        isAppend = true;
                        sb.AppendFormat(" FabricID={0} ", fabricId);
                    }
                }

                sb.AppendFormat(" WHERE IsFlag=1 AND RegisterTime>='{0} 00:00:00' AND RegisterTime<'{1} 00:00:00' ", sDate.ToString("yyyy/MM/dd"), eDate.ToString("yyyy/MM/dd"));
                if (classId > 0)
                {
                    sb.AppendFormat(" AND ClassID={0} ", classId);
                }
                if (sizeId > 0)
                {
                    sb.AppendFormat(" AND SizeID={0} ", sizeId);
                }

                i_textile.ExecuteSql(sb.ToString());
                msgModel.ResultCode = 0;
            }


            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }
    }
}