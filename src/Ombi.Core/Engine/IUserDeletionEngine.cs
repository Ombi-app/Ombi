using Microsoft.AspNetCore.Identity;
using Ombi.Store.Entities;
using System.Threading.Tasks;

namespace Ombi.Core.Engine
{
    public interface IUserDeletionEngine
    {
        Task<IdentityResult> DeleteUser(OmbiUser userToDelete);
    }
}
