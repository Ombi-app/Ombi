using Ombi.Core.Authentication;
using Ombi.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserType = Ombi.Store.Entities.UserType;

namespace Ombi.Core.Services
{
    public class PlexService : IPlexService
    {
        private readonly OmbiUserManager _userManager;
        private readonly IPlexWatchlistStatusStore _statusStore;

        public PlexService(OmbiUserManager userManager, IPlexWatchlistStatusStore statusStore)
        {
            _userManager = userManager;
            _statusStore = statusStore;
        }

        public Task<List<PlexUserWatchlistModel>> GetWatchlistUsers(CancellationToken cancellationToken)
        {
            var plexUsers = _userManager.Users.Where(x => x.UserType == UserType.PlexUser).ToList();

            var model = plexUsers.Select(plexUser => new PlexUserWatchlistModel
            {
                UserId = plexUser.Id,
                UserName = plexUser.UserName,
                SyncStatus = _statusStore.Get(plexUser.Id) ?? WatchlistSyncStatus.NotAFriend,
            }).ToList();

            return Task.FromResult(model);
        }

        public Task ForceRevalidateWatchlistUsers(CancellationToken cancellationToken)
        {
            _statusStore.Clear();
            return Task.CompletedTask;
        }
    }
}
