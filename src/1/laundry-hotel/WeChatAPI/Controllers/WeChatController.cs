﻿using MySql.Data.MySqlClient;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WeChatAPI.DB;
using WeChatAPI.FileTools;
using WeChatAPI.Models;

namespace WeChatAPI.Controllers
{
    public class WeChatController : ApiController
    {
        private static readonly string path = AppDomain.CurrentDomain.BaseDirectory + "ConnectionString.ini";

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

        public MsgModel TextileTrack([FromBody]WeChatTextileTrackParamModel requst)
        {
            MsgModel msg = new MsgModel();

            string code = requst.QRCode.Substring(0, 1);

            string temp = requst.QRCode.Substring(3, requst.QRCode.Length - 8);
            DateTime stime = GetTime(temp);
            if (stime.ToString("yyyy/MM/dd") == "2017/08/28")
            {
                code = "D";
            }

            string connectionString = INIOperation.INIGetStringValue(path, "connectionString", code, "");
            string rfidconnectionString = INIOperation.INIGetStringValue(path, "rfidconnectionString", code, "");

            if (connectionString != "")
            {
                int textileId = 0;
                int hotelId = 0;
                string epc = "";
                string sql = string.Format("SELECT  ID,TagNo,textilestate,hotelid FROM textile WHERE  TagNo=(SELECT RFIDTagNo FROM qrcode WHERE ID IN(SELECT MAX(ID) FROM qrcode WHERE QRCodeValue = '{0}' GROUP BY QRCodeValue)) AND IsFlag = 1 ", requst.QRCode);
                using (MySqlDataReader reader = DbHelperMySQL.ExecuteReader(sql, connectionString))
                {
                    if (reader.Read())
                    {
                        textileId = reader["ID"] is DBNull ? 0 : Convert.ToInt32(reader["ID"]);
                        epc = Convert.ToString(reader["TagNo"]);
                        hotelId = (Convert.ToInt32(reader["textilestate"]) == 2 || Convert.ToInt32(reader["textilestate"]) == 9) ? Convert.ToInt32(reader["hotelid"]) : 0;
                    }
                }
                List<ResponseWeChatSuYuanModel> list = new List<ResponseWeChatSuYuanModel>();
                ResponseWeChatSuYuanModel model = null;
                ResponseWeChatModel rwmodel = new ResponseWeChatModel();

                #region 查询流程数据
                sql = string.Format("SELECT r.InvType,r.InvCreateTime,i.CreateUserName FROM invoicerfid as r LEFT join invoice as i on r.InvID=i.ID WHERE TextileID={0} AND r.ID>=(SELECT MAX(ID) FROM invoicerfid WHERE TextileID = {0} AND InvType = 1 GROUP BY TextileID) and(r.InvType = 1 or r.InvType = 2 or(r.InvType = 5 and r.InvSubType = 0) or(r.InvType = 3 and r.InvSubType = 22))", textileId);
                DateTime time = DateTime.MinValue;
                using (MySqlDataReader reader = DbHelperMySQL.ExecuteReader(sql, connectionString))
                {
                    while (reader.Read())
                    {
                        model = new ResponseWeChatSuYuanModel();
                        model.dDateTime = Convert.ToDateTime(reader["InvCreateTime"]);
                        model.date = model.dDateTime.ToString("MM-dd");
                        model.time = model.dDateTime.ToString("HH:mm");
                        model.userName = Convert.ToString(reader["CreateUserName"]);
                        model.type = Convert.ToInt32(reader["InvType"]);
                        model.name = model.type == 1 ? "酒店送洗" : (model.type == 5 ? "入厂" : (model.type == 3 ? "入库" : "配送"));
                        model.mark = "";
                        if (model.type == 1)
                        {
                            time = model.dDateTime;
                        }

                        list.Add(model);
                    }
                }
                #endregion

                //是否合格依据0合格，以洗涤龙&烫平读到为准
                int pass = 1;

                #region 查询 洗涤 平烫数据
                if (rfidconnectionString != "")
                {
                    try
                    {
                        string _sql = string.Format("SELECT SiteType,ScanTime,Note FROM rfid_reader as r left join rfid_scan_record as sr on r.ReaderCode=sr.SiteCode WHERE EPC ='{0}' AND ScanTime >'{1}' group by SiteType", epc, time);
                        using (MySqlDataReader reader = DbHelperMySQL.ExecuteReader(_sql, rfidconnectionString))
                        {
                            while (reader.Read())
                            {
                                model = new ResponseWeChatSuYuanModel();
                                model.dDateTime = Convert.ToDateTime(reader["ScanTime"]);
                                model.date = model.dDateTime.ToString("MM-dd");
                                model.time = model.dDateTime.ToString("HH:mm");
                                model.userName = "";
                                model.type = 100 + Convert.ToInt32(reader["SiteType"]);
                                model.name = model.type == 101 ? "洗涤" : "平烫";
                                model.mark = Convert.ToString(reader["Note"]);
                                pass = 0;
                                list.Add(model);
                            }
                        }

                    }
                    catch
                    {

                    }
                }
                #endregion

                list = list.OrderByDescending(v => v.dDateTime).ToList();
                rwmodel.pass = pass;
                rwmodel.hotelId = hotelId;
                rwmodel.textileId = textileId;
                rwmodel.code = code;

                msg.Result = list;
                msg.OtherResult = rwmodel;
            }

            return msg;
        }

