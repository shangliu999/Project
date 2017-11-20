﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    public class ResponseInvoiceModel
    {
        public string ID { get; set; }

        public short? InvSubType { get; set; }

        public int HotelID { get; set; }

        public string HotelName { get; set; }

        public string RegionName { get; set; }

        public string InvNo { get; set; }

        public string InvState { get; set; }

        public string CreateTime { get; set; }

        public string CreateUserName { get; set; }

        public string Bag { get; set; }

        public string Remark { get; set; }

        public int Quantity { get; set; }

        public List<ResponseInvoiceDetailModel> Data { get; set; }

        public List<ResponseReapetOpModel> RepeatOperators { get; set; }
    }

    public class ResponseInvoiceDetailModel
    {
        public string ClassName { get; set; }

        public string SizeName { get; set; }

        public int TextileCount { get; set; }

        public int Normal { get; set; }

        public int Dirty { get; set; }

        public int BackWash { get; set; }

        public int GuoShui { get; set; }

        public int SendCount { get; set; }
    }

    public class ResponseReapetOpModel
    {
        public string ShortName { get; set; }

        public string RegionName { get; set; }

        public string ClassName { get; set; }

        public string SizeName { get; set; }

        public DateTime RepeatInvCreateTime { get; set; }
    }
}
