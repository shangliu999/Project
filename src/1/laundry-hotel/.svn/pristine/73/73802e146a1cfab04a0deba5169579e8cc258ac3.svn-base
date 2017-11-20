using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using ETexsys.Cloud.API.Common;
using ETexsys.Common.SMS;
using ETexsys.IDAL;
using ETexsys.Model;
using Microsoft.Practices.Unity;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace ETexsys.Cloud.API.Controllers
{
    public class AccountController : ApiController
    {
        [Dependency]
        public IRepository<sys_privilege> i_sys_privilege { get; set; }

        [Dependency]
        public IRepository<sys_right> i_sys_right { get; set; }

        [Dependency]
        public IRepository<sys_customer_right> i_sys_cus_right { get; set; }

        [Dependency]
        public IRepository<sys_customer> i_sys_cus { get; set; }

        [Dependency]
        public IRepository<sys_user_role> i_sys_user_role { get; set; }

        [Dependency]
        public IRepository<sys_user_dataview> i_sys_user_dataview { get; set; }

        [Dependency]
        public IRepository<sys_user> ibr { get; set; }

        [Dependency]
        public IRepository<sys_role> i_role { get; set; }

        [Dependency]
        public IRepository<sys_privilege> i_privilege { get; set; }

        public MsgModel Login([FromBody]AccountParamModel requestParam)
        {
            MsgModel msgModel = new MsgModel();

            sys_user model = ibr.LoadEntities(p => p.LoginName == requestParam.LoginName).FirstOrDefault();
            if (model != null)
            {
                if (model.LoginPwd == requestParam.LoginPwd)
                {
                    if (model.IsLevel == false)
                    {
                        ResponseSysUserModel userModel = new ResponseSysUserModel();
                        userModel.UserID = model.UserID;
                        userModel.UserName = model.LoginName;
                        userModel.UName = model.UName;

                        List<ResponseSysRightModel> list = GetCurrentRight(model.UserID, requestParam.TerminalType);
                        userModel.SysRights = list;

                        sys_customer customerModel = i_sys_cus.Entities.FirstOrDefault();
                        ResponseSysCustomerModel rscm = null;
                        if (customerModel != null)
                        {
                            rscm = new ResponseSysCustomerModel();
                            rscm.ID = customerModel.ID;
                            rscm.SysCusName = customerModel.SysCusName;
                            rscm.Code = customerModel.Code;

                            userModel.SysCustomer = rscm;
                        }

                        msgModel.ResultCode = 0;
                        msgModel.Result = userModel;

                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, model.UserID.ToString(), DateTime.Now, DateTime.Now.AddDays(1), false, "Sys_User");

                        string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                        System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);

                        model.LastLoginTime = DateTime.Now;
                        ibr.Update(model);

                        msgModel.ResultMsg = encryptedTicket;

                        if (Global.Access_token.ContainsKey(model.UserID.ToString()))
                        {
                            Global.Access_token.Remove(model.UserID.ToString());
                        }

                        Global.Access_token.Add(model.UserID.ToString(), encryptedTicket);
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

            return msgModel;
        }

        public MsgModel WXAuthorization([FromBody]WeChatParamModel requestParam)
        {
            MsgModel msgModel = new MsgModel();

            sys_user model = ibr.LoadEntities(p => p.XCXOpenID == requestParam.openId && p.UserType == 2).FirstOrDefault();
            if (model != null)
            {

                if (model.IsLevel == false)
                {
                    ResponseSysUserModel userModel = new ResponseSysUserModel();
                    userModel.UserID = model.UserID;
                    userModel.UserName = model.LoginName;
                    userModel.UName = model.UName;

                    List<int> RightHotel = i_sys_user_dataview.LoadEntities(v => v.UserID == model.UserID).Select(v => v.RegionID).ToList();
                    userModel.RightHotel = RightHotel;

                    msgModel.ResultCode = 0;
                    msgModel.Result = userModel;

                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, model.UserID.ToString(), DateTime.Now, DateTime.Now.AddDays(1), false, "Sys_User");

                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                    model.LastLoginTime = DateTime.Now;
                    ibr.Update(model);

                    msgModel.ResultMsg = encryptedTicket;

                    if (Global.Access_token.ContainsKey(model.UserID.ToString()))
                    {
                        Global.Access_token.Remove(model.UserID.ToString());
                    }

                    Global.Access_token.Add(model.UserID.ToString(), encryptedTicket);
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
                msgModel.ResultMsg = "当前用户名未关联账号.";
            }

            return msgModel;
        }

        public MsgModel SMSCode([FromBody]SMSParamModel request)
        {
            MsgModel msgModel = new MsgModel();

            if (IsHandset(request.tel))
            {
                sys_user model = ibr.Entities.Where(v => v.Phone == request.tel && v.UserType == 2 && v.IsLevel == false).FirstOrDefault();

                //if (model == null)
                //{
                //    model = new sys_user();
                //    model.CreateTime = DateTime.Now;
                //    model.CreateUserID = 0;
                //    model.Email = "";
                //    model.IsAdmin = false;
                //    model.IsLevel = false;
                //    model.LoginName = request.tel;
                //    model.LoginPwd = MD5Encrypt(request.tel);
                //    model.UserType = 2;
                //    model.UName = "";
                //    model.Phone = request.tel;
                //    ibr.Insert(model);

                //    sys_user_dataview dataview = new sys_user_dataview();
                //    dataview.UserID = model.UserID;
                //    dataview.RegionID = 23;
                //    dataview.CreateTime = DateTime.Now;
                //    dataview.IsDelete = false;
                //    i_sys_user_dataview.Insert(dataview);
                //}

                if (model != null)
                {
                    string code = MakeSMSCode();

                    model.SMSCode = code;
                    model.SMSTime = DateTime.Now;

                    string moblie = request.tel;
                    string key = ConfigurationManager.AppSettings["MoblieKey"];
                    string uid = ConfigurationManager.AppSettings["MoblieUid"];
                    string content = string.Format("验证码{0},您正在登录洗涤维生素平台,若非本人操作，请勿泄露。验证码有效期30分钟", code);

                    bool rtn = SMSTools.SendSMS(uid, key, moblie, content);
                    if (rtn)
                    {
                        msgModel.OtherCode = "0";
                        ibr.Update(model);
                    }
                    else
                    {
                        msgModel.OtherCode = "1";
                        msgModel.ResultMsg = "短信服务器异常.";
                    }
                }
                else
                {
                    msgModel.OtherCode = "1";
                    msgModel.ResultMsg = "当前手机系统未绑定";
                }
            }
            else
            {
                msgModel.OtherCode = "1";
                msgModel.ResultMsg = "手机号码格式不对";
            }

            return msgModel;
        }

        public MsgModel MoblieLogin([FromBody]SMSParamModel request)
        {
            MsgModel msgModel = new MsgModel();

            DateTime time = DateTime.Now.AddMinutes(-30);

            sys_user model = ibr.LoadEntities(p => p.Phone == request.tel && p.UserType == 2 && p.IsLevel == false).FirstOrDefault();
            if (model != null)
            {
                if (model.SMSCode == request.code)
                {
                    if (model.SMSTime >= time)
                    {
                        if (model.IsLevel == false)
                        {
                            ResponseSysUserModel userModel = new ResponseSysUserModel();
                            userModel.UserID = model.UserID;
                            userModel.UserName = model.LoginName;
                            userModel.UName = model.UName;

                            List<int> RightHotel = i_sys_user_dataview.LoadEntities(v => v.UserID == model.UserID).Select(v => v.RegionID).ToList();
                            userModel.RightHotel = RightHotel;

                            msgModel.ResultCode = 0;
                            msgModel.Result = userModel;

                            if (request.TerminalType == "WeChat")
                            {
                                model.XCXOpenID = request.UUID;
                            }

                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, model.UserID.ToString(), DateTime.Now, DateTime.Now.AddDays(1), false, "Sys_User");

                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

                            model.LastLoginTime = DateTime.Now;
                            ibr.Update(model);

                            msgModel.ResultMsg = encryptedTicket;

                            if (Global.Access_token.ContainsKey(model.UserID.ToString()))
                            {
                                Global.Access_token.Remove(model.UserID.ToString());
                            }

                            Global.Access_token.Add(model.UserID.ToString(), encryptedTicket);
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
                        msgModel.ResultMsg = "当前验证码已过期.";
                    }
                }
                else
                {
                    msgModel.ResultCode = 1;
                    msgModel.ResultMsg = "验证码错误.";
                }
            }
            else
            {
                msgModel.ResultCode = 1;
                msgModel.ResultMsg = "当前用户名未关联账号.";
            }

            return msgModel;
        }

        public MsgModel UserList()
        {
            MsgModel msgModel = new MsgModel();
            var query = from u in ibr.Entities
                        join s in i_sys_user_role.Entities on u.UserID equals s.UserID into s_join
                        from s in s_join.DefaultIfEmpty()
                        join p in i_privilege.Entities on s.RoleID equals p.PrivilegeMasterValue into p_join
                        from p in p_join.DefaultIfEmpty()
                        where p.PrivilegeAccess == "Menu" && u.IsLevel == false && u.UserType == 1
                        group u by new { t2=u.LoginName } into m
                        select new
                        {
                            LoginName=m.Key.t2
                        };
            msgModel.Result = query.ToList();
            return msgModel;
        }

        public MsgModel WeChatOpenId([FromBody]WeChatJSCodeParamModel requst)
        {
            MsgModel msg = new MsgModel();

            var result = OAuthApi.GetAccessToken(requst.appid, requst.secret, requst.js_code);
            if (result.errcode == ReturnCode.请求成功)
            {
                msg.Result = result.openid;
            }
            else
            {
                msg.ResultCode = 1;
            }

            return msg;
        }

        /// <summary>
        /// 验证手机号码是否合法
        /// </summary>
        /// <param name="str_handset"></param>
        /// <returns></returns>
        private bool IsHandset(string str_handset)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_handset, @"^1[3|4|5|7|8][0-9]\d{8}$");
        }

        /// <summary>
        /// 生成6位数字验证码
        /// </summary>
        /// <returns></returns>
        private string MakeSMSCode()
        {
            string tmpstr = "";
            string chars = "0123456789";
            //数组索引随机数    
            int iRandNum;
            //随机数生成器    
            Random rnd = new Random();
            for (int i = 0; i < 6; i++)
            {      //Random类的Next方法生成一个指定范围的随机数     
                iRandNum = rnd.Next(chars.Length);
                //tmpstr随机添加一个字符     
                tmpstr += chars[iRandNum];
            }
            return tmpstr;
        }

        ///   <summary>
        ///   给一个字符串进行MD5加密
        ///   </summary>
        ///   <param   name="strText">待加密字符串</param>
        ///   <returns>加密后的字符串</returns>
        public string MD5Encrypt(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strText));
            return System.Text.Encoding.Default.GetString(result);
        }

        #region 私有方法

        /// <summary>
        /// 获取用户功能列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ResponseSysRightModel> GetCurrentRight(int userId, string type)
        {
            sys_user userModel = ibr.LoadEntities(p => p.UserID == userId).FirstOrDefault();
            List<ResponseSysRightModel> list = new List<ResponseSysRightModel>();
            int AppliactionID = type == "Desktop" ? 2 : 3;

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
                    ResponseSysRightModel modelRight = null;
                    query.ToList().Where(v => r.Contains(v.RightID)).ToList().ForEach(q =>
                    {
                        modelRight = new ResponseSysRightModel();
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

            return list;
        }

        #endregion
    }
}
