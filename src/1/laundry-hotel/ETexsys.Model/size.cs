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
    
    public partial class size
    {
        public int ID { get; set; }
        public string SizeName { get; set; }
        public bool IsDelete { get; set; }
        public int Sort { get; set; }
        public int CreateUserID { get; set; }
        public System.DateTime CreateTime { get; set; }
        public Nullable<int> UpdateUserID { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
    }
}
