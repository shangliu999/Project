using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Customer.Models
{
    public class MaintainParamModel
    {
        public string sEcho { get; set; }
        public int iDisplayLength { get; set; }
        public int iDisplayStart { get; set; }
        public int InvType { get; set; }
        public string Hotel { get; set; }
        public string Floor { get; set; }
        public DateTime Time { get; set; }
        public string InvNo { get; set;} 
        public int HotelId { get; set; }
    }
}