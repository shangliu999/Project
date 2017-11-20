using MySql.Data.MySqlClient;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using WeChatAPI.DB;
using WeChatAPI.FileTools;
using WeChatAPI.Models;

namespace WeChatAPI.Controllers
{
    public class ScanController : Controller
    {
        public static readonly string AppSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];//与微信公众账号后台的Token设置保持一致，区分大小写。 
        public static readonly string AppId = WebConfigurationManager.AppSettings["WeixinAppId"];//与微信公众账号后台的AppId设置保持一致，区分大小写。 

        private static readonly string path = AppDomain.CurrentDomain.BaseDirectory + "ConnectionString.ini";

        // GET: QRCodeScan
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult B(string scode, int userId, long t)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

            if (ts.TotalSeconds - t > 60 * 5)
            {
                ViewData["effective"] = 0;
            }
            else
            {
                ViewData["effective"] = 1;
            }

            WeChatConfigModel wechatConfigModel = null;

            if (!string.IsNullOrWhiteSpace(scode))
            {
                string connectionString = INIOperation.INIGetStringValue(path, "connectionString", scode, "");

                wechatConfigModel = GetWechatConfig(connectionString);
            }
            ViewData["AppId"] = wechatConfigModel.AppId;
            ViewData["AppSecret"] = wechatConfigModel.AppSecret;

