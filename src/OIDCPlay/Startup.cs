using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OIDCPlay.Startup))]
namespace OIDCPlay
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
