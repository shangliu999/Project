using System.Web.Mvc;

namespace ETexsys.WebApplication.Areas.KANBAN
{
    public class KANBANAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "KANBAN";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "KANBAN_default",
                "KANBAN/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}