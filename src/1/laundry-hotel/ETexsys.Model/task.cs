//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ETexsys.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class task
    {
        public int ID { get; set; }
        public int RegionID { get; set; }
        public string TaskTime { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int SizeID { get; set; }
        public string SizeName { get; set; }
        public int TaskCount { get; set; }
        public short TaskType { get; set; }
        public int CheckCount { get; set; }
    }
}
