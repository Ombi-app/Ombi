using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Store.Entities;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core.Helpers
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IPrincipal _principle;
        private readonly OmbiUserManager _userManager;
        private OmbiUser _user;
        public IIdentity Identity { get; set; }

        public CurrentUser(IPrincipal principle, OmbiUserManager userManager)
        {
            _principle = principle;
            _userManager = userManager;
            Identity = _principle?.Identity;
        }

        public void SetUser(OmbiUser user)
        {
            _user = user;
        }

        public string Username => Identity.Name;
        public async Task<OmbiUser> GetUser()
        {
            if (!Username.HasValue() && _user == null)
            {
                return null;
            }

            if (_user != null)
            {
                return _user;
            }

            var username = Username.ToUpper();
            return _user ??= await _userManager.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username);
        }

    }
}
