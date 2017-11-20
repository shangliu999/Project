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
    public class OrganiseController : Controller
    {
        [Dependency]
        public IRepository<organise> i_organise { get; set; }

        [Dependency]
        public IRepository<sys_user> i_sys_user { get; set; }

        // GET: Security/Organise
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;

            List<sys_user> userList = i_sys_user.Entities.Where(v => v.IsLevel == false && v.IsAdmin == false).ToList();
            ViewData["UserList"] = userList;

            return View();
        }

        public ActionResult GetOrganiseList(ParamModel model)
        {
            var query = i_organise.Entities.Where(v => v.IsDelete == false);

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.OrgCode).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from t in orderingQuery
                       join u in i_sys_user.Entities on t.PersonID equals u.UserID into u_join
                       from u in u_join.DefaultIfEmpty()
                       select new
                       {
                           t.ID,
                           t.FullName,
                           t.OrgCode,
                           t.ParentID,
                           t.Sort,
                           t.CreateTime,
                           u.UName
                       };
           data= data.OrderBy(p => p.Sort);

            var jsonData = new
            {
                sEcho = model.sEcho,
                iTotalRecords = totalItemCount,
                iTotalDisplayRecords = totalItemCount,
                aaData = data
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrganise(int orgId)
        {
            MsgModel msgModel = new MsgModel();

            organise model = i_organise.GetByKey(orgId);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddOrganise(organise requestParam)
        {
            MsgModel msgModel = new MsgModel();

            if (requestParam.ParentID == null)
            {
                string code = i_organise.LoadEntities(v => v.ParentID == null).Max(v => v.OrgCode);
                int count = (code == null || code == "") ? 0 : Convert.ToInt32(code.Substring(code.Length - 2));
                requestParam.OrgCode = (count + 1).ToString().PadLeft(2, '0');
            }
            else
            {
                string code = i_organise.LoadEntities(v => v.ParentID == requestParam.ParentID).Max(v => v.OrgCode);
                int count = (code == null || code == "") ? 0 : Convert.ToInt32(code.Substring(code.Length - 2));
                organise model = i_organise.LoadEntities(v => v.ID == requestParam.ParentID).FirstOrDefault();
                requestParam.OrgCode = model.OrgCode + (count + 1).ToString().PadLeft(2, '0');
            }
            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.IsDelete = false;
            int rtn = i_organise.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateOrganise(organise requestParam)
        {
            MsgModel msgModel = new MsgModel();

            organise model = i_organise.GetByKey(requestParam.ID);

            if (model != null)
            {
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                model.UpdateTime = DateTime.Now;
                model.FullName = requestParam.FullName;
                model.ParentID = requestParam.ParentID;
                model.PersonID = requestParam.PersonID;
                model.Sort = requestParam.Sort;

                if (model.ParentID != requestParam.ParentID)
                {
                    if (requestParam.ParentID == null)
                    {
                        string code = i_organise.LoadEntities(v => v.ParentID == null).Max(v => v.OrgCode);
                        int count = (code == null || code == "") ? 0 : Convert.ToInt32(code.Substring(code.Length - 2));
                        model.OrgCode = (count + 1).ToString().PadLeft(2, '0');
                    }
                    else
                    {
                        string code = i_organise.LoadEntities(v => v.ParentID == requestParam.ParentID).Max(v => v.OrgCode);
                        int count = (code == null || code == "") ? 0 : Convert.ToInt32(code.Substring(code.Length - 2));
                        organise tempModel = i_organise.LoadEntities(v => v.ID == requestParam.ParentID).FirstOrDefault();
                        model.OrgCode = tempModel.OrgCode + (count + 1).ToString().PadLeft(2, '0');
                    }
                }

                int rtn = i_organise.Update(model);

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelOrganise(int orgId)
        {
            MsgModel msgModel = new MsgModel();

            organise model = i_organise.GetByKey(orgId);

            if (model != null)
            {
                model.IsDelete = true;
                model.UpdateTime = DateTime.Now;
                model.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();

                int rtn = i_organise.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckOrganiseName(string OrganiseName, int OrganiseID, string ParentID)
        {
            //id为0表示新增
            bool result = false;
            if (OrganiseID > 0)
            {
                int count = 0;
                if (ParentID == null || ParentID == "")
                { count = i_organise.LoadEntities(v => v.ID != OrganiseID && v.FullName == OrganiseName && v.IsDelete == false && v.ParentID == null).Count(); }
                else
                {
                    count = i_organise.LoadEntities(v => v.ID != OrganiseID && v.FullName == OrganiseName && v.IsDelete == false && v.ParentID == int.Parse(ParentID)).Count();
                }

                result = count > 0 ? false : true;
            }
            else
            {
                int count = 0;
                if (ParentID == null || ParentID == "")
                { count = i_organise.LoadEntities(v => v.FullName == OrganiseName && v.IsDelete == false && v.ParentID == null).Count(); }
                else
                {
                    count = i_organise.LoadEntities(v => v.FullName == OrganiseName && v.IsDelete == false && v.ParentID == int.Parse(ParentID)).Count();
                }

                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}