using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using InMemoryIdenity.ForSingleExternalAuthOnly;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using Owin;
using OIDCPlay.Models;

namespace OIDCPlay
{
    public partial class Startup
    {
 
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            /*
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<CustomUserManager>(CustomUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            */
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<CustomUserManager>(CustomUserManager.Create);
            app.CreatePerOwinContext<CustomSignInManager>(CustomSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity =
                        SecurityStampValidator.OnValidateIdentity<CustomUserManager, CustomUser>(
                            validateInterval: TimeSpan.FromMinutes(30),
                            regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Caption = "Google",
                AuthenticationType = "Google",
                ClientId = "1096301616546-edbl612881t7rkpljp3qa3juminskulo.apps.googleusercontent.com",
                ClientSecret = "gOKwmN181CgsnQQDWqTSZjFs",
                Authority = "https://accounts.google.com/",
//                ResponseType = OpenIdConnectResponseType.IdToken,// Works as well, just no access_tokens  
                ResponseType = OpenIdConnectResponseType.Code,
                Scope = "openid email",
                UseTokenLifetime = false,
                RedirectUri = "https://p7core.127.0.0.1.xip.io:44344/signin-google",
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    AuthorizationCodeRedeemed= async n =>
                    {
                        var ticket = n.AuthenticationTicket;
                        // store tokens for later use
                        var idToken = ticket.Properties.GetTokenValue("id_token");
                        var accessToken = ticket.Properties.GetTokenValue("access_token");
                        var refreshToken = ticket.Properties.GetTokenValue("refresh_token");

                        var httpContext = DependencyResolver.Current.GetService<HttpContext>();
                        Dictionary<string, string> oidc = new Dictionary<string, string>
                        {
                            {"id_token", idToken},
                            { "access_token", accessToken},
                            { "refresh_token", refreshToken}
                        };
                        httpContext.Session.Add("oidc", oidc);
                    }
                }
            });
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Caption = "Norton",
                AuthenticationType = "Norton",
                ClientId = "signin-norton/p7core44344/xip.io",
                ClientSecret = "{{put secret here}}",
                Authority = "https://login-int.norton.com/sso/oidc1/token",
                ResponseType = OpenIdConnectResponseType.Code,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateLifetime = false
                },
                Scope = "openid profile email open_web_session",
                UseTokenLifetime = false,
                RedirectUri = "https://p7core.127.0.0.1.xip.io:44344/signin-norton",
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    AuthorizationCodeRedeemed = async n =>
                    {
                        var ticket = n.AuthenticationTicket;
                        // store tokens for later use
                        var idToken = ticket.Properties.GetTokenValue("id_token");
                        var accessToken = ticket.Properties.GetTokenValue("access_token");
                        var refreshToken = ticket.Properties.GetTokenValue("refresh_token");

                        var httpContext = DependencyResolver.Current.GetService<HttpContext>();
                        Dictionary<string, string> oidc = new Dictionary<string, string>
                        {
                            {"id_token", idToken},
                            { "access_token", accessToken},
                            { "refresh_token", refreshToken}
                        };
                        httpContext.Session.Add("oidc", oidc);
                    },
                    RedirectToIdentityProvider = async n =>
                    {
                        // read cookie here to get hint of what acr value to send.
                        var cookies = n.Request.Cookies;
                       
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            n.ProtocolMessage.AcrValues = "https://login.norton.com/sso/saml_2.0_profile/nosignup";
                        }

                       
                    }
                }
            });
        }
    }
}
/*
 {
  "Google-ClientId": "1096301616546-edbl612881t7rkpljp3qa3juminskulo.apps.googleusercontent.com",
  "Google-ClientSecret": "gOKwmN181CgsnQQDWqTSZjFs",
 

    https://accounts.google.com/
    https://accounts.google.com/.well-known/openid-configuration

}
 */