        public MsgModel Textile([FromBody]WeChatTextileParamModel requst)
        {
            MsgModel msg = new MsgModel();
            string sql = string.Format("SELECT className,tba.BrandAreaName as producer,fabriclevel,fabricname,colorname,sizename,specifications,registerTime,TagNo FROM textile AS T LEFT JOIN textileclass as tc on t.classid=tc.id left JOIN size as s on t.sizeid=s.id left join color as c on t.colorid = c.id left join fabric as f on t.fabricid = f.id left join textilebrandarea as tba on T.TextileBrandID=tba.ID where t.id = {0}", requst.id);
            string connectionString = INIOperation.INIGetStringValue(path, "connectionString", requst.code, "");
            ResponseWeChatTextileModel model = null;
            using (MySqlDataReader reader = DbHelperMySQL.ExecuteReader(sql, connectionString))
            {
                if (reader.Read())
                {
                    model = new ResponseWeChatTextileModel();
                    model.textileName = Convert.ToString(reader["className"]);
                    model.producer = Convert.ToString(reader["producer"]);
                    model.level = Convert.ToString(reader["fabriclevel"]);
                    model.fabricName = Convert.ToString(reader["fabricname"]);
                    model.colorName = Convert.ToString(reader["colorname"]);
                    model.sizeName = Convert.ToString(reader["sizeName"]);
                    model.specifications = Convert.ToString(reader["specifications"]);
                    model.costtime = Convert.ToString(reader["registerTime"]);
                    model.tagno = Convert.ToString(reader["TagNo"]);
                }
            }
            msg.Result = model;
            return msg;
        }

