using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    public class ResponseWeChatIndexModel
    {
        public List<ResponseGoodsModel> TextileDistribution { get; set; }
    }

    public class ResponseGoodsModel
    {
        public string name { get; set; }

        public string rentdata { get; set; }

        public string usingdata { get; set; }

        public string abdata { get; set; }
    }

    public class ResponseIntegralRecords
    {
        public string date { get; set; }

        public string time { get; set; }

        public string data { get; set; }

        public int Type { get; set; }

        public string Remark { get; set; }

        public string invNo { get; set; }
    }

    public class ResponsePriceModel
    {
        public int ClassID { get; set; }

        public int SizeID { get; set; }

        public string ClassName { get; set; }

        public string RentData { get; set; }

        public string WashingData { get; set; }
    }

    public class ResponseDaySummaryModel
    {
        public string ClassName { get; set; }

        public string SizeName { get; set; }

        public int Count { get; set; }

        public string OtherCount { get; set; }
    }

    public class ResponseMonthSummaryModel
    {
        public string date { get; set; }

        public int Count { get; set; }

        public int OtherCount { get; set; }
    }

    public class ResponseShopGoodsCateModel
    {
        public string id { get; set; }

        public string tag { get; set; }

        public string name { get; set; }

        public List<ResponseShopGoodsModel> dishs;
    }

    public class ResponseShopGoodsModel
    {
        public string id { get; set; }

        public decimal price { get; set; }

        public decimal costprice { get; set; }

        public string name { get; set; }

        public int sales { get; set; }

        public string pic { get; set; }

        public int count { get; set; }
    }

    public class ResponseOrderModel
    {
        public string id { get; set; }

        public string invno { get; set; }

        /// <summary>
        /// 下单时间
        /// </summary>
        public string time { get; set; }

        public string stateName { get; set; }

        public int state { get; set; }

        public int total { get; set; }

        public decimal money { get; set; }

        /// <summary>
        /// 接单时间
        /// </summary>
        public string comfirmTime { get; set; }

        /// <summary>
        /// 配货时间
        /// </summary>
        public string distributionTime { get; set; }

        /// <summary>
        /// 签收时间
        /// </summary>
        public string signTime { get; set; }

        /// <summary>
        /// 预计送达时间
        /// </summary>
        public string deliveryTime { get; set; }

        public string Distributor { get; set; }

        public string DistributorTel { get; set; }

        public List<ResponseOrderDetailModel> detail { get; set; }
    }

    public class ResponseOrderDetailModel
    {
        public string goodsname { get; set; }

        public int count { get; set; }

        public decimal price { get; set; }
    }

    public enum OrderState
    {
        订单待处理 = 1,
        订单已确认 = 2,
        订单已配货 = 3,
        订单已完成 = 4
    }

    public class ResponseInvoiceSettleModel
    {
        public string InvNo { get; set; }

        public string CreateTime { get; set; }

        public string CreateUserName { get; set; }

        public string ConfirmTime { get; set; }

        public string ConfirmUserName { get; set; }

        public string RentTotalMoney { get; set; }

        public string WashingTotalMoney { get; set; }

        public double TotalMoney { get; set; }

        public int TotalTextile { get; set; }

        public List<ResponseSettleDetailModel> Detail { get; set; }
    }

    public class ResponseSettleDetailModel
    {
        public string ClassName { get; set; }

        public string SizeName { get; set; }

        public int TextileCount { get; set; }

        public string RentMoney { get; set; }

        public string WashingMoney { get; set; }
    }
}
