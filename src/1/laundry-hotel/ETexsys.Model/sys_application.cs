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
    
    public partial class sys_application
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public sys_application()
        {
            this.sys_right = new HashSet<sys_right>();
        }
    
        public int ApplicationID { get; set; }
        public string ApplicationCode { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationDesc { get; set; }
        public Nullable<bool> ShowInMenu { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<sys_right> sys_right { get; set; }
    }
}
