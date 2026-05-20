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

        public async Task<List<PlexUserWatchlistModel>> GetWatchlistUsers(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var plexUsers = _userManager.Users.Where(x => x.UserType == UserType.PlexUser).ToList();
            var statuses = await _statusStore.GetAllAsync(cancellationToken);

            return plexUsers.Select(plexUser => new PlexUserWatchlistModel
            {
                UserId = plexUser.Id,
                UserName = plexUser.UserName,
                SyncStatus = statuses.TryGetValue(plexUser.Id, out var status) ? status : WatchlistSyncStatus.Pending,
            }).ToList();
        }

        public Task ForceRevalidateWatchlistUsers(CancellationToken cancellationToken)
        {
            return _statusStore.ClearAsync(cancellationToken);
        }
    }
}
