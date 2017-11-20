﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class BillInquiryParamModel: ParamBaseModel
    {
        [Required(ErrorMessage = "酒店不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = "酒店数据格式不合法")]
        public int HotelId { get; set; }
        
        public int RegionID { get; set; }

        [Required(ErrorMessage = "时间不能为空")]
        public DateTime CreateTime { get; set; }
        
        [Required(ErrorMessage = "单据类型不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "单据数据格式不合法")]
        public int InvType { get; set; }
    }
    public class DetailsParamModel : ParamBaseModel
    {
        [Required(ErrorMessage ="单据编号不能为空")]
        public string OrderNumber { get; set; }
    }
}
