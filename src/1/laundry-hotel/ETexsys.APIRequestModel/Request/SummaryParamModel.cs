using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
     public class SummaryParamModel: ParamBaseModel
    {
        [Required(ErrorMessage = "单据类型不能为空")]
        public DateTime CreateTime { get; set; }
        
        [Required(ErrorMessage ="单据类型不能为空")]
        public int InvType { get; set; }

        public int HotelID { get; set; }

        public int RegionID { get; set; }
    }
}
