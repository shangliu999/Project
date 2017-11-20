using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace System.Web.Mvc
{
    public static class ExtendMvcHtml
    {

        public static MvcHtmlString ToolButton(this HtmlHelper html, List<sys_right_button> btnList)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in btnList)
            {
                sb.AppendFormat("<button class='{0} ' type='button' onclick='{1}()' style='margin-right:2px;'><i class='{2}'></i>&nbsp;{3}</button>", item.BtnClass, item.BtnScript, item.BtnIcon, item.BtnName);
            }

            return new MvcHtmlString(sb.ToString());
        }
    }
}