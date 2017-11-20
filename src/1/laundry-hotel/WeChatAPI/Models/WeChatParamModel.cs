using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeChatAPI.Models
{

    public class WeChatJSCodeParamModel
    {
        public string appid { get; set; }

        public string secret { get; set; }

        public string js_code { get; set; }
    }

    public class WeChatTextileTrackParamModel
    {
        public string QRCode { get; set; }

        public string openid { get; set; }
    }

    public class WeChatTextileParamModel
    {
        /// <summary>
        /// 纺织品id
        /// </summary>
        public int id { get; set; }

        public string code { get; set; }
    }

    public class WeChatHotelParamModel
    {
        
        public int hotelid { get; set; }

        public string code { get; set; }
    }
}