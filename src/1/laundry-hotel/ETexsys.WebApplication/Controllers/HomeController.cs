using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ETexsys.WebApplication.Controllers
{
    [Authorize(Roles = "Sys_User")]
    public class HomeController : Controller
    {
        [Dependency]
        public IRepository<sys_right> i_sys_right { get; set; }

        [Dependency]
        public IRepository<sys_user> i_sys_user { get; set; }

        [Dependency]
        public IRepository<sys_customer_right> i_sys_cus_right { get; set; }

        [Dependency]
        public IRepository<sys_customer> i_sys_cus { get; set; }

        [Dependency]
        public IRepository<sys_privilege> i_sys_privilege { get; set; }

        [Dependency]
        public IRepository<sys_user_role> i_sys_user_role { get; set; }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <returns></returns>
        public ActionResult LogoutLogin()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("index", "login", new { area = "" });
        }

        // GET: Sys_User/Home
        public ActionResult Index()
        {
            HttpCookie authCookie = HttpContext.Request. Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            int userId = Convert.ToInt32(authTicket.Name);


            sys_user userModel = i_sys_user.LoadEntities(p => p.UserID == userId).FirstOrDefault();
            List<sys_right> list = new List<sys_right>();

            if (userModel != null)
            {
                list = LoginUserManage.GetInstance().GetCurrentRight(userId);
            }
            else
            {
                return RedirectToAction("index", "login");
            }
            ViewData["username"] = LoginUserManage.GetInstance().GetLoginUser().LoginName;

            return View(LoginUserManage.GetInstance().GetMenuTree(list));
        }
    }
}