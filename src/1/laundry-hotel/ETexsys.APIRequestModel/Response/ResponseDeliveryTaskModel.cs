using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    /// <summary>
    /// 出库任务
    /// </summary>
    public class ResponseDeliveryTaskModel
    {
        public int HotelID { get; set; }

        public string HotelName { get; set; }

        public DateTime DeliveryDate { get; set; }

        public int DeliveryCount { get; set; }

        public string DeliveryNo { get; set; }

        public List<DeliveryTaskDetailModel> Detail { get; set; }
    }

    public class DeliveryTaskDetailModel
    {
        public int BrandID { get; set; }

        public string BrandName { get; set; }

        public int ClassID { get; set; }

        public string ClassName { get; set; }

        public int SizeID { get; set; }

        public string SizeName { get; set; }

        public int TaskCount { get; set; }
    }
}
