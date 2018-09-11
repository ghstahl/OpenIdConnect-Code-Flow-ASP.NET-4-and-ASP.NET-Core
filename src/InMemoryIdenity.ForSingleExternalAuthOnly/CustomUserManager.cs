using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace InMemoryIdenity.ForSingleExternalAuthOnly
{
    public class CustomUserManager : UserManager<CustomUser>
    {
    
        public CustomUserManager(InMemoryUserStore store)
            : base(store)
        {
        }

        public static CustomUserManager Create(IdentityFactoryOptions<CustomUserManager> options,
            IOwinContext context)
        {
            var manager = new CustomUserManager(new InMemoryUserStore());
            return manager;
        }

    }
}