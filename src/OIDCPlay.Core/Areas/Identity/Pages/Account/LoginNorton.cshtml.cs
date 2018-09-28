using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OIDCPlay.Core.Areas.Identity.Pages.Account
{
    public enum OptionType
    {
        OptionType_ACR_VALUE,
        OptionType_LOGIN_PROMPT
    }
    public class OptionModel
    {
        public OptionType OptionType { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
        public bool HasArgument { get; set; }
        public string Argument { get; set; }
    }

    public class NortonViewModel
    {
        public List<OptionModel> OptionModels { get; set; }
    }
    [AllowAnonymous]
    public class LoginNortonModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        private IConfiguration _configuration;
        private List<OAuth2SchemeRecord> _oAuth2SchemeRecords;
        private List<OAuth2SchemeRecord> OAuth2SchemeRecords {
            get
            {
                if (_oAuth2SchemeRecords == null)
                {
                    var section = _configuration.GetSection("oauth2");
                    _oAuth2SchemeRecords = new List<OAuth2SchemeRecord>();
                    section.Bind(_oAuth2SchemeRecords);
                }
                return _oAuth2SchemeRecords;
            }
         }
        public LoginNortonModel(IConfiguration configuration,SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _logger = logger;
        }
        [BindProperty] public NortonViewModel NortonViewModel { get; set; }


        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }



        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

            var query = from item in OAuth2SchemeRecords
                where item.Scheme == "Norton"
                select item;
            var nortonRecord = query.FirstOrDefault();
            NortonViewModel = new NortonViewModel { OptionModels = new List<OptionModel>()};
            foreach (var acrValue in nortonRecord.AcrValues)
            {
                NortonViewModel.OptionModels.Add(new OptionModel()
                {
                    OptionType = OptionType.OptionType_ACR_VALUE,
                    Name = acrValue,
                    Checked = false,
                    HasArgument = acrValue.Contains("{arg}")
                });
            }
            NortonViewModel.OptionModels.Add(new OptionModel()
            {
                OptionType = OptionType.OptionType_LOGIN_PROMPT,
                Name = "Login Prompt",
                Checked = false,
                HasArgument = false
            });
        }

        public async Task<IActionResult> OnPostAsync(string localReturnUrl = null)
        {
            localReturnUrl = localReturnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var cookieValue = JsonConvert.SerializeObject(NortonViewModel.OptionModels);
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30)
                };
                Response.Cookies.Append("OptionModels", cookieValue, cookieOptions);

                return RedirectToPage("ExternalLogin", "Provider", new { provider = "Norton", returnUrl = localReturnUrl });
                
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                /*
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
                */
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
