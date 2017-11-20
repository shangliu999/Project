﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    public class ResponseRFIDTagModel
    {
        public int ID { get; set; }

        public int BrandID { get; set; }

        public string BrandName { get; set; }

        public int BrandSort { get; set; }

        public int ClassID { get; set; }

        public string ClassName { get; set; }

        public int ClassSort { get; set; }

        public int SizeID { get; set; }

        public string SizeName { get; set; }

        public int SizeSort { get; set; }

        public int TextileState { get; set; }

        public string TagNo { get; set; }

        public int Washtimes { get; set; }

        public DateTime CostTime { get; set; }

        public DateTime? UpdateTime { get; set; }

        public int? HotelID { get; set; }

        public string HotelName { get; set; }

        public int? RegionID { get; set; }

        public int? LastReceiveRegionID { get; set; }

        public int RFIDWashtime { get; set; }

        public DateTime RFIDCostTime { get; set; }

        public int ClassLeft { get; set; }

        public int PackCount { get; set; }

        public string TruckTagNo { get; set; }

        public string VirtualTagNo { get; set; }

        public string LastReceiveInvID { get; set; }
    }

    public class ResponseBagRFIDTagModel
    {
        public int BagID { get; set; }

        public string BagRFIDTagNo { get; set; }

        public string BagNo { get; set; }
    }

    public class ResponseTruckRFIDTagModel
    {
        public int TruckID { get; set; }

        public string TrunckRFIDTagNo { get; set; }
    }
}
