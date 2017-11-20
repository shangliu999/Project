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

namespace ETexsys.WebApplication.Areas.Security.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class PersonnelPlaceController : Controller
    {
        [Dependency]
        public IRepository<personnelplace> i_personnelplace { get; set; }

        // GET: Security/PersonnelPlace
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetPersonnelPlaceList(ParamModel model)
        {
            var query = i_personnelplace.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.Sort).Skip(model.iDisplayStart).Take(model.iDisplayLength);


            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = orderingQuery
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPersonnelPlace(int perId)
        {
            MsgModel msgModel = new MsgModel();

            personnelplace model = i_personnelplace.GetByKey(perId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddPersonnelPlace(personnelplace requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_personnelplace.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdatePersonnelPlace(personnelplace requestParam)
        {
            MsgModel msgModel = new MsgModel();

            personnelplace model = i_personnelplace.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.PersonnelName = requestParam.PersonnelName;
                model.Responsibility = requestParam.Responsibility;
                model.Sort = requestParam.Sort;

                int rtn = i_personnelplace.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelPersonnelPlace(int perId)
        {
            MsgModel msgModel = new MsgModel();

            personnelplace model = i_personnelplace.GetByKey(perId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_personnelplace.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckPersonnelPlaceName(string PerName, int PerID)
        {
            //id为0表示新增
            bool result = false;
            if (PerID > 0)
            {
                int count = i_personnelplace.LoadEntities(v => v.ID != PerID && v.PersonnelName == PerName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_personnelplace.LoadEntities(v => v.PersonnelName == PerName && v.IsDelete == false).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}