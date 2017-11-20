using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETexsys.WebApplication.Common
{
    /// <summary>
    /// 表示需要用户点击URL才可以使用的特性 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AddressUrlAttribute : FilterAttribute, IAuthorizationFilter
    {
        public AddressUrlAttribute() { }

        /// <summary>
        /// 处理是否异步请求
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if(filterContext.HttpContext == null)
            {
                throw new Exception("此特性只适合于Web应用程序使用！");
            }
            else
            {
                bool isAjax = filterContext.RequestContext.HttpContext.Request.IsAjaxRequest();
                if (!isAjax && !filterContext.RequestContext.HttpContext.Request.Url.AbsolutePath.Contains("Export"))
                {
                    filterContext.Result = new RedirectResult("~/Home/Index");
                }
            }
        }
    }
}