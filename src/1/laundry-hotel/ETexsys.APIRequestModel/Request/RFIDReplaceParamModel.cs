using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class RFIDRelaceQRCodeParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "不能为空")]
        public string QRCode { get; set; }

        public DateTime RequestTime { get; set; }
    }
    public class RFIDReplaceParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "数据格式不合法")]
        public int TextileID { get; set; }

        [Required(ErrorMessage = "不能为空")]
        [MaxLength(24)]
        public string OldTagNo { get; set; }

        [Required(ErrorMessage = "不能为空")]
        [MaxLength(24)]
        public string NewTagNo { get; set; }

        public int CreateUserId { get; set; }
    }
}
