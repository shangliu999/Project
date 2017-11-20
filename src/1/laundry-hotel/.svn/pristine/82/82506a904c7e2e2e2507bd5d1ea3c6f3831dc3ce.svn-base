using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.APIRequestModel.Response
{
    public class ResponseSysUserModel
    {
        public int UserID { get; set; }

        public string UserName { get; set; }

        public string UName { get; set; }

        public List<ResponseSysRightModel> SysRights { get; set; }

        public List<int> RightHotel { get; set; }
        
        public ResponseSysCustomerModel SysCustomer { get; set; }
    }

    public class ResponseSysRightModel
    {
        public int RightID { get; set; }
        public string RightCode { get; set; }
        public Nullable<int> RightParentID { get; set; }
        public string RightName { get; set; }
        public Nullable<int> RightSort { get; set; }
        public string RightUrl { get; set; }
        public string RightIcon { get; set; }
        public Nullable<int> RightType { get; set; }
        public Nullable<bool> ShowInMainMenu { get; set; }
        public int ApplicationID { get; set; }
    }

    public class ResponseSysCustomerModel
    {
        public int ID { get; set; }
        public string SysCusName { get; set; }
        public string Code { get; set; }
    }
}
