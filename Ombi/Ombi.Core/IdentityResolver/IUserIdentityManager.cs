using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models;

namespace Ombi.Core.IdentityResolver
{
    public interface IUserIdentityManager
    {
        Task<UserDto> CreateUser(UserDto user);
        Task<bool> CredentialsValid(string username, string password);
        Task<UserDto> GetUser(string username);
        Task<IEnumerable<UserDto>> GetUsers();
        Task DeleteUser(UserDto user);
        Task<UserDto> UpdateUser(UserDto userDto);
    }
}