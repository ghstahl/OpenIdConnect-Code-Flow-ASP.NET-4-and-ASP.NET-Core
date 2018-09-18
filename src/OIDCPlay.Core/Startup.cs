using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OIDCPlay.Core.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace OIDCPlay.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var section = Configuration.GetSection("oauth2");
            var oAuth2SchemeRecords = new List<OAuth2SchemeRecord>();
            section.Bind(oAuth2SchemeRecords);
            var authenticationBuilder = services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                }).AddCookie();

            if (!(string.IsNullOrEmpty(Configuration["Norton-ClientId"]) ||
                 string.IsNullOrEmpty(Configuration["Norton-ClientSecret"])))
            {
                authenticationBuilder.P7AddOpenIdConnect(NortonDefaults.AuthenticationScheme, NortonDefaults.DisplayName,
                    o =>
                    {
                        var openIdConnectOptions = new NortonOpenIdConnectOptions();
                        o.CallbackPath = Configuration["oauth2:norton:callbackPath"];

                        o.ClientId = Configuration["Norton-ClientId-Two"];
                        o.ClientSecret = Configuration["Norton-ClientSecret-Two"];

                        o.Authority = Configuration["oauth2:norton:authority"];
                        o.ResponseType = openIdConnectOptions.ResponseType;
                        o.GetClaimsFromUserInfoEndpoint = openIdConnectOptions.GetClaimsFromUserInfoEndpoint;
                        o.SaveTokens = openIdConnectOptions.SaveTokens;
                        o.Scope.Add("offline_access");
                        o.Events = new P7.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents()
                        {
                            OnMessageReceived = (context) =>
                            {
                                if (context.ProtocolMessage.Error != null)
                                {
                                    var errorUrl = context.HttpContext.Request.Cookies["x-errorUrl"];
                                    CookieOptions option = new CookieOptions { Expires = DateTime.Now };

                                    context.HttpContext.Response.Cookies.Append("x-errorUrl", "", option);
                                    if (string.IsNullOrEmpty(errorUrl))
                                    {
                                        errorUrl = "/account/ErrorJson";
                                    }

                                    context.Response.Redirect($"{errorUrl}?error={context.ProtocolMessage.Error}");

                                    context.HandleResponse();
                                }
                                return Task.FromResult(0);

                            },
                            OnAuthenticationFailed = (context) =>
                            {
                                return Task.FromResult(0);
                            },
                            OnRedirectToIdentityProvider = (context) =>
                            {

                                var acrValues = context.ProtocolMessage.AcrValues;
                                context.ProtocolMessage.Scope += " open_web_session";
                                if (context.HttpContext.User.Identity.IsAuthenticated)
                                {
                                    // assuming a relogin trigger, so we will make the user relogin on the IDP
                                    context.ProtocolMessage.Prompt = "login";
                                }
                                //  

                                var query = from item in context.Request.Query
                                            where string.Compare(item.Key, "prompt", true) == 0
                                            select item.Value;
                                if (query.Any())
                                {
                                    var prompt = query.FirstOrDefault();
                                    context.ProtocolMessage.Prompt = prompt;
                                }

                                query = from item in context.Request.Query
                                        where string.Compare(item.Key, "errorUrl", true) == 0
                                        select item.Value;
                                if (query.Any())
                                {
                                    var errorUrl = query.FirstOrDefault();
                                    context.Response.Cookies.Append("x-errorUrl", errorUrl);
                                }


                                return Task.FromResult(0);
                            },
                            OnTicketReceived = (context) =>
                            {

                                ClaimsIdentity identity = (ClaimsIdentity)context.Principal.Identity;
                                var givenName = identity.FindFirst(ClaimTypes.GivenName);
                                var familyName = identity.FindFirst(ClaimTypes.Surname);
                                var nameIdentifier = identity.FindFirst(ClaimTypes.NameIdentifier);
                                var userId = identity.FindFirst("UserId");


                                var claimsToKeep = new List<Claim> { givenName, familyName, nameIdentifier, userId };
                                claimsToKeep.Add(new Claim("DisplayName", $"{givenName.Value} {familyName.Value}"));
                                var newIdentity = new ClaimsIdentity(claimsToKeep, identity.AuthenticationType);

                                context.Principal = new ClaimsPrincipal(newIdentity);
                                return Task.CompletedTask;
                            }
                        };


                    });
            }
            if (!(string.IsNullOrEmpty(Configuration["Google-ClientId"]) ||
                  string.IsNullOrEmpty(Configuration["Google-ClientSecret"])))
            {
                authenticationBuilder.P7AddOpenIdConnect(GoogleDefaults.AuthenticationScheme, GoogleDefaults.DisplayName,
                      o =>
                      {
                          var openIdConnectOptions = new GoogleOpenIdConnectOptions();
                          o.CallbackPath = Configuration["oauth2:google:callbackPath"];

                          o.ClientId = Configuration["Google-ClientId"];
                          o.ClientSecret = Configuration["Google-ClientSecret"];

                          o.Authority = Configuration["oauth2:google:authority"];
                          o.ResponseType = openIdConnectOptions.ResponseType;
                          o.GetClaimsFromUserInfoEndpoint = openIdConnectOptions.GetClaimsFromUserInfoEndpoint;
                          o.SaveTokens = openIdConnectOptions.SaveTokens;

                          o.Events = new P7.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents()
                          {
                              OnMessageReceived = (context) =>
                              {
                                  if (context.ProtocolMessage.Error != null)
                                  {
                                      var errorUrl = context.HttpContext.Request.Cookies["x-errorUrl"];
                                      CookieOptions option = new CookieOptions { Expires = DateTime.Now };

                                      context.HttpContext.Response.Cookies.Append("x-errorUrl", "", option);
                                      if (string.IsNullOrEmpty(errorUrl))
                                      {
                                          errorUrl = "/account/ErrorJson";
                                      }

                                      context.Response.Redirect($"{errorUrl}?error={context.ProtocolMessage.Error}");

                                      context.HandleResponse();
                                  }
                                  return Task.FromResult(0);

                              },
                              OnAuthenticationFailed = (context) =>
                              {
                                  return Task.FromResult(0);
                              },
                              OnRedirectToIdentityProvider = (context) =>
                              {
                                  if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                                  {
                                      context.ProtocolMessage.AcrValues = "v1=some-value";
                                  }
                                  var query = from item in context.Request.Query
                                              where string.Compare(item.Key, "prompt", true) == 0
                                              select item.Value;
                                  if (query.Any())
                                  {
                                      var prompt = query.FirstOrDefault();
                                      context.ProtocolMessage.Prompt = prompt;
                                  }

                                  query = from item in context.Request.Query
                                          where string.Compare(item.Key, "errorUrl", true) == 0
                                          select item.Value;
                                  if (query.Any())
                                  {
                                      var errorUrl = query.FirstOrDefault();
                                      context.Response.Cookies.Append("x-errorUrl", errorUrl);
                                  }


                                  return Task.FromResult(0);
                              },
                              OnTicketReceived = (context) =>
                              {

                                  ClaimsIdentity identity = (ClaimsIdentity)context.Principal.Identity;
                                  var query = from claim in context.Principal.Claims
                                              where claim.Type == ClaimTypes.Name || claim.Type == "name"
                                              select claim;
                                  var nameClaim = query.FirstOrDefault();
                                  var nameIdentifier = identity.FindFirst(ClaimTypes.NameIdentifier);


                                  var claimsToKeep =
                                      new List<Claim>
                                      {
                                        nameClaim,
                                        nameIdentifier,
                                        new Claim("DisplayName", nameClaim.Value),
                                        new Claim("UserId", nameIdentifier.Value)
                                      };

                                  var newIdentity = new ClaimsIdentity(claimsToKeep, identity.AuthenticationType);

                                  context.Principal = new ClaimsPrincipal(newIdentity);
                                  return Task.CompletedTask;

                              },
                              OnUserInformationReceived = (context) =>
                              {
                                  return Task.FromResult(0);

                              }

                          };

                      });
            }


            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
