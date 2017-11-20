using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class TextileFlowParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "ID不能为空")]
        public int TextileId { get; set; }

        [Required(ErrorMessage = "Type不能为空 1 收发 2所有")]
        public int Type { get; set; }
    }
}
