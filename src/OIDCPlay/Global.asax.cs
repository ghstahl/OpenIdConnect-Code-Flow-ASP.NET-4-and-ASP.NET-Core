using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;


namespace OIDCPlay
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //this.PostAuthenticateRequest += Application_PostAuthenticateRequest;
            //this.AcquireRequestState += Application_AcquireRequestState;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        public void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        }
        public void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var current = HttpContext.Current;

        }
    }
}
