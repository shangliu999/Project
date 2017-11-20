using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    public class ResponseBillInquiry
    {
        public string InvNo { get; set; }

        public string CreateUserName { get; set; }

        public string HotelName { get; set; }

        public int HotelID { get; set; }

        public int? RegionID { get; set; }

        public string RegionName { get; set; }

        public int Quantity { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
