using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Model
{
   public class sys_user
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
    }
}
