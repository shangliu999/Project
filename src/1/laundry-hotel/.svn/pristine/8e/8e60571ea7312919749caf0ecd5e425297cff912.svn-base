using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class InvoiceQueryParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "创建人ID不能为空")]
        public int CreateUserID { get; set; }

        public int HotelID { get; set; }

        public int RegionID { get; set; }

        public string CreateTime { get; set; }

        [Required(ErrorMessage = "单据类型不能为空")]
        public int InvType { get; set; }

        [Required(ErrorMessage = "页码不能为空")]
        public int PageIndex { get; set; }
    
        [Required(ErrorMessage = "每页大小不能为空")]
        public int PageSize { get; set; }
    }
}
