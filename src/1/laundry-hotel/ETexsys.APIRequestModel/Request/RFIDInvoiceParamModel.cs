using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class RFIDInvoiceParamModel : ParamBaseModel
    {
        /// <summary>
        /// 防重唯一编码
        /// </summary>
        [Required(ErrorMessage = "不能为空")]
        public string GUID { get; set; }

        /// <summary>
        /// 酒店ID
        /// </summary>
        [Required(ErrorMessage = "酒店不能为空")]
        public int HotelID { get; set; }

        /// <summary>
        /// 楼层ID
        /// </summary>
        [Required(ErrorMessage = "楼层不能为空")]
        public int RegionID { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        [Required(ErrorMessage = "类型不能为空")]
        [Range(1, 20, ErrorMessage = "类型数据格式不合法")]
        public short InvType { get; set; }

        /// <summary>
        /// 单据子类型
        /// 出入库单据中，该字段同时作为出库依据的字段，所有从20开始作为出入库的类型。1租赁仓库出库 20净物出库 21污物出库 22净物入库 23污物入库 24洗涤入库
        /// 入厂单据中，0：租赁仓库入厂 1：洗涤工厂入厂
        /// </summary>
        public short InvSubType { get; set; }

        /// <summary>
        /// 合计
        /// </summary>
        [Required(ErrorMessage = " 不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = " 数据格式不合法")]
        public int Quantity { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remrak { get; set; }

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

        public List<AttchModel> Attach { get; set; }

        /// <summary>
        /// 所选任务类型 1：污物送洗 2：订单
        /// </summary>
        public int TaskType { get; set; }

        public DateTime ConfirmTime { get; set; }

        public int ConfirmUserID { get; set; }

        public string ConfirmUserName { get; set; }

        /// <summary>
        /// 手工发货单，收货时间
        /// </summary>
        public string ReciveDate { get; set; }

        #region android端

        /// <summary>
        /// 净物签收的依据。0:按实际扫描  1:按净物配送
        /// </summary>
        public int CreateMode { get; set; }

        /// <summary>
        /// 手工收发货明细
        /// </summary>
        public string[] Details { get; set; }

        /// <summary>
        /// 包，WEB API无法解析复杂类型Attach，只能换成List<string>
        /// </summary>
        public List<string> Bag { get; set; }

        /// <summary>
        /// 配货单ID，WEB API无法解析复杂类型Attach，只能换成List<string>
        /// </summary>
        public List<string> TaskInv { get; set; }

        /// <summary>
        /// 签名，WEB API无法解析复杂类型Attach，只能换成List<string>
        /// </summary>
        public List<string> Sign { get; set; }

        #endregion
    }

    public class AttchModel
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }
}
