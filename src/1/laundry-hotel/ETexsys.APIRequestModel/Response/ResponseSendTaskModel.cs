using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    public class ResponseSendTaskModel
    {
        public int ClassID { get; set; }

        public string ClassName { get; set; }

        public int ClassSort { get; set; }

        public int SizeID { get; set; }

        public string SizeName { get; set; }

        public int SizeSort { get; set; }

        public int TaskCount { get; set; }

        public short TaskType { get; set; }

        public int CheckCount { get; set; }

        public string TaskTime { get; set; }

        public int HotelID { get; set; }

        public string HotelName { get; set; }

        /// <summary>
        /// 酒店配货至 1：酒店，2：楼层
        /// </summary>
        public int? RegionMode { get; set; }

        public int RegionID { get; set; }

        public string RegionName { get; set; }
    }
}
