using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Customer.Models
{
    public class RentWashiUpdataModel
    {
        public string TemplateName { get; set; }
        public int DataSources { get; set; }
        public int SettlementMode { get; set; }
        public int SubType { get; set; }
        public string ClassName { get; set; }
        public string SizeName { get; set; }
        public decimal Price { get; set; }
        public decimal ClassId { get; set; }
        public decimal SizeId { get; set; }
    }
}