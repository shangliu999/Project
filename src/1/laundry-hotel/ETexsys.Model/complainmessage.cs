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
    
    public partial class complainmessage
    {
        public int ID { get; set; }
        public string ComplainID { get; set; }
        public int PublishUserID { get; set; }
        public short PublishType { get; set; }
        public string PublishMessage { get; set; }
        public System.DateTime PublishTime { get; set; }
    }
}
