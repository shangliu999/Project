using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Report.Models
{
    public class ComplaintModel 
    {
        public string sEcho { get; set; }
        public int iDisplayLength { get; set; }
        public int iDisplayStart { get; set; }
        public string HotelName { get; set; }
        public string State { get; set; }
        public int Count { get; set; }
    }
}