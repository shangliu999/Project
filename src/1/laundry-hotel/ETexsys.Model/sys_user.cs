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
    
    public partial class sys_user
    {
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string LoginPwd { get; set; }
        public Nullable<int> OrganiseID { get; set; }
        public Nullable<int> PersonnelPlaceID { get; set; }
        public string UName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Nullable<int> CreateUserID { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public Nullable<bool> IsAdmin { get; set; }
        public Nullable<bool> IsLevel { get; set; }
        public Nullable<System.DateTime> LastLoginTime { get; set; }
        public Nullable<short> UserType { get; set; }
        public string WXOpenID { get; set; }
        public string XCXOpenID { get; set; }
        public string SMSCode { get; set; }
        public Nullable<System.DateTime> SMSTime { get; set; }
        public Nullable<int> StoreID { get; set; }
        public Nullable<short> StoreType { get; set; }
    }
}
