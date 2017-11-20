using ETexsys.APIRequestModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.WashingCabinet.Model
{
    public class LoginModel: ParamBaseModel
    {
        public string LoginPwd;
        public string LoginName;
    }
}
