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

namespace ETexsys.WebApplication.Controllers
{

    public class LoginController : Controller
    {
        [Dependency]
        public IRepository<sys_user> ibr { get; set; }

        [Dependency]
        public IRepository<sys_admin> sys_admin_ibr { get; set; }

        [Dependency]
        public IRepository<sys_customer> i_sys_cus { get; set; }

        // GET: Login
        public ActionResult Index()
        {
            sys_customer customerModel = i_sys_cus.Entities.FirstOrDefault();
            ViewData["Code"] = customerModel.Code;
            ViewBag.uid = Response.Cookies["userName"].Value == null ? "" : Response.Cookies["userName"].Value;
            return View();
        }

        public JsonResult CheckLogin(sys_user requestParam)
        {
            MsgModel msgModel = new MsgModel();
            sys_user model = ibr.LoadEntities(p => p.LoginName == requestParam.LoginName).FirstOrDefault();
            if (model != null)
            {
                if (model.LoginPwd == requestParam.LoginPwd)
                {
                    if (model.IsLevel == false)
                    {
                        msgModel.ResultCode = 0;
                        msgModel.Result = model;


                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, model.UserID.ToString(), DateTime.Now, DateTime.Now.AddMinutes(20), false, "Sys_User");

                        string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                        System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);

                        model.LastLoginTime = DateTime.Now;
                        ibr.Update(model);
                    }
                    else
                    {
                        msgModel.ResultCode = 1;
                        msgModel.ResultMsg = "当前账号已禁用.";
                    }
                }
                else
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "密码错误.";
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "当前用户名不存在.";
            }
            JsonResult jr = new JsonResult()
            {
                Data = msgModel
            };
            return jr;
        }

        public ActionResult SysAdmin()
        {
            return View();
        }

        public JsonResult SysCheckLogin(sys_admin requestParam)
        {
            MsgModel msgModel = new MsgModel();
            sys_admin model = sys_admin_ibr.LoadEntities(p => p.UserName == requestParam.UserName).FirstOrDefault();
            if (model != null)
            {
                if (model.UserPwd == requestParam.UserPwd)
                {
                    msgModel.ResultCode = 0;
                    msgModel.Result = model;

                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, model.UserName, DateTime.Now, DateTime.Now.AddMinutes(20), false, "Sys_Admin");
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                    HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);

                }
                else
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "密码错误.";
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "当前用户名不存在.";
            }
            JsonResult jr = new JsonResult()
            {
                Data = msgModel
            };
            return jr;
        }

        public ActionResult ViewPassword()
        {
            return View();
        }

        public ActionResult UpdatePassword(string oldPwd, string newPwd1, string newPwd2)
        {
            MsgModel msgModel = new MsgModel();

            sys_user user = LoginUserManage.GetInstance().GetLoginUser();

            if (user.LoginPwd.Equals(oldPwd))
            {
                if (!newPwd1.Equals(newPwd2))
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "修改失败，新密码和确认新密码不一致.";
                }
                else
                {
                    user.LoginPwd = newPwd1;
                    ibr.Update(user);
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "修改失败，旧密码错误.";
            }

            JsonResult jr = new JsonResult()
            {
                Data = msgModel
            };

            return jr;
        }
    }
}