using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using InMemoryIdenity.ForSingleExternalAuthOnly;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace OIDCPlay.Models
{
    // You can add profile data for the user by adding more properties to your CustomUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
     

    public class ApplicationDbContext : IdentityDbContext<CustomUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}