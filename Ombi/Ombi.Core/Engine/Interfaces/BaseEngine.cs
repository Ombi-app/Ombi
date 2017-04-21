using System.Threading.Tasks;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models;

namespace Ombi.Core.Engine.Interfaces
{
    public abstract class BaseEngine
    {
        protected BaseEngine(IUserIdentityManager identity)
        {
            UserIdentity = identity;
        }

        protected IUserIdentityManager UserIdentity { get; }

        protected async Task<UserDto> GetUser(string username)
        {
            
        }

    }
}