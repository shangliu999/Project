using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Dashboard.Models
{
    public class DashboardBaseModel
    {
        /// <summary>
        /// 架子车数量
        /// </summary>
        public int TruckTotal { get; set; }

        /// <summary>
        /// 布草包数量
        /// </summary>
        public int BagTotal { get; set; }

        /// <summary>
        /// 纺织品数量
        /// </summary>
        public int TextileTotal { get; set; }

        /// <summary>
        /// 60天以上未使用纺织品
        /// </summary>
        public int AbnormalTextileTotal { get; set; }

        /// <summary>
        /// 服务酒店数量
        /// </summary>
        public int HotelTotal { get; set; }

        /// <summary>
        /// 本月新增酒店
        /// </summary>
        public int CurrentMonthHotelTotal { get; set; }

        /// <summary>
        /// 本月收入
        /// </summary>
        public decimal InCome { get; set; }

        /// <summary>
        /// 上月收入
        /// </summary>
        public decimal PreInCome { get; set; }
    }

    public class DashboardScrapModel
    {
        public List<string> ClassName { get; set; }

        public List<int> ScrapTotal { get; set; }

        public List<int> ABScrapTotal { get; set; }
    }

    public class DashboardTextileDistrbutionModel
    {
        public string CateName { get; set; }

        public List<string> LegendList { get; set; }

        public List<PieChatModel> TextileData { get; set; }
    }

    public class PieChatModel
    {
        public string name { get; set; }

        public string value { get; set; }
    }

    public class DashboardWashingModel
    {
        public List<string> LegendList { get; set; }

        public List<string> xAxis { get; set; }

        public List<ChatSeriesModel> Series { get; set; }
    }

    public class ChatSeriesModel
    {
        public string name { get; set; }

        public List<double> data { get; set; }
    }
}