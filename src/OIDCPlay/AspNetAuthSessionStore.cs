using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;

namespace OIDCPlay
{
    public class AspNetAuthSessionStore : IAuthenticationSessionStore
    {
        public AspNetAuthSessionStore()
        {
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            string key = Guid.NewGuid().ToString();
            HttpContext httpContext = HttpContext.Current;
            CheckSessionAvailable(httpContext);
            httpContext.Session[key + ".Ticket"] = ticket;
            return Task.FromResult(key);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            HttpContext httpContext = HttpContext.Current;
            httpContext.Session[key + ".Ticket"] = ticket;
            return Task.FromResult(0);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            HttpContext httpContext = HttpContext.Current;
            CheckSessionAvailable(httpContext);
            var ticket = httpContext.Session[key + ".Ticket"] as AuthenticationTicket;
            return Task.FromResult(ticket);
        }

        public Task RemoveAsync(string key)
        {
            HttpContext httpContext = HttpContext.Current;
            CheckSessionAvailable(httpContext);
            httpContext.Session.Remove(key + ".Ticket");
            return Task.FromResult(0);
        }

        private static void CheckSessionAvailable(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new InvalidOperationException("Not running on SystemWeb");
            }
            if (httpContext.Session == null)
            {
                throw new InvalidOperationException("Session is not enabled for this request");
            }
        }
    }
}