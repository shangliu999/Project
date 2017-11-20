﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using CM = System.Configuration.ConfigurationManager;
using ETexsys.APIRequestModel.Response;
using System.Reflection;
using ETexsys.APIRequestModel.Request;

namespace ETextsys.Terminal.Utilities
{
    public class ApiController
    {
        private HttpClient client;

        private string token;
        
        #region 单例模式初始化

        private static readonly object obj = new object();

        private static volatile ApiController instance;
        public static ApiController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (obj)
                    {
                        if (null == instance)
                        {
                            instance = new ApiController();
                        }
                    }
                }
                return instance;
            }
        }

        #endregion

        #region 构造函数

        public ApiController()
        {
            client = new HttpClient();

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("token", token);
            }

            //基本的API URL
            client.BaseAddress = new Uri(BaseUrl);
            //默认希望响应使用Json序列化(内容协商机制，我接受json格式的数据)
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        #endregion

        #region get,post,login请求

        //post
        public async Task<MsgModel> DoPost(string url, object obj)
        {
            url = string.Format("{0}/api/{1}", BaseUrl, url);
            try
            {
                Task<HttpResponseMessage> hrm = client.PostAsJsonAsync(url, obj);


                var tmm = await await hrm.ContinueWith(x => x.Result.Content.ReadAsAsync<MsgModel>(
                      new List<MediaTypeFormatter>() { new JsonMediaTypeFormatter(), new XmlMediaTypeFormatter() }));

                if (hrm.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    tmm.ResultCode = 1;
                    tmm.ResultMsg = "服务器异常";
                }
                return tmm;
            }
            catch (Exception ex)
            {
                //throw ex;
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Info(ex);
            }

            MsgModel mm = new MsgModel() { ResultCode = 1 };
            return mm;
        }

        //get
        public async Task<MsgModel> DoGet(string url)
        {
            url = string.Format("{0}/api/{1}", BaseUrl, url);

            try
            {
                Task<HttpResponseMessage> hrm = client.GetAsync(url);

                var tmm = await await hrm.ContinueWith(x => x.Result.Content.ReadAsAsync<MsgModel>(
                    new List<MediaTypeFormatter>() { new JsonMediaTypeFormatter(), new XmlMediaTypeFormatter() }));

                if (hrm.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    tmm.ResultCode = 1;
                    tmm.ResultMsg = "服务器异常";
                }

                return tmm;
            }
            catch (Exception ex)
            {
                //throw ex;
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Info(ex);
            }
            MsgModel mm = new MsgModel() { ResultCode = 1 };
            return mm;
        }

        //login
        public async Task<MsgModel> DoLogin(string url, object obj)
        {
            url = string.Format("{0}/api/{1}", BaseUrl, url);
            try
            {
                Task<HttpResponseMessage> hrm = client.PostAsJsonAsync(url, obj);

                var tmm = await await hrm.ContinueWith(x => x.Result.Content.ReadAsAsync<MsgModel>(
                    new List<MediaTypeFormatter>() { new JsonMediaTypeFormatter(), new XmlMediaTypeFormatter() }));

                if (hrm.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    tmm.ResultCode = 1;
                    tmm.ResultMsg = "服务器异常";
                }
                else
                {
                    if (tmm.ResultCode == 0)
                    {
                        token = tmm.ResultMsg;
                        client.DefaultRequestHeaders.Add("access_token", token);
                    }
                }

                return tmm;
            }
            catch (Exception ex)
            {
                //throw ex;
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                log.Info(ex);
            }
            MsgModel mm = new MsgModel() { ResultCode = 1 };
            return mm;
        }

        #endregion

        #region api 接口路径

        /// <summary>
        /// apiurl
        /// </summary> 
        public string BaseUrl = "http://192.168.199.115:81/";
      //  public string BaseUrl = "https://api.langosys.com/ynkj/api/";

        /// <summary>
        /// loginurl
        /// </summary>
        public readonly string Login = "Account/Login";

        public readonly string GetUserList = "Account/UserList";

        public readonly string Register = "Textile/Register";

        public readonly string AssetsReg = "Textile/AssetsReg";

        public readonly string CheckSyn = "DataSync/Check";

        public readonly string FirSyn = "DataSync/FirSync";

        public readonly string SecSync = "DataSync/SecSync";

        public readonly string QRCodeRemoveBinding = "Textile/QRCodeRemoveBinding";

        public readonly string RFIDTagAnalysisByQRCode = "Textile/RFIDTagAnalysisByQRCode";

        public readonly string RFIDTagAnalysis = "Textile/RFIDTagAnalysis";

        public readonly string ComplexRFIDTagAnalysis = "Textile/ComplexRFIDTagAnalysis";

        public readonly string BagRFIDTagAnalysis = "Textile/BagRFIDTagAnalysis";

        public readonly string TruckRFIDTagAnalysis = "Textile/TruckRFIDTagAnalysis";

        public readonly string LoadTextileByTruck = "Textile/LoadTextileByTruck";

        public readonly string InsertRFIDInvoice = "Textile/InsertRFIDInvoice";

        public readonly string SendTask = "Textile/SendTask";

        public readonly string SendTask1 = "Textile/SendTask1";

        public readonly string SelectInvoice = "BillInquiry/SelectInvoice";

        public readonly string TextileScrap = "Textile/TextileScrap";

        public readonly string TextileReset = "Textile/TextileReset";

        public readonly string Details = "BillInquiry/Details";

        public readonly string Summary = "BillInquiry/Summary";

        public readonly string QRCodeBinding = "Textile/QRCodeBinding";

        public readonly string DeliveryTask = "BusinessInvoice/DeliveryTask";

        public readonly string RDIFReplace = "Textile/RFIDReplace";

        public readonly string HotelList = "BasicData/HotelList";

        public readonly string RegionList = "BasicData/RegionList";

        public readonly string StoreList = "BasicData/StoreList";

        public readonly string TextileClassList = "BasicData/TextileClassList";

        public readonly string BrandTypeList = "BasicData/BrandTypeList";

        public readonly string SizeList = "BasicData/ClassSizeList";

        public readonly string ColorList = "BasicData/ColorList";

        public readonly string FabricList = "BasicData/FabricList";

        public readonly string TextileBrandList = "BasicData/TextileBrandList";

        public readonly string ScrapList = "BasicData/ScrapList";

        public readonly string BagList = "BasicData/BagList";

        public readonly string TextileGrouping = "Textile/TextileGrouping";

        public readonly string RemoveTextileGroupingAnalysis = "Textile/RemoveTextileGroupingAnalysis";

        public readonly string RemoveTextileGrouping = "Textile/RemoveTextileGrouping";

        public readonly string InfactoryTask = "Textile/InfactoryTask";

        public readonly string QRCodeCheckOut = "Textile/QRCodeCheckOut";

        public readonly string TextileFlow = "Textile/TextileFlow";

        public readonly string TextileMergeTruck = "Textile/TextileMergeTruck";
        #endregion

        #region 公共方法

        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));

            return dtStart.AddSeconds(double.Parse(timeStamp));
        }

        #endregion

        #region 动画图片路径（无法扫描、开始扫描、暂停扫描）

        public readonly static string ApplicationPath = System.AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 无法扫描
        /// </summary>
        public readonly static string NotScan = string.Format("{0}/Skins/Default/Images/notscan.gif", ApiController.ApplicationPath);
        /// <summary>
        /// 扫描中
        /// </summary>
        public readonly static string Scan = string.Format("{0}/Skins/Default/Images/scan.gif", ApiController.ApplicationPath);
        /// <summary>
        /// 暂停扫描
        /// </summary>
        public readonly static string StopScan = string.Format("{0}/Skins/Default/Images/stopscan.gif", ApiController.ApplicationPath);

        #endregion
    }
}