        public MsgModel Hotel([FromBody]WeChatHotelParamModel requst)
        {
            MsgModel msg = new MsgModel();
            string sql = string.Format("SELECT Slogan,Logo,HotelName,HotelPoint,HotelProfile,HotelTel,HotelImg FROM hotel_detail WHERE HotelID={0}", requst.hotelid);
            string connectionString = INIOperation.INIGetStringValue(path, "connectionString", requst.code, "");
            ResponseWeChatHotelModel model = null;
            using (MySqlDataReader reader = DbHelperMySQL.ExecuteReader(sql, connectionString))
            {
                if (reader.Read())
                {
                    model = new ResponseWeChatHotelModel();
                    model.hotelSlogan = Convert.ToString(reader["Slogan"]);
                    model.hotelLogo = Convert.ToString(reader["Logo"]);
                    model.hotelName = Convert.ToString(reader["HotelName"]);
                    model.hotelProfile = Convert.ToString(reader["HotelProfile"]);
                    model.hotelPoint = Convert.ToString(reader["HotelPoint"]).Split(';');
                    model.hotelTel = Convert.ToString(reader["HotelTel"]);
                    model.hotelImg = Convert.ToString(reader["HotelImg"]).Split(';').Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
                }
                else
                {
                    model = new ResponseWeChatHotelModel();
                    model.hotelSlogan = "";
                    model.hotelLogo = "";
                    model.hotelName = "";
                    model.hotelProfile = "";
                    model.hotelTel = "";
                }
            }

            msg.Result = model;

            try
            {
                sql = string.Format("SELECT FactoryImg,Logo,FullName,Introduce,ServiceTel,SystemWebUrl,SystemApiUrl FROM sys_customer ");
                ResponseWeChatFactoryModel Fmodel = null;
                using (MySqlDataReader reader = DbHelperMySQL.ExecuteReader(sql, connectionString))
                {
                    if (reader.Read())
                    {
                        Fmodel = new ResponseWeChatFactoryModel();
                        Fmodel.factoryImg = Convert.ToString(reader["FactoryImg"]).Split(';').Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
                        Fmodel.factoryLogo = Convert.ToString(reader["Logo"]);
                        Fmodel.factoryName = Convert.ToString(reader["FullName"]);
                        Fmodel.factoryProfile = Convert.ToString(reader["Introduce"]);
                        Fmodel.factoryTel = Convert.ToString(reader["ServiceTel"]);
                        Fmodel.weburl = Convert.ToString(reader["SystemWebUrl"]);
                        Fmodel.apiurl = Convert.ToString(reader["SystemApiUrl"]) + "UploadFile/Customer/";
                    }
                    else
                    {
                        Fmodel = new ResponseWeChatFactoryModel();
                        Fmodel.factoryLogo = "";
                        Fmodel.factoryName = "";
                        Fmodel.factoryProfile = "";
                        Fmodel.factoryTel = "";
                        Fmodel.weburl = "";
                        Fmodel.apiurl = "";
                    }
                }
                msg.OtherResult = Fmodel;
            }
            catch (Exception ex)
            {
                msg.ResultMsg = ex.Message;
            }
            return msg;
        }

        public MsgModel Factory([FromBody]WeChatHotelParamModel requst)
        {
            MsgModel msg = new MsgModel();
            string sql = string.Format("SELECT FactoryImg,Logo,FullName,Introduce,ServiceTel,SystemWebUrl FROM sys_customer ");
            string connectionString = INIOperation.INIGetStringValue(path, "connectionString", requst.code, "");
            ResponseWeChatFactoryModel model = null;
            using (MySqlDataReader reader = DbHelperMySQL.ExecuteReader(sql, connectionString))
            {
                if (reader.Read())
                {
                    model = new ResponseWeChatFactoryModel();
                    model.factoryImg = Convert.ToString(reader["FactoryImg"]).Split(';').Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
                    model.factoryLogo = Convert.ToString(reader["Logo"]);
                    model.factoryName = Convert.ToString(reader["FullName"]);
                    model.factoryProfile = Convert.ToString(reader["Introduce"]);
                    model.factoryTel = Convert.ToString(reader["ServiceTel"]);
                    model.weburl = Convert.ToString(reader["SystemWebUrl"]);
                }
                else
                {
                    model = new ResponseWeChatFactoryModel();
                    model.factoryLogo = "";
                    model.factoryName = "";
                    model.factoryProfile = "";
                    model.factoryTel = "";
                    model.weburl = "";
                }
            }
            msg.Result = model;
            return msg;
        }

        private DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

            return dtStart.AddSeconds(double.Parse(timeStamp));
        }
    }
}
