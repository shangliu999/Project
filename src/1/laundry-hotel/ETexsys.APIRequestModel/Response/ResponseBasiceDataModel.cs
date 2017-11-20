using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    /// <summary>
    /// 酒店
    /// </summary>
    public class ResponseHotelModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string HotelName { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public int BrandID { get; set; }

        /// <summary>
        /// 配货方式 1门店 2楼层
        /// </summary>
        public int RegionMode { get; set; }

        public int Sort { get; set; }

        /// <summary>
        /// 送达时间
        /// </summary>
        public string DeliveryTime { get; set; }
    }

    /// <summary>
    /// 楼层
    /// </summary>
    public class ResponseRegionModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 楼层名称
        /// </summary>
        public string RegionName { get; set; }

        /// <summary>
        /// 酒店ID
        /// </summary>
        public int HotelID { get; set; }

        /// <summary>
        /// 1污物送洗 2订单 3污物送洗+订单
        /// </summary>
        public int RegionMode { get; set; }

        public int Sort { get; set; }
    }

    /// <summary>
    /// 仓库
    /// </summary>
    public class ResponseStoreModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }


        /// <summary>
        /// 仓库名称
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 仓库类型 1租赁仓库 2周转仓库
        /// </summary>
        public int StoreType { get; set; }

        public int Sort { get; set; }
    }

    public class ResponseBrandTypeModel
    {
        public int ID { get; set; }

        public string BrandName { get; set; }

        public int Sort { get; set; }
    }

    /// <summary>
    /// 品名
    /// </summary>
    public class ResponseTextileClassModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 品名
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 预计寿命
        /// </summary>
        public int ClassLeft { get; set; }

        /// <summary>
        /// 打扎数
        /// </summary>
        public int PackCount { get; set; }

        /// <summary>
        /// 是否RFID
        /// </summary>
        public bool IsRFID { get; set; }

        public int Sort { get; set; }
    }

    public class ResponseClassSizeModel
    {
        public int ClassID { get; set; }

        public int SizeID { get; set; }

        public int Sort { get; set; }

        public string SizeName { get; set; }
    }

    public class ResponseSizeModel
    {
        public int ID { get; set; }

        public string SizeName { get; set; }

        public int Sort { get; set; }
    }

    /// <summary>
    /// 颜色
    /// </summary>
    public class ResponseColorModel
    {
        public int ID { get; set; }

        public string ColorName { get; set; }

        public int Sort { get; set; }
    }

    /// <summary>
    /// 面料
    /// </summary>
    public class ResponseFabricModel
    {
        public int ID { get; set; }

        public string FabricName { get; set; }
    }

    public class ResponseScrapModel
    {
        public int ID { get; set; }

        public string ScrapName { get; set; }
    }

    public class ResponseBagModel
    {
        public int ID { get; set; }

        public string BagNo { get; set; }
    }

    public class ResponseTextileBrandModel
    {
        public int ID { get; set; }

        public string TextileBrandName { get; set; }
    }
}
