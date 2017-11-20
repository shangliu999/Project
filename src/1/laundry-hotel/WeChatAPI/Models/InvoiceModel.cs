using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeChatAPI.Models
{
    public class InvoiceModel
    {
        public string Code { get; set; }

        public string InvID { get; set; }

        public string InvNo { get; set; }

        public string HotelName { get; set; }

        public string RegionName { get; set; }

        public string CreateTime { get; set; }

        public string CreateUserName { get; set; }

        public int Total { get; set; }

        public int InvType { get; set; }

        public string InvTypeName { get; set; }

        public string SubTypeName { get; set; }

        public int Comfirmed { get; set; }

        public string ConfirmUserName { get; set; }

        public string ConfirmTime { get; set; }

        public List<InvoiceDetailModel> Detail { get; set; }
    }

    public class InvoiceDetailModel
    {
        public string ClassName { get; set; }

        public string SizeName { get; set; }

        public int TextileCount { get; set; }
    }

    public enum InvoiceType
    {
        污物送洗单 = 1,
        净物配送单 = 2,
        入库单 = 3,
        出库单 = 4,
        入厂单 = 5,
        订单 = 6,
        退回 = 7
    }
}