using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    public class ResponseTextileFlowModel
    {
        /// <summary>
        /// 位置
        /// </summary>
        public string PositionName { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string OperationUser { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public string OperationTime { get; set; }
    }
}
