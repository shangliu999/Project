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
    public class UnitController : Controller
    {
        [Dependency]
        public IRepository<unit> i_unit { get; set; }

        // GET: Dictionary/Unit
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetUnitList(ParamModel model)
        {
            var query = i_unit.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.ID).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery.ToList()
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnit(int unitId)
        {
            MsgModel msgModel = new MsgModel();

            unit model = i_unit.GetByKey(unitId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddUnit(unit requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_unit.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateUnit(unit requestParam)
        {
            MsgModel msgModel = new MsgModel();

            unit model = i_unit.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.UnitName = requestParam.UnitName; 

                int rtn = i_unit.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelUnit(int unitId)
        {
            MsgModel msgModel = new MsgModel();

            unit model = i_unit.GetByKey(unitId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_unit.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckUnitName(string UnitName, int UnitID)
        {
            //id为0表示新增
            bool result = false;
            if (UnitID > 0)
            {
                int count = i_unit.LoadEntities(v => v.ID != UnitID && v.UnitName == UnitName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_unit.LoadEntities(v => v.UnitName == UnitName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}