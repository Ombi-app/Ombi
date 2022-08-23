using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserType = Ombi.Store.Entities.UserType;

namespace Ombi.Core.Services
{
    public class PlexService : IPlexService
    {
        private readonly IRepository<PlexWatchlistUserError> _watchlistUserErrors;
        private readonly OmbiUserManager _userManager;

        public PlexService(IRepository<PlexWatchlistUserError> watchlistUserErrors, OmbiUserManager userManager)
        {
            _watchlistUserErrors = watchlistUserErrors;
            _userManager = userManager;
        }

        public async Task<List<PlexUserWatchlistModel>> GetWatchlistUsers(CancellationToken cancellationToken)
        {
            var plexUsers = _userManager.Users.Where(x => x.UserType == UserType.PlexUser);
            var userErrors = await _watchlistUserErrors.GetAll().ToListAsync(cancellationToken);

            var model = new List<PlexUserWatchlistModel>();
            
            
            foreach(var plexUser in plexUsers)
            {
                model.Add(new PlexUserWatchlistModel
                {
                    UserId = plexUser.Id,
                    UserName = plexUser.UserName,
                    SyncStatus = GetWatchlistSyncStatus(plexUser, userErrors)
                });
            }

            return model;
        }

        private static WatchlistSyncStatus GetWatchlistSyncStatus(OmbiUser user, List<PlexWatchlistUserError> userErrors)
        {
            if (string.IsNullOrWhiteSpace(user.MediaServerToken))
            {
                return WatchlistSyncStatus.NotEnabled;
            }
            return userErrors.Any(x => x.UserId == user.Id) ? WatchlistSyncStatus.Failed : WatchlistSyncStatus.Successful;
        }
    }
}