            ViewData["Code"] = scode;
            ViewData["UserId"] = userId;
            return View();
        }

        public ActionResult BindingCallback(string access_code, string code, int userId)
        {
            MsgModel msg = new MsgModel();

            if (string.IsNullOrEmpty(access_code))
            {
                msg.ResultCode = 1;
                msg.ResultMsg = "未授权";
            }
            else
            {
                string connectionString = INIOperation.INIGetStringValue(path, "connectionString", code, "");

                WeChatConfigModel configModel = GetWechatConfig(connectionString);

                var result = OAuthApi.GetAccessToken(configModel.AppId, configModel.AppSecret, access_code);
                if (result.errcode == ReturnCode.请求成功)
                {
                    string openid = result.openid;

                    if (connectionString != "")
                    {
                        string _sql = string.Format("SELECT * FROM sys_user WHERE WXOpenID='{0}'", openid);
                        MySqlDataReader reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);
                        if (reader.Read())
                        {
                            msg.ResultCode = 0;
                            msg.OtherCode = "1";
                        }
                        else
                        {
                            string sql = string.Format("UPDATE sys_user SET WXOpenID='{1}' WHERE UserID={0}", userId, openid);

                            int rtn = DbHelperMySQL.ExecuteSql(sql, connectionString);

                            msg.ResultCode = rtn > 0 ? 0 : 1;
                            msg.OtherCode = "0";
                        }
                    }

                }
                else
                {
                    msg.ResultCode = 1;
                    msg.ResultMsg = "获取信息失败";
                }
            }

            JsonResult jr = new JsonResult() { Data = msg };
            return jr;
        }

        public ActionResult L(string scode, string GUID, long t)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

            if (ts.TotalSeconds - t > 60 * 5)
            {
                ViewData["effective"] = 0;
            }
            else
            {
                ViewData["effective"] = 1;
            }
            WeChatConfigModel wechatConfigModel = null;

            if (!string.IsNullOrWhiteSpace(scode))
            {
                string connectionString = INIOperation.INIGetStringValue(path, "connectionString", scode, "");

                wechatConfigModel = GetWechatConfig(connectionString);
            }
            ViewData["AppId"] = wechatConfigModel.AppId;
            ViewData["AppSecret"] = wechatConfigModel.AppSecret;

            ViewData["Code"] = scode;
            ViewData["GUID"] = GUID;

            return View();
        }

        public ActionResult LoginCallback(string access_code, string code)
        {
            MsgModel msg = new MsgModel();

            if (string.IsNullOrEmpty(access_code))
            {
                msg.ResultCode = 1;
                msg.ResultMsg = "未授权";
            }
            else
            {
                string connectionString = INIOperation.INIGetStringValue(path, "connectionString", code, "");
                WeChatConfigModel wechatConfigModel = GetWechatConfig(connectionString);

                var result = OAuthApi.GetAccessToken(wechatConfigModel.AppId, wechatConfigModel.AppSecret, access_code);
                if (result.errcode == ReturnCode.请求成功)
                {
                    string openid = result.openid;
                    if (connectionString != "")
                    {
                        string _sql = string.Format("SELECT LoginName,LoginPwd FROM sys_user WHERE WXOpenID='{0}'", openid);
                        MySqlDataReader reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);
                        if (reader.Read())
                        {
                            string userName = reader["LoginName"].ToString();
                            string pwd = reader["LoginPwd"].ToString();
                            msg.Result = userName + "|" + pwd;
                            msg.ResultCode = 0;
                            msg.OtherCode = "0";
                        }
                        else
                        {
                            msg.ResultCode = 0;
                            msg.OtherCode = "1";
                        }
                    }

                }
                else
                {
                    msg.ResultCode = 1;
                    msg.ResultMsg = "获取信息失败";
                }
            }

            JsonResult jr = new JsonResult() { Data = msg };
            return jr;
        }

        public ActionResult R(string scode, string GUID, int id, long t)
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

            if (ts.TotalSeconds - t > 60 * 5)
            {
                ViewData["effective"] = 0;
            }
            else
            {
                ViewData["effective"] = 1;
            }
            WeChatConfigModel wechatConfigModel = null;

            if (!string.IsNullOrWhiteSpace(scode))
            {
                string connectionString = INIOperation.INIGetStringValue(path, "connectionString", scode, "");

                wechatConfigModel = GetWechatConfig(connectionString);
            }
            ViewData["AppId"] = wechatConfigModel.AppId;
            ViewData["AppSecret"] = wechatConfigModel.AppSecret;

            ViewData["Code"] = scode;
            ViewData["GUID"] = GUID;
            ViewData["ID"] = id;

            return View();
        }

        public ActionResult RecieveCallback(string access_code, string code, int id)
        {
            MsgModel msg = new MsgModel();

            if (string.IsNullOrEmpty(access_code))
            {
                msg.ResultCode = 1;
                msg.ResultMsg = "未授权";
            }
            else
            {
                string connectionString = INIOperation.INIGetStringValue(path, "connectionString", code, "");
                WeChatConfigModel wechatConfigModel = GetWechatConfig(connectionString);

                var result = OAuthApi.GetAccessToken(wechatConfigModel.AppId, wechatConfigModel.AppSecret, access_code);
                if (result.errcode == ReturnCode.请求成功)
                {
                    string openid = result.openid;
                    
                    if (connectionString != "")
                    {
                        string userName, pwd;
                        userName = pwd = string.Empty;

                        int userid = 0;
                        string _sql = string.Format("SELECT UserID,UName FROM sys_user WHERE WXOpenID='{0}'", openid);
                        MySqlDataReader reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);

                        Sys_UserModel sum = null;
                        if (reader.Read())
                        {
                            sum = new Sys_UserModel();
                            sum.UserID = Convert.ToInt32(reader["UserID"]);
                            userid = sum.UserID;
                            sum.UserName = reader["UName"].ToString();
                            //msg.ResultCode = 0;
                            //msg.OtherCode = "0";
                        }
                        else
                        {
                            msg.ResultCode = 0;
                            msg.OtherCode = "1";
                        }

                        if (userid != 0)
                        {
                            _sql = string.Format("SELECT UserID FROM sys_user_dataview WHERE UserID={0} AND RegionID={1}", userid, id);
                            reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);
                            if (reader.Read())
                            {
                                msg.Result = sum;
                                msg.ResultCode = 0;
                                msg.OtherCode = "0";
                            }
                            else
                            {
                                msg.ResultCode = 0;
                                msg.OtherCode = "2";
                            }
                        }
                    }
                }
                else
                {
                    msg.ResultCode = 1;
                    msg.ResultMsg = "获取信息失败";
                }
            }

            JsonResult jr = new JsonResult() { Data = msg };
            return jr;
        }

        public WeChatConfigModel GetWechatConfig(string connectionString)
        {
            WeChatConfigModel wechatConfigModel = null;

            string _sql = "SELECT WeixinAppId,WeixinAppSecret FROM sys_customer limit 1";

            MySqlDataReader reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);
            if (reader.Read())
            {
                wechatConfigModel = new WeChatConfigModel();
                wechatConfigModel.AppId = Convert.ToString(reader["WeixinAppId"]);
                wechatConfigModel.AppSecret = Convert.ToString(reader["WeixinAppSecret"]);
            }

            if (wechatConfigModel == null)
            {
                wechatConfigModel = new WeChatConfigModel();
                wechatConfigModel.AppSecret = AppSecret;
                wechatConfigModel.AppId = AppId;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(wechatConfigModel.AppId) || string.IsNullOrWhiteSpace(wechatConfigModel.AppSecret))
                {
                    wechatConfigModel.AppSecret = AppSecret;
                    wechatConfigModel.AppId = AppId;
                }
            }
            return wechatConfigModel;
        }
    }
}