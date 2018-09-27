using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
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

        public void OnGet()
        {
            Message = "Your application description page.";
            if (User.Identity.IsAuthenticated)
            {
                if (Request.Cookies.ContainsKey("oidc"))
                {
                    var protectedPayload = Request.Cookies["oidc"];
                    // unprotect the payload
                    string unprotectedPayload = _protector.Unprotect(protectedPayload);
                    OIDC = JsonConvert.DeserializeObject<Dictionary<string,string>>(unprotectedPayload);
                }
            }
        }
    }
}
