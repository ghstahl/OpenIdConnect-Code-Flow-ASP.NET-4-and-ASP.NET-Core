using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.DataProtection;
using OIDCPlay.Controllers;
using OIDCPlay.Extensions;
using OIDCPlay.Models;
using Owin;


[assembly: OwinStartupAttribute(typeof(OIDCPlay.Startup))]

namespace OIDCPlay
{
    public interface ISomething
    {
        IDataProtector DataProtector();
    }

    public class Something : ISomething
    {
        IDataProtector _protector;

        public Something(IDataProtectionProvider provider)
        {
            _protector = provider.Create("OIDCPlay.io");
        }

        public IDataProtector DataProtector()
        {
            return _protector;
        }
    }

    public partial class Startup
    {
       

        public void Configuration(IAppBuilder app)
        {
          
            

            // SetSessionStateBehavior must be called before AcquireState
            app.UseStageMarker(PipelineStage.MapHandler);
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(HttpRuntime.AppDomainAppPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = configurationBuilder.Build();

            var builder = new ContainerBuilder();
            builder.RegisterOptions();

            var section = configuration.GetSection("oauth2");
            var oAuth2SchemeRecords = new List<OAuth2SchemeRecord>();
            section.Bind(oAuth2SchemeRecords);

            //add asp .net session
            app.RequireAspNetSession();
            ConfigureAuth(app);
            builder.Register(c => configuration).As<IConfiguration>();
            builder.Register(c => HttpContext.Current)
                .As(typeof(HttpContext)).InstancePerLifetimeScope();
            builder.RegisterType<NortonLoginRedirectOptionsStore>().InstancePerLifetimeScope();
            builder.RegisterType<MemoryCache>().As<IMemoryCache>().InstancePerLifetimeScope();
            builder.Register<IDataProtectionProvider>(c => app.GetDataProtectionProvider()).InstancePerRequest();
            builder.RegisterType<Something>().As<ISomething>().InstancePerLifetimeScope();
            // Register your MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Run other optional steps, like registering model binders,
            // web abstractions, etc., then set the dependency resolver
            // to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // OWIN MVC SETUP:

            // Register the Autofac middleware FIRST, then the Autofac MVC middleware.
            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();
          
            app.Use((context, next) =>
            {
                var session = HttpContext.Current.Session;
                // now use the session
                //   HttpContext.Current.Session["test"] = 1;
                return next();
            }).UseStageMarker(PipelineStage.PostAcquireState);
        }

    }
}
