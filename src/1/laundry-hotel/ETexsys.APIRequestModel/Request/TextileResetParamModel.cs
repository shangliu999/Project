﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class TextileResetParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "芯片码不能为空")]
        [MinLength(1)]
        public string[] TagList { get; set; }
    }
}
