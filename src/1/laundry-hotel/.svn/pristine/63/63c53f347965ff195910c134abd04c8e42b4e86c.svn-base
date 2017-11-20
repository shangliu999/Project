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
    public class ScrapController : Controller
    {
        [Dependency]
        public IRepository<scrap> i_scrap { get; set; }

        // GET: Dictionary/Scrap
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetScrapList(ParamModel model)
        {
            var query = i_scrap.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.ID).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from t in orderingQuery
                       select new
                       {
                           t.ID,
                           t.ScrapName,
                           t.ScrapDesc,
                           t.CreateTime,
                           ScrapTypeName = t.ScrapType == 1 ? "正常" : "异常"
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

        public ActionResult GetScrap(int scrapId)
        {
            MsgModel msgModel = new MsgModel();

            scrap model = i_scrap.GetByKey(scrapId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddScrap(scrap requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_scrap.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateScrap(scrap requestParam)
        {
            MsgModel msgModel = new MsgModel();

            scrap model = i_scrap.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.ScrapName = requestParam.ScrapName;
                model.ScrapType = requestParam.ScrapType;
                model.ScrapDesc = requestParam.ScrapDesc;

                int rtn = i_scrap.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelScrap(int scrapId)
        {
            MsgModel msgModel = new MsgModel();

            scrap model = i_scrap.GetByKey(scrapId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_scrap.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckScrapName(string ScrapName, int ScrapID)
        {
            //id为0表示新增
            bool result = false;
            if (ScrapID > 0)
            {
                int count = i_scrap.LoadEntities(v => v.ID != ScrapID && v.ScrapName == ScrapName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_scrap.LoadEntities(v => v.ScrapName == ScrapName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}