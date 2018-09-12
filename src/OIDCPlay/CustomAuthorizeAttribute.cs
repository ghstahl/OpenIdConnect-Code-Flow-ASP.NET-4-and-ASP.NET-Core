using System.Web;
using System.Web.Mvc;

public class CustomAuthorizeAttribute : AuthorizeAttribute
{
    public CustomAuthorizeAttribute( )
    {
    }
 

    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        return httpContext.User.Identity.IsAuthenticated;
    }
    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {


        filterContext.Result = new RedirectResult($"/Account/Login?returnUrl={filterContext.HttpContext.Request.RawUrl}");
    }
}