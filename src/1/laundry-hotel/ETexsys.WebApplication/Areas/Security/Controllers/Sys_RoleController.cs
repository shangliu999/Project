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
using System.Web.Security;

namespace ETexsys.WebApplication.Areas.Security.Controllers
{
    [Authorize(Roles = "Sys_User")]
    [AddressUrl]
    public class Sys_RoleController : Controller
    {
        [Dependency]
        public IRepository<sys_right> i_sys_right { get; set; }

        [Dependency]
        public IRepository<sys_role> i_sys_role { get; set; }

        [Dependency]
        public IRepository<sys_privilege> i_sys_privilege { get; set; }

        [Dependency]
        public IRepository<sys_user> i_sys_user { get; set; }

        [Dependency]
        public IRepository<sys_right_button> i_sys_right_btn { get; set; }

        [Dependency]
        public IRepository<sys_customer_right> i_cus_right { get; set; }


        // GET: Security/Sys_Role
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetSys_RoleList(ParamModel model)
        {
            var query = i_sys_role.Entities;

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.RoleID).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from t in orderingQuery
                       join su in i_sys_user.Entities on t.CreateUserID equals su.UserID
                       select new
                       {
                           t.RoleID,
                           t.RoleName,
                           t.RoleDesc,
                           t.CreateTime,
                           su.UName
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

        public ActionResult GetRole(int RoleID)
        {
            MsgModel msgModel = new MsgModel();

            sys_role model = i_sys_role.GetByKey(RoleID);

            msgModel.ResultCode = (model != null) ? 0 : 1;
            msgModel.Result = model;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddSys_Role(sys_role requestParam)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            int rtn = i_sys_role.Insert(requestParam);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateSys_Role(sys_role requestParam)
        {
            MsgModel msgModel = new MsgModel();

            sys_role roleModel = i_sys_role.GetByKey(requestParam.RoleID);

            roleModel.UpdateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            roleModel.UpdateTime = DateTime.Now;
            roleModel.RoleName = requestParam.RoleName;
            roleModel.RoleDesc = requestParam.RoleDesc; 

            int rtn = i_sys_role.Update(roleModel);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelSys_Role(int RoleID)
        {
            MsgModel msgModel = new MsgModel();

            int rtn = i_sys_role.Delete(RoleID);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckRoleName(string RoleName, int RoleID)
        {
            //id为0表示新增
            bool result = false;
            if (RoleID > 0)
            {
                int count = i_sys_role.LoadEntities(v => v.RoleID != RoleID && v.RoleName == RoleName).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_sys_role.LoadEntities(v => v.RoleName == RoleName).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetRoleRightList(int RoleID)
        {
            var query = i_sys_right.Entities;

            var orderingQuery = query.OrderBy(q => q.ApplicationID).ThenBy(q => q.RightParentID).ThenBy(q => q.RightCode);

            var cusright = i_cus_right.Entities;

            //角色对应的功能
            var roleRight = i_sys_privilege.Entities.Where(v => v.PrivilegeMaster == "Role" && v.PrivilegeMasterValue == RoleID && v.PrivilegeAccess == "Menu" && v.PrivilegeOperation == true);
            //角色对应的按钮
            var roleRightBtn = i_sys_privilege.Entities.Where(v => v.PrivilegeMaster == "Role" && v.PrivilegeMasterValue == RoleID && v.PrivilegeAccess == "Button" && v.PrivilegeOperation == true);


            var roleBtnQuery = (from a in i_sys_right_btn.Entities
                                join b in roleRightBtn
                                on a.BtnID equals b.PrivilegeAccessValue into b_join
                                from b in b_join.DefaultIfEmpty()
                                select new
                                {
                                    a.RightID,
                                    a.BtnID,
                                    a.BtnName,
                                    IsCheck = b == null ? false : true
                                }).ToList();

            var data = (from A in orderingQuery
                        join cr in cusright on A.RightID equals cr.RightID
                        join r in roleRight on A.RightID equals r.PrivilegeAccessValue into r_join
                        from r in r_join.DefaultIfEmpty()
                        select new
                        {
                            A.sys_application.ApplicationName,
                            A.ApplicationID,
                            A.RightCode,
                            A.RightIcon,
                            A.RightID,
                            A.RightName,
                            A.RightParentID,
                            A.RightSort,
                            A.RightType,
                            A.RightUrl,
                            A.ShowInMainMenu,
                            IsCheck = r == null ? false : true, //是否有权限，控制界面的复选框 
                        }).ToList();

            var d = from i in data
                    select new
                    {
                        i.RightID,
                        i.RightName,
                        i.RightParentID,
                        i.ApplicationName,
                        i.RightCode,
                        i.IsCheck,
                        RightBtn = roleBtnQuery.Where(v => v.RightID == i.RightID).OrderBy(v => v.BtnID).ToList()
                    };


            return Json(d.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetRoleRight(int roleId, string rightIds, string rightBtns)
        {
            MsgModel msgModel = new MsgModel();

            i_sys_privilege.Delete(p => p.PrivilegeMaster == "Role" && p.PrivilegeMasterValue == roleId, false);

            List<sys_privilege> list = new List<sys_privilege>();
            sys_privilege model = null;

            string[] rightId = rightIds.Split('|');
            foreach (var r in rightId)
            {
                int id = 0;
                int.TryParse(r, out id);
                if (id > 0)
                {
                    model = new sys_privilege();
                    model.PrivilegeMaster = "Role";
                    model.PrivilegeMasterValue = roleId;
                    model.PrivilegeAccess = "Menu";
                    model.PrivilegeAccessValue = id;
                    model.PrivilegeOperation = true;
                    list.Add(model);
                }
            }

            string[] btnId = rightBtns.Split('|');
            foreach (var b in btnId)
            {
                int id = 0;
                int.TryParse(b, out id);
                if (id > 0)
                {
                    model = new sys_privilege();
                    model.PrivilegeMaster = "Role";
                    model.PrivilegeMasterValue = roleId;
                    model.PrivilegeAccess = "Button";
                    model.PrivilegeAccessValue = id;
                    model.PrivilegeOperation = true;
                    list.Add(model);
                }
            }

            int rtn = i_sys_privilege.Insert(list);

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }
    }
}