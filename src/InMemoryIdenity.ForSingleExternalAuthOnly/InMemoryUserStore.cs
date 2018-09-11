using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace InMemoryIdenity.ForSingleExternalAuthOnly
{
    public class InMemoryUserStore : IUserStore<CustomUser>, IUserPasswordStore<CustomUser>, IUserLoginStore<CustomUser>
    {
        private Dictionary<string,CustomUser> userMap = new Dictionary<string, CustomUser>();
        private IList<CustomUser> userList = new List<CustomUser>();

        public InMemoryUserStore()
        {
            //userList.Add(new CustomUser("1", "admin", Crypto.HashPassword("admin")));
        }

        public void Dispose()
        {

        }

        public Task CreateAsync(CustomUser user)
        {
            userMap[user.Id] = user;
            return Task.FromResult(true);
        }

        public Task UpdateAsync(CustomUser user)
        {
            userMap[user.Id] =  user;
            return Task.FromResult(true);
        }

        public Task DeleteAsync(CustomUser user)
        {
            userMap.Remove(user.Id);
            return Task.FromResult(true);
        }

        public Task<CustomUser> FindByIdAsync(string userId)
        {
            CustomUser user;
            if (userMap.TryGetValue(userId, out user))
            {
                return Task.FromResult<CustomUser>(user);
            }
            return Task.FromResult<CustomUser>(null);
//            return Task.FromResult(userList.FirstOrDefault(x => x.Id.Equals(userId)));
        }

        public Task<CustomUser> FindByNameAsync(string userName)
        {
            return Task.FromResult<CustomUser>(null);
            return Task.FromResult(userList.FirstOrDefault(x => x.UserName.Equals(userName)));
        }

        public Task SetPasswordHashAsync(CustomUser user, string passwordHash)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(CustomUser user)
        {
            return Task.FromResult("hi");
//            return Task.FromResult(userList.First(x => x.UserName.Equals(user.UserName)).hashedPassword);
        }

        public Task<bool> HasPasswordAsync(CustomUser user)
        {
            var appUser = userList.FirstOrDefault(x => x.UserName.Equals(user.UserName));
            if (appUser == null)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
            //return Task.FromResult(!String.IsNullOrEmpty(appUser.hashedPassword));
        }

        public Task AddLoginAsync(CustomUser user, UserLoginInfo login)
        {
            return Task.FromResult(true);
        }

        public Task RemoveLoginAsync(CustomUser user, UserLoginInfo login)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(CustomUser user)
        {
            throw new NotImplementedException();
        }

        public Task<CustomUser> FindAsync(UserLoginInfo login)
        {
            return Task.FromResult<CustomUser>(null);
            //throw new NotImplementedException();
        }
    }
}