using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class QRCodeBindingParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "不能为空")] 
        public string QRCode { get; set; }

        [Required(ErrorMessage = "不能为空")]
        [MaxLength(24)]
        public string RFIDTagNo { get; set; }
    }
}
