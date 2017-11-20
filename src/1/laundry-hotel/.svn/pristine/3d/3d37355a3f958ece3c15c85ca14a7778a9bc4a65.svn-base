using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Models
{
    public class MsgModel
    {
        /// <summary>
        /// 请求状态 0成功 1失败 
        /// </summary>
        public int ResultCode { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string ResultMsg { get; set; }

        /// <summary>
        /// 辅助信息
        /// </summary>
        public string OtherCode { get; set; }

        /// <summary>
        /// 请求返回结果
        /// </summary>
        public object Result { get; set; }

        public object OtherResult { get; set; }
    }

    /// <summary>
    /// 分页基类
    /// </summary>
    public class ParamModel
    {
        /// <summary>
        /// DataTable请求服务器端次数
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// 每页显示的数量
        /// </summary> 
        public int iDisplayLength { get; set; }

        /// <summary>
        /// 分页时每页跨度数量
        /// </summary>
        public int iDisplayStart { get; set; }

        public string qCondition1 { get; set; }

        public string qCondition2 { get; set; }

        public string qCondition3 { get; set; }

        public string qCondition4 { get; set; }

        public string qCondition5 { get; set; }

        public string qCondition6 { get; set; }
    }
}