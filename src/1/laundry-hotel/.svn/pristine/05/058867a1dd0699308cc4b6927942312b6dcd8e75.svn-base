using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class WeChatOrderDetailParamModel : ParamBaseModel
    {
        public string invno { get; set; }
    }

    public class WeChatParamModel : ParamBaseModel
    {
        public string openId { get; set; }

        public int hotelId { get; set; }

        public int userId { get; set; }
    }

    public class WeChatInvoiceParamModel : ParamBaseModel
    {
        public int hotelId { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        public int invType { get; set; }

        /// <summary>
        /// 1日表 2月表
        /// </summary>
        public int subType { get; set; }

        public string time { get; set; }
    }

    public class WeChatComplainParamModel : ParamBaseModel
    {
        public string openId { get; set; }

        public int hotelId { get; set; }

        public int userId { get; set; }

        public string cause { get; set; }

        public string remarks { get; set; }

        public string happenDate { get; set; }
    }

    public class WeChatShopOrderParamModel : ParamBaseModel
    {
        public string guid { get; set; }

        public int hotelId { get; set; }

        public int userId { get; set; }

        public string deliveryDate { get; set; }

        public string deliveryTime { get; set; }

        public string remarks { get; set; }

        public List<WcChatShopOrderDetailParamModel> Detail { get; set; }
    }

    public class WcChatShopOrderDetailParamModel
    {
        public int goodsId { get; set; }

        public int goodsCount { get; set; }
    }

    public class WeChatInvoiceSettleParamModel : ParamBaseModel
    {
        public string InvNo { get; set; }
    }

    public class WeChatHotelDetailParamModel : ParamBaseModel
    {
        public int hotelId { get; set; }

        public string slogan { get; set; }

        public string logo { get; set; }

        public string hotelName { get; set; }

        public string hotelPoint { get; set; }

        public string hotelProfile { get; set; }

        public string hotelTel { get; set; }

        public string hotelImg { get; set; }

        public string[] delImg { get; set; }
    }
}
