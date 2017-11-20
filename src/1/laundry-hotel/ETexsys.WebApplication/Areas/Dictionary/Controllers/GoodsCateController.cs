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
    public class GoodsCateController : Controller
    {
        [Dependency]
        public IRepository<goodscate> i_goodscate { get; set; }

        // GET: Dictionary/GoodsCate
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetList(ParamModel model)
        {
            var query = i_goodscate.Entities.Where(v => v.Deleted == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.Sort).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery.ToList()
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetModel(int cateId)
        {
            MsgModel msgModel = new MsgModel();

            goodscate model = i_goodscate.GetByKey(cateId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult Add(goodscate requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.Deleted = false;
            int rtn = i_goodscate.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult Update(goodscate requestParam)
        {
            MsgModel msgModel = new MsgModel();

            goodscate model = i_goodscate.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.CateName = requestParam.CateName;
                model.Sort = requestParam.Sort;

                int rtn = i_goodscate.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult Delete(int cateId)
        {
            MsgModel msgModel = new MsgModel();

            goodscate model = i_goodscate.GetByKey(cateId);

            if (model != null)
            {
                model.Deleted = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_goodscate.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckCateName(string CateName, int CateID)
        {
            //id为0表示新增
            bool result = false;
            if (CateID > 0)
            {
                int count = i_goodscate.LoadEntities(v => v.ID != CateID && v.CateName == CateName && v.Deleted == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_goodscate.LoadEntities(v => v.CateName == CateName && v.Deleted == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}