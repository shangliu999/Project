using ETexsys.DAL;
using ETexsys.IDAL;
using ETexsys.Model;
using ETexsys.WebApplication.Common;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace ETexsys.WebApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static int InvoiceNubmer { get; set; }

        public static DateTime InvoiceTime { get; set; }

        public static int BusInvoiceNubmer { get; set; }

        public static DateTime BusInvoiceTime { get; set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Initialise();
        }

        void Initialise()
        {
            var uc = BuildUnityContainer();
            DependencyResolver.SetResolver(new Common.UnityDependencyResolver(uc));
            //GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(uc);

        }

        IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            container.AddNewExtension<Interception>();
            //container.RegisterType<IRepository<sys_user>, RepositoryDA<sys_user>>().Configure<Interception>().SetInterceptorFor<IRepository<sys_user>>(new InterfaceInterceptor());

            UnityConfigurationSection configuration = ConfigurationManager.GetSection(UnityConfigurationSection.SectionName) as UnityConfigurationSection;
            configuration.Configure(container, "defaultContainer");

            container.Configure<Interception>().SetInterceptorFor<IRepository<object>>(new InterfaceInterceptor());

            IRepository<businessinvoice> i_businessinvoice = container.Resolve<IRepository<businessinvoice>>();
            businessinvoice model = i_businessinvoice.Entities.OrderByDescending(v => v.CreateTime).FirstOrDefault();
            if (model != null)
            {
                BusInvoiceTime = model.CreateTime;
                BusInvoiceNubmer = Convert.ToInt32(model.BNo.Substring(model.BNo.Length - 4));
            }
            
            IRepository<invoice> i_invoice = container.Resolve<IRepository<invoice>>();
            invoice invModel = i_invoice.Entities.OrderByDescending(v => v.CreateTime).FirstOrDefault();
            if (model != null)
            {
                InvoiceTime = model.CreateTime;
                InvoiceNubmer = Convert.ToInt32(invModel.InvNo.Substring(invModel.InvNo.Length - 6));
            }

            return container;
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null || authCookie.Value == "")
            {

                return;
            }
            FormsAuthenticationTicket authTicket = null;
            try
            {
                authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return;
            }
            string[] roles = authTicket.UserData.Split(new char[] { ';' });
            if (Context.User != null)
            {
                Context.User = new System.Security.Principal.GenericPrincipal(Context.User.Identity, roles);
            }
        }

    }
}
