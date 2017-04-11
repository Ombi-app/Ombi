using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Core.IdentityResolver
{
    public interface IUserIdentityManager
    {
        Task CreateUser(User user);
        Task<bool> CredentialsValid(string username, string password);
        Task<User> GetUser(string username);
        Task<IEnumerable<User>> GetUsers();
    }
}