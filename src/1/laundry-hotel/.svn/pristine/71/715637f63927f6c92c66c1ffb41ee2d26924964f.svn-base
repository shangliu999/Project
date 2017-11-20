using ETexsys.Common.Rabbit;
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
    public class InvoiceController : Controller
    {
        public static readonly string AppSecret = WebConfigurationManager.AppSettings["WeixinAppSecret"];//与微信公众账号后台的Token设置保持一致，区分大小写。 
        public static readonly string AppId = WebConfigurationManager.AppSettings["WeixinAppId"];//与微信公众账号后台的AppId设置保持一致，区分大小写。 

        private static readonly string path = AppDomain.CurrentDomain.BaseDirectory + "ConnectionString.ini";

        // GET: Invoice
        public ActionResult Index()
        {
            string code = Request.Params["scode"];
            string invId = Request.Params["id"];

            InvoiceModel model = null;
            WeChatConfigModel wechatConfigModel = null;

            if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(invId))
            {
                string connectionString = INIOperation.INIGetStringValue(path, "connectionString", code, "");

                if (connectionString != "")
                {
                    string _sql = string.Format("SELECT I.ID AS InvID,I.InvNo,R1.RegionName AS HotelName,R2.RegionName,I.CreateTime,I.CreateUserName,I.Quantity,InvType,InvSubType,Comfirmed,ConfirmUserName,ConfirmTime FROM invoice AS I LEFT JOIN region AS R1 ON I.HotelID = R1.ID LEFT JOIN region AS R2 ON I.RegionID = R2.ID WHERE I.ID = '{0}'", invId);
                    MySqlDataReader reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);

                    if (reader.Read())
                    {
                        model = new InvoiceModel();
                        model.Code = code;
                        model.InvID = reader["InvID"].ToString();
                        model.InvNo = reader["InvNo"].ToString();
                        model.HotelName = reader["HotelName"].ToString();
                        model.RegionName = reader["RegionName"].ToString();
                        model.CreateTime = Convert.ToString(reader["CreateTime"]);
                        model.CreateUserName = reader["CreateUserName"].ToString();
                        model.Total = Convert.ToInt32(reader["Quantity"]);
                        model.InvTypeName = Enum.GetName(typeof(InvoiceType), reader["InvType"]);
                        model.InvType = Convert.ToInt32(reader["InvType"]);
                        model.Comfirmed = Convert.ToInt32(reader["Comfirmed"]);
                        model.ConfirmUserName = Convert.ToString(reader["ConfirmUserName"]);
                        model.ConfirmTime = Convert.ToString(reader["ConfirmTime"]);
                    }

                    if (model != null)
                    {
                        _sql = string.Format("SELECT ClassName,SizeName,TextileCount FROM invoicedetail WHERE InvID='{0}'", model.InvID);
                        reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);
                        List<InvoiceDetailModel> list = new List<InvoiceDetailModel>();
                        InvoiceDetailModel detailModel = null;
                        while (reader.Read())
                        {
                            detailModel = new InvoiceDetailModel();
                            detailModel.ClassName = reader["ClassName"].ToString();
                            detailModel.SizeName = reader["SizeName"].ToString();
                            detailModel.TextileCount = Convert.ToInt32(reader["TextileCount"]);
                            list.Add(detailModel);
                        }
                        model.Detail = list;
                    }

                    wechatConfigModel = GetWechatConfig(connectionString);
                }
            }

            ViewData["AppId"] = wechatConfigModel.AppId;
            ViewData["AppSecret"] = wechatConfigModel.AppSecret;

            return View(model);
        }

        public ActionResult Comfirme(string id, string code, string access_code, int invType)
        {
            MsgModel msg = new MsgModel();

            try
            {
                if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(id))
                {
                    string connectionString = INIOperation.INIGetStringValue(path, "connectionString", code, "");

                    if (connectionString != "")
                    {
                        if (string.IsNullOrEmpty(access_code))
                        {
                            msg.ResultCode = 1;
                            msg.ResultMsg = "未授权";
                        }
                        else
                        {
                            WeChatConfigModel configModel = GetWechatConfig(connectionString);

                            var result = OAuthApi.GetAccessToken(configModel.AppId, configModel.AppSecret, access_code);
                            if (result.errcode == ReturnCode.请求成功)
                            {
                                string openid = result.openid;
                                string _sql = string.Format("SELECT UserID,LoginName,UName FROM sys_user WHERE WXOpenID='{0}'", openid);
                                MySqlDataReader reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);
                                int userId = 0;
                                string uName = "";
                                if (reader.Read())
                                {
                                    int.TryParse(Convert.ToString(reader["UserID"]), out userId);
                                    uName = Convert.ToString(reader["UName"]);
                                }
                                if (userId > 0)
                                {
                                    _sql = string.Format("UPDATE invoice SET Comfirmed=1,ConfirmUserName='{0}',ConfirmUserID='{1}',ConfirmTime='{2}' WHERE ID='{3}' ", uName, userId, DateTime.Now, id);
                                    int rtn = DbHelperMySQL.ExecuteSql(_sql, connectionString);
                                    msg.ResultCode = rtn > 0 ? 0 : 1;

                                    if (msg.ResultCode == 0 && invType == 1)
                                    {
                                        RabbitMQModel mqModel = new RabbitMQModel();
                                        mqModel.Type = "InvoiceSettle";
                                        mqModel.Value = id.ToString();
                                        mqModel.Code = code;
                                        RabbitMQHelper.Enqueue<RabbitMQModel>(mqModel);
                                    }
                                }
                                else
                                {
                                    msg.ResultCode = 1;
                                    msg.ResultMsg = "您未分配确认的权限";
                                }

                            }
                            else
                            {
                                msg.ResultCode = 1;
                                msg.ResultMsg = "获取信息失败";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.ResultCode = 1;
                msg.ResultMsg = ex.Message;
            }
            JsonResult jr = new JsonResult() { Data = msg };
            return jr;
        }

        public ActionResult ABComfirme(string no, string code, string access_code, string context)
        {
            MsgModel msg = new MsgModel();

            try
            {
                if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(no))
                {
                    string connectionString = INIOperation.INIGetStringValue(path, "connectionString", code, "");

                    if (connectionString != "")
                    {
                        if (string.IsNullOrEmpty(access_code))
                        {
                            msg.ResultCode = 1;
                            msg.ResultMsg = "未授权";
                        }
                        else
                        {
                            WeChatConfigModel configModel = GetWechatConfig(connectionString);

                            var result = OAuthApi.GetAccessToken(configModel.AppId, configModel.AppSecret, access_code);
                            if (result.errcode == ReturnCode.请求成功)
                            {
                                string openid = result.openid;
                                string _sql = string.Format("SELECT UserID,LoginName,UName FROM sys_user WHERE WXOpenID='{0}'", openid);
                                MySqlDataReader reader = DbHelperMySQL.ExecuteReader(_sql, connectionString);
                                int userId = 0;
                                string uName = "";
                                if (reader.Read())
                                {
                                    int.TryParse(Convert.ToString(reader["UserID"]), out userId);
                                    uName = Convert.ToString(reader["UName"]);
                                }
                                if (userId > 0)
                                {
                                    List<string> list = new List<string>();
                                    _sql = string.Format("UPDATE invoice SET Comfirmed=2,ConfirmUserName='{0}',ConfirmUserID='{1}',ConfirmTime='{2}' WHERE InvNo='{3}' ", uName, userId, DateTime.Now, no);
                                    list.Add(_sql);
                                    _sql = string.Format("INSERT INTO invoiceattach(InvNo,ParamType,ParamValue)VALUES('{0}','Feedback','{1}')", no, context);
                                    list.Add(_sql);
                                    int rtn = DbHelperMySQL.ExecuteSqlTran(list, connectionString);
                                    msg.ResultCode = rtn > 0 ? 0 : 1;
                                }
                                else
                                {
                                    msg.ResultCode = 1;
                                    msg.ResultMsg = "您未分配确认的权限";
                                }

                            }
                            else
                            {
                                msg.ResultCode = 1;
                                msg.ResultMsg = "获取信息失败";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg.ResultCode = 1;
                msg.ResultMsg = ex.Message;
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