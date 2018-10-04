using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Autofac.Integration.Owin;
using InMemoryIdenity.ForSingleExternalAuthOnly;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OIDCPlay.Controllers;
using Owin;
using OIDCPlay.Models;

namespace OIDCPlay
{
    public partial class Startup
    {
        private static string DecodeJwt(string jwtToken)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            //Check if readable token (string is in a JWT format)
            var readableToken = jwtHandler.CanReadToken(jwtToken);
            if (readableToken != true)
            {
                return null;
            }
            else
            {
                var token = jwtHandler.ReadJwtToken(jwtToken);

                //Extract the headers of the JWT
                var headers = token.Header;
                var jwtHeader = "{";
                foreach (var h in headers)
                {
                    jwtHeader += '"' + h.Key + "\":\"" + h.Value + "\",";
                }

                jwtHeader += "}";
                var jwtIdToken = "Header:\r\n" + JToken.Parse(jwtHeader).ToString(Formatting.Indented);

                //Extract the payload of the JWT
                var claims = token.Claims;
                var jwtPayload = "{";
                foreach (Claim c in claims)
                {
                    jwtPayload += '"' + c.Type + "\":\"" + c.Value + "\",";
                }

                jwtPayload += "}";
                jwtIdToken += "\r\nPayload:\r\n" + JToken.Parse(jwtPayload).ToString(Formatting.Indented);


                return jwtIdToken;
            }

        }

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
                            {"access_token", accessToken},
                            {"refresh_token", refreshToken}
                        };
                        var decodedDictionary = new Dictionary<string, string>();
                        foreach (var item in oidc)
                        {
                            var decodedItem = DecodeJwt(item.Value);
                            if (!string.IsNullOrEmpty(decodedItem))
                            {
                                decodedDictionary.Add($"{item.Key}.decoded", decodedItem);
                            }
                        }

                        foreach (var item in decodedDictionary)
                        {
                            oidc.Add(item.Key, item.Value);
                        }

                        httpContext.Session.Add("oidc", oidc);
                    }
                }
            })
            .UseStageMarker(PipelineStage.PostAcquireState);

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
                {
                    Caption = "Norton",
                    AuthenticationType = "Norton",
                    ClientId = "signin-norton/p7core44344/xip.io",
                    ClientSecret = "herb_secret_@!",
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
                        AuthorizationCodeRedeemed = async context =>
                        {

                            var httpContext2 = DependencyResolver.Current.GetService<HttpContext>();
                            var nortonLoginRedirectOptionsStore = DependencyResolver.Current.GetService<NortonLoginRedirectOptionsStore>();
                          
                            HttpContextBase httpContext =
                                context.OwinContext.Get<HttpContextBase>(typeof(HttpContextBase).FullName);

                            var ticket = context.AuthenticationTicket;
                            // store tokens for later use
                            var idToken = ticket.Properties.GetTokenValue("id_token");
                            var accessToken = ticket.Properties.GetTokenValue("access_token");
                            var refreshToken = ticket.Properties.GetTokenValue("refresh_token");

                         
                            Dictionary<string, string> oidc = new Dictionary<string, string>
                            {
                                {"id_token", idToken},
                                {"access_token", accessToken},
                                {"refresh_token", refreshToken}
                            };
                            var decodedDictionary = new Dictionary<string, string>();
                            foreach (var item in oidc)
                            {
                                var decodedItem = DecodeJwt(item.Value);
                                if (!string.IsNullOrEmpty(decodedItem))
                                {
                                    decodedDictionary.Add($"{item.Key}.decoded", decodedItem);
                                }
                            }

                            foreach (var item in decodedDictionary)
                            {
                                oidc.Add(item.Key, item.Value);
                            }

                            httpContext.Session.Add("oidc", oidc);
                        },
                        RedirectToIdentityProvider = async context =>
                        {
                            if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                            {
                                // I can't seem to get the following to work on this event.
                                // 1. My sessions are always, null, not so in the AuthorizationCodeRedeemed event
                                // 2. I can't resolve a service, without an error from autofac about nested lifetime issues.
                                // So I am stuck with only being able to read cookies.
                                //var nortonLoginRedirectOptionsStore = DependencyResolver.Current.GetService<NortonLoginRedirectOptionsStore>();// this blows here

                                HttpContextBase httpContext =
                                    context.OwinContext.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                                // Session is null,  wtf mate!
                            //    var oidcNortonOptionsJson = httpContext.Session["oidc.norton.options"] as string;
                                var cookie = httpContext.Request.Cookies["oidc.norton.options"];
                                if (cookie != null)
                                {
                                    var oidcNortonOptionsJson = cookie.Value;
                                    // get the cookie, eat the cookie, delete the cookie.
                                    httpContext.Response.Cookies.Add(new HttpCookie("oidc.norton.options", "")
                                    {
                                        Expires = DateTime.Now.AddMinutes(-30)
                                    });

                                    var acrViewModels =
                                        JsonConvert.DeserializeObject<List<OptionModel>>(oidcNortonOptionsJson);

                                    var queryAcrValues = from item in acrViewModels
                                        where item.Checked && item.OptionType == OptionType.OptionType_ACR_VALUE
                                        select item;

                                    var acrValues = "";
                                    foreach (var v in queryAcrValues)
                                    {
                                        if (v.HasArgument)
                                        {
                                            acrValues += $"{v.Name.Replace("{arg}", v.Argument)} ";
                                        }
                                        else
                                        {
                                            acrValues += $"{v.Name} ";
                                        }

                                    }

                                    context.ProtocolMessage.AcrValues = acrValues.TrimEnd();

                                    var optionLoginPrompt = (from item in acrViewModels
                                        where item.Checked && item.OptionType == OptionType.OptionType_LOGIN_PROMPT
                                        select item).FirstOrDefault();
                                    if (optionLoginPrompt != null && optionLoginPrompt.Checked)
                                    {
                                        context.ProtocolMessage.Prompt = "login";
                                    }
                                }

                                if (httpContext.User.Identity.IsAuthenticated)
                                {
                                    // assuming a relogin trigger, so we will make the user relogin on the IDP
                                    context.ProtocolMessage.Prompt = "login";
                                }
                            }
                        }
                    }
                })
            .UseStageMarker(PipelineStage.PostAcquireState);
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
