using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Report.Models
{
    public class SelectParamModel
    {
        public string sEcho { get; set; }
        public int iDisplayLength { get; set; }
        public int iDisplayStart { get; set; }
        public int InvType { get; set; }
        public int Time { get; set; }
        public string Hotel { get; set; }
        public string Floor { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public int InquiryMode { get; set; }
        public int HotelId { get; set; }
    }
}