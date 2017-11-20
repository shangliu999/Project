using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeChatAPI.Models
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

        /// <summary>
        /// 辅助结果
        /// </summary>
        public object OtherResult { get; set; }
    }
}