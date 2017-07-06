using Ombi.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Core.IdentityResolver
{
    public interface IUserIdentityManager
    {
        Task<UserDto> CreateUser(UserDto user);

        Task<bool> CredentialsValid(string username, string password);

        Task<UserDto> GetUser(string username);
        Task<UserDto> GetUser(int userId);

        Task<IEnumerable<UserDto>> GetUsers();

        Task DeleteUser(UserDto user);

        Task<UserDto> UpdateUser(UserDto userDto);
    }
}