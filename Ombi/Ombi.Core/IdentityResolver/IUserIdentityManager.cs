using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models;

namespace Ombi.Core.IdentityResolver
{
    public interface IUserIdentityManager
    {
        Task CreateUser(UserDto user);
        Task<bool> CredentialsValid(string username, string password);
        Task<UserDto> GetUser(string username);
        Task<IEnumerable<UserDto>> GetUsers();
    }
}