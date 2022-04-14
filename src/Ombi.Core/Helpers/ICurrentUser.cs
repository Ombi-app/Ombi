using Ombi.Store.Entities;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core.Helpers
{
    public interface ICurrentUser
    {
        string Username { get; }

        Task<OmbiUser> GetUser();
        void SetUser(OmbiUser user);
        IIdentity Identity { get; set; }
    }
}