using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Shop.Models
{
    public class OrderModel
    {
        public string OrderId { get; set; }

        public string OrderNo { get; set; }

        public string HotelName { get; set; }

        public string StateName { get; set; }

        public int State { get; set; }

        public string PaymentName { get; set; }

        public string Delivery { get; set; }

        public string CreateTime { get; set; }

        public string ComfirmTime { get; set; }

        public string DistributonTime { get; set; }

        public string SignTime { get; set; }

        public string Distributor { get; set; }

        public string DistributorTel { get; set; }

        public string GoodsTotal { get; set; }

        public string GoodsTotalMoney { get; set; }

        public string Remarks { get; set; }

        public List<OrderDetailModel> Detail { get; set; }
    }

    public class OrderDetailModel
    {
        public string GoodsName { get; set; }

        public string GoodsCount { get; set; }

        public string GoodsPrice { get; set; }
    }

    public enum OrderState
    {
        待处理 = 1,
        已确认 = 2,
        已配货 = 3,
        已完成 = 4
    }

    public enum Payment
    {
        未支付 = 1,
        线上支付 = 2,
        货到付款 = 3,
    }
}