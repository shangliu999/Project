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
    public class Hotel_UserController : Controller
    {
        [Dependency]
        public IRepository<sys_right_button> i_sys_right_btn { get; set; }

        [Dependency]
        public IRepository<sys_user> i_sys_user { get; set; }

        [Dependency]
        public IRepository<sys_role> i_sys_role { get; set; }

        [Dependency]
        public IRepository<sys_user_role> i_sys_user_role { get; set; }

        [Dependency]
        public IRepository<sys_customer_right> i_cus_right { get; set; }

        [Dependency]
        public IRepository<sys_right> i_sys_right { get; set; }

        [Dependency]
        public IRepository<sys_privilege> i_sys_privilege { get; set; }

        [Dependency]
        public IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }

        [Dependency]
        public IRepository<region> i_region { get; set; }

        [Dependency]
        public IRepository<sys_customer> i_sys_cus { get; set; }

        // GET: Security/Hotel_User
        public ActionResult Index()
        {
            int rightId = Convert.ToInt32(Request.Params["funcId"]);
            List<sys_right_button> btnList = LoginUserManage.GetInstance().GetOperationBtn(rightId);//  i_sys_right_btn.Entities.Where(v => v.RightID == rightId).ToList();
            List<sys_role> RoleList = i_sys_role.Entities.ToList();
            List<region> hotelList = i_region.Entities.Where(v => v.RegionType == 1 && v.IsDelete == false).OrderBy(v => v.BrandID).ThenBy(v => v.Sort).ToList();
            sys_customer customerModel = i_sys_cus.Entities.FirstOrDefault();

            sys_user sys_user = LoginUserManage.GetInstance().GetLoginUser();
            //StoreType 1租赁仓库 2周转仓库 3采购仓库
            int storeType = 3;
            int storeID = 0;
            if (sys_user != null && sys_user.StoreType != null)
            {
                storeType = (int)sys_user.StoreType;
                storeID = (int)sys_user.StoreID;
            }
            List<region> regionList = null;
            if (storeType == 3)
            {
                regionList = i_region.Entities.Where(c => c.RegionType == 3 && (c.RegionMode == 1 || c.RegionMode == 3)).ToList();
            }
            else
            {
                regionList = i_region.Entities.Where(c => c.ID == storeID).ToList();
            }

            ViewData["Code"] = customerModel.Code;

            ViewData["RegionList"] = regionList;
            ViewData["HotelList"] = hotelList;
            ViewData["RoleList"] = RoleList;
            ViewData["BtnList"] = btnList;
            return View();
        }

        public ActionResult GetSys_UserList(ParamModel model)
        {
            var query = i_sys_user.Entities.Where(v => v.UserType == 2);

            sys_user sys_user = LoginUserManage.GetInstance().GetLoginUser();
            //StoreType 1租赁仓库 2周转仓库 3采购仓库
            int storeType = 3;
            int storeID = 0;
            if (sys_user != null && sys_user.StoreType != null)
            {
                storeType = (int)sys_user.StoreType;
                storeID = (int)sys_user.StoreID;
            }

            if (storeType != 3)
            {
                query = query.Where(c => c.StoreID == storeID);
            }

            var totalItemCount = query.Count();
            var orderingQuery = query.OrderBy(v => v.OrganiseID).Skip(model.iDisplayStart).Take(model.iDisplayLength);

            var data = from t in orderingQuery
                       join d in i_sys_user_dataview.Entities on t.UserID equals d.UserID into d_join
                       from d in d_join.DefaultIfEmpty()
                       join h in i_region.Entities on d.RegionID equals h.ID into h_join
                       from h in h_join.DefaultIfEmpty()
                       join ru in i_sys_user_role.Entities on t.UserID equals ru.UserID
                       into ru_join
                       from r1 in ru_join.DefaultIfEmpty()
                       join r in i_sys_role.Entities on r1.RoleID equals r.RoleID
                       into r_join
                       from r in r_join.DefaultIfEmpty()
                       join re in i_region.Entities on t.StoreID equals re.ID
                       into re_join
                       from re in re_join.DefaultIfEmpty()
                       where h.RegionType == 1
                       select new
                       {
                           t.UserID,
                           t.LoginName,
                           t.UName,
                           HotelName = h == null ? "" : h.RegionName,
                           RoleName = r == null ? "" : r.RoleName,
                           t.Phone,
                           t.Email,
                           t.LastLoginTime,
                           t.IsLevel,
                           t.WXOpenID,
                           StoreName = re.RegionName,
                           StoreType = re.RegionMode,
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

        public ActionResult GetUser(int UserID)
        {
            MsgModel msgModel = new MsgModel();
            
            var query = from u in i_sys_user.Entities
                        join ru in i_sys_user_role.Entities on u.UserID equals ru.UserID into ru_join
                        from r1 in ru_join.DefaultIfEmpty()
                        join r in i_sys_role.Entities on r1.RoleID equals r.RoleID
                        into r_join
                        from r in r_join.DefaultIfEmpty()
                        join d in i_sys_user_dataview.Entities on u.UserID equals d.UserID into d_join
                        from d in d_join.DefaultIfEmpty()
                        where u.UserID == UserID
                        select new
                        {
                            u.CreateTime,
                            u.CreateUserID,
                            u.Email,
                            u.IsAdmin,
                            u.IsLevel,
                            u.LastLoginTime,
                            u.LoginName,
                            HotelID = d.RegionID,
                            u.LoginPwd,
                            u.Phone,
                            u.UName,
                            u.UserID,
                            u.UserType,
                            u.StoreID,
                            u.StoreType,
                            r.RoleName,
                            RoleID = r == null ? 0 : r.RoleID
                        };
            var data = query.FirstOrDefault();

            msgModel.ResultCode = (data != null) ? 0 : 1;
            msgModel.Result = data;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult AddSys_User(sys_user requestParam, int RoleID, int hotelId)
        {
            MsgModel msgModel = new MsgModel();

            requestParam.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
            requestParam.CreateTime = DateTime.Now;
            requestParam.UserType = 2;
            requestParam.IsAdmin = false;
            requestParam.IsLevel = false;
            int rtn = i_sys_user.Insert(requestParam);

            if (RoleID > 0)
            {
                sys_user_role userRole = new sys_user_role();
                userRole.UserID = requestParam.UserID;
                userRole.RoleID = RoleID;
                userRole.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                userRole.CreateTime = DateTime.Now;
                i_sys_user_role.Insert(userRole);
            }

            if (hotelId > 0)
            {
                sys_user_dataview dataview = new sys_user_dataview();
                dataview.UserID = requestParam.UserID;
                dataview.RegionID = hotelId;
                dataview.IsDelete = false;
                dataview.CreateTime = DateTime.Now;
                dataview.UpdateTime = DateTime.Now;
                i_sys_user_dataview.Insert(dataview);
            }

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UpdateSys_User(sys_user requestParam, int RoleID, int hotelId)
        {
            MsgModel msgModel = new MsgModel();

            sys_user modelUser = i_sys_user.GetByKey(requestParam.UserID);
            if (modelUser != null)
            {
                modelUser.StoreID = requestParam.StoreID;
                modelUser.StoreType = requestParam.StoreType;
                modelUser.OrganiseID = requestParam.OrganiseID;
                modelUser.PersonnelPlaceID = requestParam.PersonnelPlaceID;
                modelUser.Phone = requestParam.Phone;
                modelUser.Email = requestParam.Email;
                modelUser.UName = requestParam.UName;
                int rtn = i_sys_user.Update(modelUser);

                i_sys_user_role.Delete(p => p.UserID == requestParam.UserID);
                if (RoleID > 0)
                {
                    sys_user_role userRole = new sys_user_role();
                    userRole.UserID = requestParam.UserID;
                    userRole.RoleID = RoleID;
                    userRole.CreateUserID = LoginUserManage.GetInstance().GetLoginUserId();
                    userRole.CreateTime = DateTime.Now;
                    i_sys_user_role.Insert(userRole);
                }
                if (hotelId > 0)
                {
                    sys_user_dataview dataview = i_sys_user_dataview.Entities.Where(v => v.UserID == requestParam.UserID && v.IsDelete == false).FirstOrDefault();
                    if (dataview == null)
                    {
                        dataview = new sys_user_dataview();
                        dataview.UserID = requestParam.UserID;
                        dataview.RegionID = hotelId;
                        dataview.IsDelete = false;
                        dataview.CreateTime = DateTime.Now;
                        dataview.UpdateTime = DateTime.Now;
                        i_sys_user_dataview.Insert(dataview);
                    }
                    else
                    {
                        if (dataview.RegionID != hotelId)
                        {
                            dataview.RegionID = hotelId;
                            i_sys_user_dataview.Update(dataview);
                        }
                    }
                }

                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult UnBindingWX(int UserID)
        {
            MsgModel msgModel = new MsgModel();

            sys_user model = i_sys_user.GetByKey(UserID);
            if (model != null)
            {
                model.WXOpenID = "";
                int rtn = i_sys_user.Update(model);
                msgModel.ResultCode = (rtn > 0) ? 0 : 1;
            }

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult DelSys_User(int UserID)
        {
            MsgModel msgModel = new MsgModel();

            int rtn = i_sys_user.Delete(UserID);

            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult CheckUserName(string LoginName, int UserID)
        {
            //id为0表示新增
            bool result = false;
            if (UserID > 0)
            {
                int count = i_sys_user.LoadEntities(v => v.UserID != UserID && v.LoginName == LoginName).Count();
                result = count > 0 ? false : true;
            }
            else
            {
                int count = i_sys_user.LoadEntities(v => v.LoginName == LoginName).Count();
                result = count > 0 ? false : true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ChangeUserState(int userId)
        {
            MsgModel msgModel = new MsgModel();

            sys_user userModel = i_sys_user.GetByKey(userId);
            userModel.IsLevel = !userModel.IsLevel;
            int rtn = i_sys_user.Update(userModel);
            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public JsonResult InitializePwd(int userId, string pwd)
        {
            MsgModel msgModel = new MsgModel();

            sys_user userModel = i_sys_user.GetByKey(userId);
            userModel.LoginPwd = pwd;
            int rtn = i_sys_user.Update(userModel);
            msgModel.ResultCode = (rtn > 0) ? 0 : 1;

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }

        public ActionResult GetUserRightList(int UserID)
        {
            var query = i_sys_right.Entities;

            var orderingQuery = query.OrderBy(q => q.ApplicationID).ThenBy(q => q.RightParentID).ThenBy(q => q.RightCode);

            var cusright = i_cus_right.Entities;

            var roleModel = i_sys_user_role.Entities.Where(v => v.UserID == UserID).FirstOrDefault();
            int RoleID = roleModel == null ? 0 : roleModel.RoleID.Value;

            //角色对应的功能
            var roleRight = i_sys_privilege.Entities.Where(v => v.PrivilegeMaster == "Role" && v.PrivilegeMasterValue == RoleID && v.PrivilegeAccess == "Menu" && v.PrivilegeOperation == true).Select(v => new { v.PrivilegeAccessValue, v.PrivilegeOperation });
            //用户对应的功能（包含禁用）
            var userRight = i_sys_privilege.Entities.Where(v => v.PrivilegeMaster == "User" && v.PrivilegeMasterValue == UserID && v.PrivilegeAccess == "Menu").Select(v => new { v.PrivilegeAccessValue, v.PrivilegeOperation });
            //角色对应的按钮
            var roleRightBtn = i_sys_privilege.Entities.Where(v => v.PrivilegeMaster == "Role" && v.PrivilegeMasterValue == RoleID && v.PrivilegeAccess == "Button" && v.PrivilegeOperation == true).Select(v => new { v.PrivilegeAccessValue, v.PrivilegeOperation });
            //用户对应的操作按钮（包含禁用）
            var userRightBtn = i_sys_privilege.Entities.Where(v => v.PrivilegeMaster == "User" && v.PrivilegeMasterValue == UserID && v.PrivilegeAccess == "Button").Select(v => new { v.PrivilegeAccessValue, v.PrivilegeOperation });

            var right = roleRight.Union(userRight).Where(x =>
                 userRight.Where(y => y.PrivilegeOperation == false).Select(y => y.PrivilegeAccessValue).Contains(x.PrivilegeAccessValue) == false);

            var rightBtn = roleRightBtn.Union(userRightBtn).Where(x =>
                            userRightBtn.Where(y => y.PrivilegeOperation == false).Select(y => y.PrivilegeAccessValue).Contains(x.PrivilegeAccessValue) == false);


            var roleBtnQuery = (from a in i_sys_right_btn.Entities
                                join b in rightBtn
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
                        join r in right on A.RightID equals r.PrivilegeAccessValue into r_join
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

        public ActionResult SetUserRight(int userId, string rightIds, string rightBtns)
        {
            MsgModel msgModel = new MsgModel();

            var roleModel = i_sys_user_role.LoadEntities(p => p.UserID == userId).FirstOrDefault();
            int roleId = roleModel == null ? 0 : roleModel.RoleID.Value;
            //当前角色对应功能
            List<sys_privilege> oldRoleRightList = i_sys_privilege.LoadEntities(p => p.PrivilegeMaster == "Role" && p.PrivilegeMasterValue == roleId).ToList();
            //当前用户对应功能
            List<sys_privilege> oldUserRightList = i_sys_privilege.LoadEntities(p => p.PrivilegeMaster == "User" && p.PrivilegeMasterValue == userId).ToList();
            //用户待赋予的功能
            List<sys_privilege> list = new List<sys_privilege>();
            sys_privilege model = null;

            string[] rightId = rightIds.Split('|');
            foreach (var r in rightId)
            {
                int id = 0;
                int.TryParse(r, out id);
                if (id > 0)
                {
                    //角色对应功能不存在，则需要用户单独添加
                    model = oldRoleRightList.Where(v => v.PrivilegeAccessValue == id && v.PrivilegeAccess == "Menu").FirstOrDefault();
                    if (model == null)
                    {
                        model = oldUserRightList.Where(v => v.PrivilegeAccessValue == id && v.PrivilegeAccess == "Menu").FirstOrDefault();
                        //用户对应的功能不存在，则添加
                        if (model == null)
                        {
                            model = new sys_privilege();
                            model.PrivilegeMaster = "User";
                            model.PrivilegeMasterValue = userId;
                            model.PrivilegeAccess = "Menu";
                            model.PrivilegeAccessValue = id;
                            model.PrivilegeOperation = true;
                            list.Add(model);
                        }
                        else
                        {
                            //用户对应的功能存在 ，且是禁用的状态，则开启
                            if (model.PrivilegeOperation == false)
                            {
                                model.PrivilegeOperation = true;
                                i_sys_privilege.Update(model, false);
                            }
                        }
                    }
                }
            }

            //用户禁用的功能
            oldRoleRightList.Where(v => v.PrivilegeAccess == "Menu" && !rightId.Contains(v.PrivilegeAccessValue.Value.ToString())).ToList().ForEach(q =>
            {
                model = new sys_privilege();
                model.PrivilegeMaster = "User";
                model.PrivilegeMasterValue = userId;
                model.PrivilegeAccess = "Menu";
                model.PrivilegeAccessValue = q.PrivilegeAccessValue;
                model.PrivilegeOperation = false;
                list.Add(model);
            });

            string[] btnId = rightBtns.Split('|');
            foreach (var b in btnId)
            {
                int id = 0;
                int.TryParse(b, out id);
                if (id > 0)
                {
                    //角色对应按钮不存在，则需要用户单独添加
                    model = oldRoleRightList.Where(v => v.PrivilegeAccessValue == id && v.PrivilegeAccess == "Button").FirstOrDefault();
                    if (model == null)
                    {
                        model = oldUserRightList.Where(v => v.PrivilegeAccessValue == id && v.PrivilegeAccess == "Button").FirstOrDefault();
                        //用户对应的按钮不存在，则添加
                        if (model == null)
                        {
                            model = new sys_privilege();
                            model.PrivilegeMaster = "User";
                            model.PrivilegeMasterValue = userId;
                            model.PrivilegeAccess = "Button";
                            model.PrivilegeAccessValue = id;
                            model.PrivilegeOperation = true;
                            list.Add(model);
                        }
                        else
                        {
                            //用户对应的按钮存在 ，且是禁用的状态，则开启
                            if (model.PrivilegeOperation == false)
                            {
                                model.PrivilegeOperation = true;
                                i_sys_privilege.Update(model, false);
                            }
                        }
                    }
                }
            }

            //用户禁用的按钮
            oldRoleRightList.Where(v => v.PrivilegeAccess == "Button" && !btnId.Contains(v.PrivilegeAccessValue.Value.ToString())).ToList().ForEach(q =>
            {
                model = new sys_privilege();
                model.PrivilegeMaster = "User";
                model.PrivilegeMasterValue = userId;
                model.PrivilegeAccess = "Button";
                model.PrivilegeAccessValue = q.PrivilegeAccessValue;
                model.PrivilegeOperation = false;
                list.Add(model);
            });

            int rtn = i_sys_privilege.Insert(list);

            JsonResult jr = new JsonResult() { Data = msgModel };
            return jr;
        }
    }
}