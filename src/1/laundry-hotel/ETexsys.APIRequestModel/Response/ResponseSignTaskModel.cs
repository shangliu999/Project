using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    public class ResponseSignTaskModel
    {
        public string ID { get; set; }

        public string InvNo { get; set; }

        public DateTime CreateTime { get; set; }

        public string Bags { get; set; }

        public int Total { get; set; }

        public List<ResponseSignTaskItemModel> Data { get; set; }
    }

    public class ResponseSignTaskItemModel
    {
        public string Name { get; set; }

        public string SizeName { get; set; }

        public int Count { get; set; }
    }
}
