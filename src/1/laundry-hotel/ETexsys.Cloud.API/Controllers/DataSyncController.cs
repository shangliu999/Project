using ETexsys.APIRequestModel.Request;
using ETexsys.APIRequestModel.Response;
using ETexsys.Cloud.API.Common;
using ETexsys.Common.Sync;
using ETexsys.IDAL;
using ETexsys.Model;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ETexsys.Cloud.API.Controllers
{
    [SupportFilter]
    public class DataSyncController : ApiController
    {
        SqliteHelper sqlite = new SqliteHelper();

        /// <summary>
        /// 检测同步，根据版本号在服务器上查询版本的时间
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        public MsgModel Check([FromBody]SyncParamModel requestParam)
        {
            MsgModel mm = new MsgModel();

            bool isok = sqlite.Check(requestParam);

            mm.ResultCode = 0;

            if (isok)
            {
                mm.Result = 1;
            }
            else
            {
                mm.Result = 0;
            }

            return mm;
        }

        /// <summary>
        /// 首次同步，根据版本号在服务器上查询版本的时间
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        public MsgModel FirSync([FromBody]SyncParamModel requestParam)
        {
            //string path = HttpContext.Current.Request.MapPath("~");
            string path = AppDomain.CurrentDomain.BaseDirectory;

            MsgModel mm = new MsgModel();

            string zipfile = string.Empty;
            SyncParamModel model = sqlite.FirSync(path, ref zipfile, requestParam);

            if (string.IsNullOrEmpty(zipfile))
            {
                mm.ResultCode = 1;
                mm.ResultMsg = "数据压缩失败。";
            }
            else
            {
                mm.ResultCode = 0;
                model.FilePath = string.Format("DBData/{0}", zipfile);
                mm.Result = model;
            }

            return mm;
        }

        /// <summary>
        /// 二次同步，根据版本号在服务器上查询版本的时间
        /// </summary>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        public MsgModel SecSync([FromBody]SyncParamModel requestParam)
        {
            //string path = HttpContext.Current.Request.MapPath("~");
            string path = AppDomain.CurrentDomain.BaseDirectory;

            MsgModel mm = new MsgModel();

            string zipfile = string.Empty;
            SyncParamModel model = sqlite.SecSync(path, ref zipfile, requestParam);

            if (string.IsNullOrEmpty(zipfile))
            {
                mm.ResultCode = 1;
                mm.ResultMsg = "数据压缩失败。";
            }
            else
            {
                mm.ResultCode = 0;
                model.FilePath = string.Format("DBData/{0}", zipfile);
                mm.Result = model;
            }

            return mm;
        }
    }
}
