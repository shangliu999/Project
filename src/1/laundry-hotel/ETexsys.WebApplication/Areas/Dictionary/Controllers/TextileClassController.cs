using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using ETexsys.WebApplication.Models;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Dictionary.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class TextileClassController : Controller
    {
        [Dependency]
        public IRepository<textileclass> i_textileclass { get; set; }

        [Dependency]
        public IRepository<category> i_category { get; set; }

        [Dependency]
        public IRepository<unit> i_unit { get; set; }

        [Dependency]
        public IRepository<size> i_size { get; set; }

        [Dependency]
        public IRepository<classsize> i_classsize { get; set; }

        // GET: Dictionary/TextileClass
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            List<category> cateList = i_category.Entities.Where(v => v.IsDelete == false).ToList();
            ViewData["CateList"] = cateList;

            List<unit> unitList = i_unit.Entities.Where(v => v.IsDelete == false).ToList();
            ViewData["UnitList"] = unitList;

            List<size> sizeList = i_size.Entities.Where(v => v.IsDelete == false).ToList();
            ViewData["SizeList"] = sizeList;

            return View();
        }

        public ActionResult GetTextileClassList(ParamModel model)
        {
            var query = i_textileclass.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderByDescending(v => v.IsRFID).ThenBy(v => v.Sort).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from t in orderingQuery
                       join cate in i_category.Entities on t.CateID equals cate.ID into cate_join
                       from cate in cate_join.DefaultIfEmpty()
                       join u in i_unit.Entities on t.UnitID equals u.ID into u_join
                       from u in u_join.DefaultIfEmpty()
                       select new
                       {
                           t.ID,
                           t.ClassCode,
                           t.ClassLeft,
                           t.ClassName,
                           t.CreateTime,
                           t.IsRFID,
                           t.PackCount,
                           t.Sort,
                           cate.CateName,
                           u.UnitName
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

        public ActionResult GetTextileClass(int classId)
        {
            MsgModel msgModel = new MsgModel();

            textileclass model = i_textileclass.GetByKey(classId);

            List<classsize> list = i_classsize.Entities.Where(v => v.ClassID == classId).ToList();

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;
            msgModel.OtherResult = list;


            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddTextileClass(textileclass requestParam, string SizeIds)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_textileclass.Insert(requestParam);

            List<classsize> list = new List<classsize>();
            classsize model = null;

            int sid = 0;
            string[] sizeIds = SizeIds.Split(',');
            foreach (var item in sizeIds)
            {
                int.TryParse(item, out sid);
                if (sid > 0)
                {
                    model = new classsize();
                    model.ClassID = requestParam.ID;
                    model.SizeID = sid;
                    model.UpdateTime = DateTime.Now;
                    list.Add(model);
                }
            }

            i_classsize.Insert(list);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateTextileClass(textileclass requestParam, string SizeIds)
        {
            MsgModel msgModel = new MsgModel();

            textileclass model = i_textileclass.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.CateID = requestParam.CateID;
                model.ClassCode = requestParam.ClassCode;
                model.ClassLeft = requestParam.ClassLeft;
                model.ClassName = requestParam.ClassName;
                model.IsRFID = requestParam.IsRFID;
                model.PackCount = requestParam.PackCount;
                model.UnitID = requestParam.UnitID;
                model.Sort = requestParam.Sort;

                int rtn = i_textileclass.Update(model, false);

                List<classsize> list = new List<classsize>();
                classsize cmodel = null;

                i_classsize.Delete(p => p.ClassID == model.ID, false);
                int sid = 0;
                string[] sizeIds = SizeIds.Split(',');
                foreach (var item in sizeIds)
                {
                    int.TryParse(item, out sid);
                    if (sid > 0)
                    {
                        cmodel = new classsize();
                        cmodel.ClassID = model.ID;
                        cmodel.SizeID = sid;
                        cmodel.UpdateTime = DateTime.Now;
                        list.Add(cmodel);
                    }
                }

                i_classsize.Insert(list);

                msgModel.ResultCode = 0;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelTextileClass(int classId)
        {
            MsgModel msgModel = new MsgModel();

            textileclass model = i_textileclass.GetByKey(classId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_textileclass.Update(model, false);

                i_classsize.Delete(p => p.ClassID == model.ID);

                msgModel.ResultCode = 0;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckClassName(string ClassName, int ClassID)
        {
            //id为0表示新增
            bool result = false;
            if (ClassID > 0)
            {
                int count = i_textileclass.LoadEntities(v => v.ID != ClassID && v.ClassName == ClassName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_textileclass.LoadEntities(v => v.ClassName == ClassName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}