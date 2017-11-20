using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Request
{
    public class SyncParamModel : ParamBaseModel
    {
        public long Version { get; set; }

        public DateTime CreateTime { get; set; }

        public string FilePath { get; set; }
    }
}
