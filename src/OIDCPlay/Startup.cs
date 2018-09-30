using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
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
            var builder = new ContainerBuilder();
            ConfigureAuth(app);

            builder.Register(c => HttpContext.Current)
                .As(typeof(HttpContext)).InstancePerRequest();

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
        }

    }
}
