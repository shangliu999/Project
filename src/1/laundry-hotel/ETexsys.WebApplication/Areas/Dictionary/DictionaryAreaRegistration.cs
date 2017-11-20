using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.Dictionary
{
    public class DictionaryAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Dictionary";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Dictionary_default",
                "Dictionary/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}