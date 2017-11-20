using ETexsys.Common.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Security;

namespace ETexsys.Cloud.API.Common
{
    public class SupportFilter : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            string method = actionContext.Request.Method.Method;

            IEnumerable<string> requestToken = new List<string>();
            actionContext.Request.Headers.TryGetValues("access_token", out requestToken);

            string token = string.Empty, signature = string.Empty;

            if (requestToken != null)
            {
                token = requestToken.FirstOrDefault();
            }

            new Log4NetFile().Log("Token:" + token);

            string data = string.Empty;
            if (!string.IsNullOrEmpty(token))
            {
                FormsAuthenticationTicket authTicket = null;
                try
                {
                    authTicket = FormsAuthentication.Decrypt(token);

                    if (Global.Access_token.ContainsValue(token))
                    {
                        new Log4NetFile().Log(authTicket.Expiration.ToString());
                        if (authTicket.Expiration > DateTime.Now)
                        {
                            base.IsAuthorized(actionContext);
                        }
                    }

                }
                catch
                {

                }
                base.OnAuthorization(actionContext);
            }
            else
            {
                var attributes = actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().OfType<AllowAnonymousAttribute>();
                bool isAnonymous = attributes.Any(a => a is AllowAnonymousAttribute);
                if (isAnonymous) base.OnAuthorization(actionContext);
                else HandleUnauthorizedRequest(actionContext);
            }
        }
    }
}