﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Customer.Models
{
    public class ReceivingDetailsModel
    {
        public string TextileName { get; set; }

        public string SizeName { get; set; }

        public int AreaID { get; set; }

        public int SizeID { get; set; }

        public string Count { get; set; }
    }

    public class ReceivingModel
    {
        public string ID { get; set; }

        public int HotelID { get; set; }

        public int RegionID { get; set; }

        public int Quantity { get; set; }

        public string Remrak { get; set; }

        public DateTime InvCreateTime { get; set; }

        public Nullable<short> InvSubType { get; set; }

        public List<ReceivingDetailsModel> Data { get; set; }
    }
}