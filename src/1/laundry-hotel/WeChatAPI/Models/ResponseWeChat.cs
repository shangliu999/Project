﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeChatAPI.Models
{
    public class ResponseWeChatModel
    {
        public int pass { get; set; }

        public int textileId { get; set; }

        public int hotelId { get; set; }

        public string code { get; set; }
    }

    public class ResponseWeChatSuYuanModel
    {

        public DateTime dDateTime { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public string time { get; set; }

        /// <summary>
        /// 环节
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 环节
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 操作人员
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string mark { get; set; }

    }

    public class ResponseWeChatTextileModel
    {
        public string textileName { get; set; }

        public string producer { get; set; }

        public string level { get; set; }

        public string fabricName { get; set; }

        public string colorName { get; set; }

        public string sizeName { get; set; }

        public string specifications { get; set; }

        public string costtime { get; set; }
        
        public string tagno { get; set; }
    }

    public class ResponseWeChatHotelModel
    {
        public string hotelLogo { get; set; }

        public string hotelName { get; set; }

        public string hotelSlogan { get; set; }

        public string hotelProfile { get; set; }

        public string[] hotelPoint { get; set; }

        public string[] hotelImg { get; set; }

        public string hotelTel { get; set; }
    }

    public class ResponseWeChatFactoryModel
    {
        public string factoryLogo { get; set; }

        public string factoryName { get; set; }

        public string factoryProfile { get; set; }

        public string factoryTel { get; set; }

        public string[] factoryImg { get; set; }

        public string weburl { get; set; }

        public string apiurl { get; set; }
    }
}