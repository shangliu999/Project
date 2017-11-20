using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Microsoft.Practices.Unity.InterceptionExtension;
using System.Configuration;
using ETexsys.IDAL;
using ETexsys.Model;

namespace ETexsys.Cloud.API
{
    public class Global : HttpApplication
    {
        public static int InvoiceNubmer { get; set; }

        public static DateTime InvoiceTime { get; set; }

        public static int OrderNubmer { get; set; }

        public static DateTime OrderTime { get; set; }

        public static Dictionary<string, string> Access_token { get; set; }

        void Application_Start(object sender, EventArgs e)
        {
            // 在应用程序启动时运行的代码
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Access_token = new Dictionary<string, string>();

            Initialise();
        }

        void Initialise()
        {
            var uc = BuildUnityContainer();
            DependencyResolver.SetResolver(new Common.UnityDependencyResolver(uc));
            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(uc);
        }

        IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();
            container.AddNewExtension<Interception>();

            UnityConfigurationSection configuration = ConfigurationManager.GetSection(UnityConfigurationSection.SectionName) as UnityConfigurationSection;
            configuration.Configure(container, "defaultContainer");

            container.Configure<Interception>().SetInterceptorFor<IRepository<object>>(new InterfaceInterceptor());

            IRepository<invoice> i_invoice = container.Resolve<IRepository<invoice>>();
            invoice model = i_invoice.Entities.OrderByDescending(v => v.CreateTime).FirstOrDefault();
            if (model != null)
            {
                InvoiceTime = model.CreateTime;
                InvoiceNubmer = Convert.ToInt32(model.InvNo.Substring(model.InvNo.Length - 6));
            }

            IRepository<goodsorder> i_goodsorder = container.Resolve<IRepository<goodsorder>>();
            goodsorder orderModel = i_goodsorder.Entities.OrderByDescending(v => v.CreateTime).FirstOrDefault();
            if (orderModel != null)
            {
                OrderTime = orderModel.CreateTime;
                OrderNubmer = Convert.ToInt32(orderModel.OrderNo.Substring(orderModel.OrderNo.Length - 6));
            }
            return container;
        }
    }
}