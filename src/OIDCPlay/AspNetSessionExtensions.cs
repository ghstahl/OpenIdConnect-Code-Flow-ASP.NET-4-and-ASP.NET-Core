using System.Web;
using System.Web.SessionState;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace OIDCPlay
{
    public static class AspNetSessionExtensions
    {
        public static IAppBuilder RequireAspNetSession(this IAppBuilder app)
        {
            app.Use((context, next) =>
            {
                // Depending on the handler the request gets mapped to, session might not be enabled. Force it on.
                HttpContextBase httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                httpContext.SetSessionStateBehavior(SessionStateBehavior.Required);
                return next();
            });
            // SetSessionStateBehavior must be called before AcquireState
            app.UseStageMarker(PipelineStage.MapHandler);
            return app;
        }

        public static IAppBuilder UseAspNetAuthSession(this IAppBuilder app)
        {
            return app.UseAspNetAuthSession(new CookieAuthenticationOptions());
        }

        public static IAppBuilder UseAspNetAuthSession(this IAppBuilder app, CookieAuthenticationOptions options)
        {
            app.RequireAspNetSession();
            options.SessionStore = new AspNetAuthSessionStore();
            app.UseCookieAuthentication(options, PipelineStage.PreHandlerExecute);
            return app;
        }
    }
}