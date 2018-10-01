using System;
using System.Collections.Generic;
 
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security.DataProtection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OIDCPlay.Models;

namespace OIDCPlay.Controllers
{
    public class HomeController : Controller
    {
        private ISomething _something;
        public HomeController(ISomething something)
        {
            _something = something;
        }
        private string DecodeJwt(string jwtToken)
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
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            var oidc = Session["oidc"] as Dictionary<string, string>;
            return View(new AboutModel()
            {
                OIDC = oidc
            });
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}