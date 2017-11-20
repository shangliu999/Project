using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETexsys.WebApplication.Areas.Report.Models
{
    public class RSDailyModel
    {
        public string ShortName { get; set; }

        public string CreateTime { get; set; }

        public List<RSDailyItemModel> Data { get; set; }
    }

    public class RSDailyItemModel
    {
        public string Code { get; set; }

        public string TextileName { get; set; }

        public int Normal { get; set; }

        public int Dirty { get; set; }

        public int BackWash { get; set; }

        public int GuoShui { get; set; }

        public int SendCount { get; set; }
    }
}