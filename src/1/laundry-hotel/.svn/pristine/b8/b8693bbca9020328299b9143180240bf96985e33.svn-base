using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class ScrapParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "报废责任方不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "数据格式不合法")]
        /// <summary>
        /// 报废责任方 1 工厂 2 酒店
        /// </summary>
        public int ResponsibleType { get; set; }

        [Required(ErrorMessage = "不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = "数据格式不合法")]
        public int HotelID { get; set; }

        public string HotelName { get; set; }

        [Required(ErrorMessage = "报废原因不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "数据格式不合法")]
        /// <summary>
        /// 报废原因ID
        /// </summary>
        public int ScrapID { get; set; }

        /// <summary>
        /// 报废原因
        /// </summary>
        public string ScrapName { get; set; }

        [Required(ErrorMessage = " 不能为空")]
        public int CreateUserID { get; set; }

        [Required(ErrorMessage = " 不能为空")]
        public string CreateUserName { get; set; }

        /// <summary>
        /// 芯片码
        /// </summary>
        [Required(ErrorMessage = "芯片码不能为空")]
        [MinLength(1)]
        public string[] Tags { get; set; }
    }
}
