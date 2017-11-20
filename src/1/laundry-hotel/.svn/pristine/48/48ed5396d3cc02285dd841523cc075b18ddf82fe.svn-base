using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class RegisterParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "品牌不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "品牌数据格式不合法")]
        public int BrandID { get; set; }

        [Required(ErrorMessage = "品名不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "品名数据格式不合法")]
        public int ClassID { get; set; }

        [Required(ErrorMessage = "尺寸不能为空")]
        public int SizeID { get; set; }

        [Required(ErrorMessage = "面料不能为空")]
        public int FabricID { get; set; }

        [Required(ErrorMessage = "颜色不能为空")]
        public int ColorID { get; set; }

        [Required(ErrorMessage = "纺织品品牌不能为空")]
        public int TextileBrandID { get; set; }

        [Required(ErrorMessage = "仓库不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "仓库数据格式不合法")]
        public int StoreID { get; set; }

        [Required(ErrorMessage = "芯片码不能为空")]
        [MinLength(1)]
        public string[] Tags { get; set; }
    }

    public class AssetsRegParamModel : ParamBaseModel
    {
        [Required(ErrorMessage = "类型不能为空")]
        public int AssetsType { get; set; }

        [Required(ErrorMessage = "操作人不能为空")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "编码不能为空")]
        public string Code { get; set; }

        [Required(ErrorMessage = "芯片码不能为空")]
        [MinLength(1)]
        public string[] Tags { get; set; }
    }
}
