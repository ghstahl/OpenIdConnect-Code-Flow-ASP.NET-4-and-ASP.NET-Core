using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OIDCPlay.Core.Areas.Identity.Pages.Account;

namespace OIDCPlay.Core.Pages
{
    [Authorize]
    public class AboutModel : PageModel
    {
        IDataProtector _protector;
        public Dictionary<string, string> OIDC { get; set; }
        public AboutModel(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("OIDCPlay.Core.io");
        }
        public string Message { get; set; }

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
        public void OnGet()
        {
            Message = "Your application description page.";
            if (User.Identity.IsAuthenticated)
            {
                byte[] oidcStored = null;
                HttpContext.Session.TryGetValue("oidc", out oidcStored);

                if (oidcStored != null)
                {
                    string oidcJson = Encoding.ASCII.GetString(oidcStored);
                    OIDC = JsonConvert.DeserializeObject<Dictionary<string,string>>(oidcJson);
                    var decodedDictionary = new Dictionary<string,string>();
                    foreach (var item in OIDC)
                    {
                        var decodedItem = DecodeJwt(item.Value);
                        if (!string.IsNullOrEmpty(decodedItem))
                        {
                            decodedDictionary.Add($"{item.Key}.decoded", decodedItem);
                        }
                    }

                    foreach (var item in decodedDictionary)
                    {
                        OIDC.Add(item.Key,item.Value);
                    }
                }
            }
        }
    }
}
