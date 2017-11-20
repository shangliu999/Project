using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Customer.Models
{
    public class RentWashPriceModel
    {
        public int ID { get; set; }
        public string TemplateName { get; set; }
        public string DataSources { get; set; }
        public string SettlementMode { get; set; }
        public string ClassName { get; set; }
        public string SizeName { get; set; }
        public decimal Price { get; set; }
    }
}