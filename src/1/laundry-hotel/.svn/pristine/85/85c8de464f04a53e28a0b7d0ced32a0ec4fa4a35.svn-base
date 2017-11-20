using ETexsys.IDAL;
using ETexsys.Model;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ETexsys.WebApplication.Common
{
    public class LoginUserManage
    {
        private static LoginUserManage uniqueInstance;

        private static readonly object locker = new object();

        private static readonly object invLcker = new object();

        private LoginUserManage() { }

        public static LoginUserManage GetInstance()
        {
            if (uniqueInstance == null)
            {
                lock (locker)
                {
                    if (uniqueInstance == null)
                    {
                        uniqueInstance = new Common.LoginUserManage();
                        var container = new UnityContainer();
                        UnityConfigurationSection configuration = ConfigurationManager.GetSection(UnityConfigurationSection.SectionName) as UnityConfigurationSection;
                        configuration.Configure(container, "defaultContainer");
                        i_sys_user = container.Resolve<IRepository<sys_user>>();
                        i_sys_privilege = container.Resolve<IRepository<sys_privilege>>();
                        i_sys_right_btn = container.Resolve<IRepository<sys_right_button>>();
                        i_sys_user_role = container.Resolve<IRepository<sys_user_role>>();
                        i_sys_right = container.Resolve<IRepository<sys_right>>();
                        i_sys_cus_right = container.Resolve<IRepository<sys_customer_right>>();
                        i_sys_cus = container.Resolve<IRepository<sys_customer>>();
                        i_region = container.Resolve<IRepository<region>>();
                        i_sys_user_dataview = container.Resolve<IRepository<sys_user_dataview>>();
                    }
                }
            }

            return uniqueInstance;
        }

        public static IRepository<sys_user> i_sys_user { get; set; }

        public static IRepository<sys_privilege> i_sys_privilege { get; set; }

        public static IRepository<sys_right_button> i_sys_right_btn { get; set; }

        public static IRepository<sys_user_role> i_sys_user_role { get; set; }

        public static IRepository<sys_right> i_sys_right { get; set; }

        public static IRepository<sys_customer_right> i_sys_cus_right { get; set; }

        public static IRepository<sys_customer> i_sys_cus { get; set; }

        public static IRepository<region> i_region { get; set; }

        public static IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }

        /// <summary>
        /// 获取当前登录人ID
        /// </summary>
        /// <returns></returns>
        public int GetLoginUserId()
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            int userId = Convert.ToInt32(authTicket.Name);

            return userId;
        }

        public sys_user GetLoginUser()
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            int userId = Convert.ToInt32(authTicket.Name);

            sys_user user = i_sys_user.Entities.SingleOrDefault(c => c.UserID == userId);

            return user;
        }

        /// <summary>
        /// 获取当前功能操作列表
        /// </summary>
        /// <param name="rightId"></param>
        /// <returns></returns>
        public List<sys_right_button> GetOperationBtn(int rightId)
        {
            List<sys_right_button> list = new List<sys_right_button>();

            int userId = GetLoginUserId();

            sys_user userModel = i_sys_user.LoadEntities(v => v.UserID == userId).FirstOrDefault();

            if (userModel != null)
            {
                list = i_sys_right_btn.Entities.Where(v => v.RightID == rightId).ToList();
                if (userModel.IsAdmin == false)
                {
                    var userRightQuery = (from t in i_sys_privilege.Entities
                                          where t.PrivilegeMaster == "User" && t.PrivilegeMasterValue == userModel.UserID && t.PrivilegeAccess == "Button"
                                          select new
                                          {
                                              t.PrivilegeAccessValue,
                                              t.PrivilegeOperation
                                          }).ToList();

                    var userRoleQuery = (from t in i_sys_user_role.Entities
                                         join p in i_sys_privilege.Entities on t.RoleID equals p.PrivilegeMasterValue
                                         where t.UserID == userModel.UserID && p.PrivilegeMaster == "Role" && p.PrivilegeAccess == "Button"
                                         select new
                                         {
                                             p.PrivilegeAccessValue,
                                             p.PrivilegeOperation
                                         }).ToList();

                    //var removeBtn = userRightQuery.Where(v => v.PrivilegeOperation == false).Select(v => v.PrivilegeAccessValue).ToList();
                    //userRoleQuery.RemoveAll(v => removeBtn.Contains(v.PrivilegeAccessValue));

                    //userRightQuery.Where(v => v.PrivilegeOperation == true).ToList().ForEach(q =>
                    //{
                    //    if (userRoleQuery.Where(c => c.PrivilegeAccessValue == q.PrivilegeAccessValue).Count() == 0)
                    //    {
                    //        userRoleQuery.Add(new { q.PrivilegeAccessValue, q.PrivilegeOperation });
                    //    }
                    //});


                    var r = userRightQuery.Union(userRoleQuery).Where(x =>
                      userRightQuery.Where(y => y.PrivilegeOperation == false).Select(y => y.PrivilegeAccessValue).Contains(x.PrivilegeAccessValue) == false).Select(v => v.PrivilegeAccessValue).ToList();

                    list = list.Where(v => r.Contains(v.BtnID)).ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// 获取用户功能列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<sys_right> GetCurrentRight(int userId)
        {
            sys_user userModel = i_sys_user.LoadEntities(p => p.UserID == userId).FirstOrDefault();
            List<sys_right> list = new List<sys_right>();
            int AppliactionID = 0;
            int.TryParse(ConfigurationManager.AppSettings["AppliactionID"], out AppliactionID);

            if (userModel != null)
            {
                sys_customer customerModel = i_sys_cus.Entities.FirstOrDefault();
                if (customerModel != null)
                {
                    var query = from t in i_sys_right.Entities
                                join cr in i_sys_cus_right.Entities on t.RightID equals cr.RightID
                                where t.ApplicationID == AppliactionID && t.RightType == 1
                                select new
                                {
                                    t.RightID,
                                    t.RightName,
                                    t.ApplicationID,
                                    t.RightCode,
                                    t.RightIcon,
                                    t.RightParentID,
                                    t.RightSort,
                                    t.RightUrl,
                                    t.ShowInMainMenu
                                };

                    if (userModel.IsAdmin == true)
                    {

                        sys_right modelRight = null;
                        query.ToList().ForEach(q =>
                        {
                            modelRight = new sys_right();
                            modelRight.RightID = q.RightID;
                            modelRight.RightName = q.RightName;
                            modelRight.ApplicationID = q.ApplicationID;
                            modelRight.RightCode = q.RightCode;
                            modelRight.RightIcon = q.RightIcon;
                            modelRight.RightParentID = q.RightParentID;
                            modelRight.RightSort = q.RightSort;
                            modelRight.RightUrl = q.RightUrl;
                            modelRight.ShowInMainMenu = q.ShowInMainMenu;
                            list.Add(modelRight);
                        });
                    }
                    else
                    {

                        var userRightQuery = (from t in i_sys_privilege.Entities
                                              where t.PrivilegeMaster == "User" && t.PrivilegeMasterValue == userModel.UserID && t.PrivilegeAccess == "Menu"
                                              select new
                                              {
                                                  t.PrivilegeAccessValue,
                                                  t.PrivilegeOperation
                                              }).ToList();

                        var userRoleQuery = (from t in i_sys_user_role.Entities
                                             join p in i_sys_privilege.Entities on t.RoleID equals p.PrivilegeMasterValue
                                             where t.UserID == userModel.UserID && p.PrivilegeMaster == "Role" && p.PrivilegeAccess == "Menu"
                                             select new
                                             {
                                                 p.PrivilegeAccessValue,
                                                 p.PrivilegeOperation
                                             }).ToList();


                        var r = userRightQuery.Union(userRoleQuery).Where(x =>
                          userRightQuery.Where(y => y.PrivilegeOperation == false).Select(y => y.PrivilegeAccessValue).Contains(x.PrivilegeAccessValue) == false).Select(v => v.PrivilegeAccessValue).ToList();
                        sys_right modelRight = null;
                        query.ToList().Where(v => r.Contains(v.RightID)).ToList().ForEach(q =>
                        {
                            modelRight = new sys_right();
                            modelRight.RightID = q.RightID;
                            modelRight.RightName = q.RightName;
                            modelRight.ApplicationID = q.ApplicationID;
                            modelRight.RightCode = q.RightCode;
                            modelRight.RightIcon = q.RightIcon;
                            modelRight.RightParentID = q.RightParentID;
                            modelRight.RightSort = q.RightSort;
                            modelRight.RightUrl = q.RightUrl;
                            modelRight.ShowInMainMenu = q.ShowInMainMenu;
                            list.Add(modelRight);
                        });
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 获取菜单树形结构
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<sys_right> GetMenuTree(List<sys_right> list)
        {
            List<sys_right> newList = new List<sys_right>();
            newList.AddRange(list);

            var tempList = list.Where(v => v.RightParentID != null).Select(v => v.RightParentID).Distinct().ToList();
            sys_right model = null;

            foreach (var item in tempList)
            {
                if (newList.Where(v => v.RightID == item.Value).Count() == 0)
                {
                    model = i_sys_right.Entities.Where(v => v.RightID == item.Value).FirstOrDefault();

                    if (model != null)
                    {
                        newList.Add(model);

                        if (model.RightParentID != null)
                        {
                            model = i_sys_right.Entities.Where(v => v.RightID == model.RightParentID).FirstOrDefault();
                            if (model != null)
                                newList.Add(model);
                        }
                    }
                }
            }

            return newList;
        }

        /// <summary>
        /// 获取当前用户权限视图
        /// </summary>
        /// <returns></returns>
        public List<region> GetCurrentDataView()
        {
            int userId = GetLoginUserId();

            List<region> list = new List<region>();
            region model = null;

            var query = from r in i_region.Entities
                        join v in i_sys_user_dataview.Entities on r.ID equals v.RegionID
                        where v.IsDelete == false && r.IsDelete == false && v.UserID == userId
                        select new
                        {
                            r.ID,
                            r.FullName,
                            r.RegionName,
                            r.RegionType,
                            r.RegionMode,
                            r.ParentID,
                            r.BrandID,
                            r.Sort,
                            r.IsDelete,
                            r.Address,
                            r.LinkMan,
                            r.Tel
                        };

            query.ToList().ForEach(q =>
            {
                model = new region();
                model.ID = q.ID;
                model.FullName = q.FullName;
                model.RegionName = q.RegionName;
                model.RegionType = q.RegionType;
                model.RegionMode = q.RegionMode;
                model.ParentID = q.ParentID;
                model.BrandID = q.BrandID;
                model.Sort = q.Sort;
                model.IsDelete = q.IsDelete;
                model.Address = q.Address;
                model.LinkMan = q.LinkMan;
                model.Tel = q.Tel;
                list.Add(model);
            });

            return list;
        }

        /// <summary>
        /// 获取业务表单据流水号
        /// </summary>
        /// <returns></returns>
        public int GetBusInvoiceNuber()
        {
            lock (invLcker)
            {
                int rtn = 1;
                if (MvcApplication.BusInvoiceTime.ToString("yyyy/MM/dd") != DateTime.Now.ToString("yyyy/MM/dd"))
                {
                    MvcApplication.BusInvoiceTime = DateTime.Now;
                    MvcApplication.BusInvoiceNubmer = rtn;
                }
                else
                {
                    rtn = MvcApplication.BusInvoiceNubmer + 1;
                    MvcApplication.BusInvoiceNubmer = rtn;
                }
                return rtn;
            }
        }
    }
}