using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using InMemoryIdenity.ForSingleExternalAuthOnly;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.Configuration;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using OIDCPlay.Models;

namespace OIDCPlay.Controllers
{
    public class NortonLoginRedirectOptionsStore
    {
        private IConfiguration _configuration;
        private List<OAuth2SchemeRecord> _oAuth2SchemeRecords;
        private List<OAuth2SchemeRecord> OAuth2SchemeRecords
        {
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
        public NortonLoginRedirectOptionsStore(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private List<OptionModel> _optionModels;
        public List<OptionModel> OptionModels => _optionModels ?? (_optionModels = BuildNortonOptionModels());

        public OptionModel GetOptionModelByKey(string key)
        {
            var query = from item in OptionModels
                where item.Key == key
                select item;
            return query.FirstOrDefault();
        }
        List<OptionModel> BuildNortonOptionModels()
        {
            var query = from item in OAuth2SchemeRecords
                where item.Scheme == "Norton"
                select item;
            var optionModels = new List<OptionModel>();
            var nortonRecord = query.FirstOrDefault();
         
            foreach (var acrValue in nortonRecord.AcrValues)
            {
                optionModels.Add(new OptionModel()
                {
                    OptionType = OptionType.OptionType_ACR_VALUE,
                    Name = acrValue,
                    Checked = false,
                    HasArgument = acrValue.Contains("{arg}")
                });
            }
            optionModels.Add(new OptionModel()
            {
                OptionType = OptionType.OptionType_LOGIN_PROMPT,
                Name = "Login Prompt",
                Checked = false,
                HasArgument = false
            });
            foreach (var optionModel in optionModels)
            {
                optionModel.Key = optionModel.Name.GetHashCode().ToString();
            }
            return optionModels;
        }
    }
    [Authorize]
    public class AccountController : Controller
    {
        private CustomSignInManager _signInManager;
        private CustomUserManager _userManager;

        public NortonLoginRedirectOptionsStore NortonLoginRedirectOptionsStore { get; }
        public AccountController(NortonLoginRedirectOptionsStore nortonLoginRedirectOptionsStore)
        {
            NortonLoginRedirectOptionsStore = nortonLoginRedirectOptionsStore;
        }

        public AccountController(CustomUserManager userManager, CustomSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public CustomSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<CustomSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public CustomUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<CustomUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            //return new ChallengeResult("Google", Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));

            return View();
        }

        NortonViewModel BuildNortonViewModel()
        {
            var nortonViewModel = new NortonViewModel { OptionModels = NortonLoginRedirectOptionsStore.OptionModels };
            return nortonViewModel;
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult LoginNorton(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(BuildNortonViewModel());
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginNorton(NortonViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var query = from item in model.OptionModels
                where item.Checked == true
                select item;
            var trimmedList = query.ToList();
            if (trimmedList.Count > 0)
            {
                var cookieValue = JsonConvert.SerializeObject(trimmedList);
                Session.Add("oidc.norton.options", cookieValue);
                Response.Cookies.Add(new HttpCookie("oidc.norton.options", cookieValue)
                {
                    Expires = DateTime.Now.AddMinutes(30)
                });
            }
            else
            {
                Session.Remove("oidc.norton.options");
                Response.Cookies.Add(new HttpCookie("oidc.norton.options", "")
                {
                    Expires = DateTime.Now.AddMinutes(-30)
                });
            }
           
            return ExternalLogin("Norton", returnUrl);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }
 
        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new {ReturnUrl = returnUrl, RememberMe = false});
                case SignInStatus.Failure:
                default:

                    var autoCreateAccount = true;
                    if (autoCreateAccount)
                    {
                        var email = loginInfo.ExternalIdentity.FindFirstValue(ClaimTypes.Email);
                        var nameIdentifier = loginInfo.ExternalIdentity.FindFirstValue(ClaimTypes.NameIdentifier);
                        var name = loginInfo.ExternalIdentity.FindFirstValue(ClaimTypes.Name);
                        if (email == null)
                        {
                            email = string.Format("{0}@{1}", name, nameIdentifier);
                        }

                        // NOTE: This bypasses the initial onboarding of a user and ust uses their nameIdentifier as an email
                        var result2 = await ExternalLoginConfirmation(
                            new ExternalLoginConfirmationViewModel
                            {
                                Email = email
                            },
                            returnUrl);
                        return result2;
                    }
                    else
                    {
                        // If the user does not have an account, then prompt the user to create an account
                        ViewBag.ReturnUrl = returnUrl;
                        ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                        return View("ExternalLoginConfirmation",
                            new ExternalLoginConfirmationViewModel {Email = loginInfo.Email});
                    }
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new CustomUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

       

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}