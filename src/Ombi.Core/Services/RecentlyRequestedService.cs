using Microsoft.EntityFrameworkCore;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Ombi.Core.Engine.BaseMediaEngine;

namespace Ombi.Core.Services
{
    public class RecentlyRequestedService : BaseEngine, IRecentlyRequestedService
    {
        private readonly IMovieRequestRepository _movieRequestRepository;
        private readonly ITvRequestRepository _tvRequestRepository;
        private readonly IMusicRequestRepository _musicRequestRepository;
        private readonly ISettingsService<CustomizationSettings> _customizationSettings;
        private readonly ISettingsService<OmbiSettings> _ombiSettings;

        private const int AmountToTake = 7;

        public RecentlyRequestedService(
            IMovieRequestRepository movieRequestRepository,
            ITvRequestRepository tvRequestRepository,
            IMusicRequestRepository musicRequestRepository,
            ISettingsService<CustomizationSettings> customizationSettings,
            ISettingsService<OmbiSettings> ombiSettings,
            ICurrentUser user,
            OmbiUserManager um,
            IRuleEvaluator rules) : base(user, um, rules)
        {
            _movieRequestRepository = movieRequestRepository;
            _tvRequestRepository = tvRequestRepository;
            _musicRequestRepository = musicRequestRepository;
            _customizationSettings = customizationSettings;
            _ombiSettings = ombiSettings;
        }

        public async Task<IEnumerable<RecentlyRequestedModel>> GetRecentlyRequested(CancellationToken cancellationToken)
        {
            var customizationSettingsTask = _customizationSettings.GetSettingsAsync();

            var recentMovieRequests = _movieRequestRepository.GetAll().Include(x => x.RequestedUser).OrderByDescending(x => x.RequestedDate).Take(AmountToTake);
            var recentTvRequests = _tvRequestRepository.GetChild().Include(x => x.RequestedUser).Include(x => x.ParentRequest).OrderByDescending(x => x.RequestedDate).Take(AmountToTake);
            var recentMusicRequests = _musicRequestRepository.GetAll().Include(x => x.RequestedUser).OrderByDescending(x => x.RequestedDate).Take(AmountToTake);

            var settings = await customizationSettingsTask;
            if (settings.HideAvailableRecentlyRequested)
            {
                recentMovieRequests = recentMovieRequests.Where(x => !x.Available);
                recentTvRequests = recentTvRequests.Where(x => !x.Available);
                recentMusicRequests = recentMusicRequests.Where(x => !x.Available);
            }
            var hideUsers = await HideFromOtherUsers();

            var model = new List<RecentlyRequestedModel>();

            foreach (var item in await recentMovieRequests.ToListAsync(cancellationToken))
            {
                model.Add(new RecentlyRequestedModel
                {
                    RequestId = item.Id,
                    Available = item.Available,
                    Overview = item.Overview,
                    ReleaseDate = item.ReleaseDate,
                    RequestDate = item.RequestedDate,
                    Title = item.Title,
                    Type = RequestType.Movie,
                    Approved = item.Approved,
                    UserId = hideUsers.Hide ? string.Empty : item.RequestedUserId,
                    Username = hideUsers.Hide ? string.Empty : item.RequestedUser.UserAlias,
                    MediaId = item.TheMovieDbId.ToString(),
                });
            }

            foreach (var item in await recentMusicRequests.ToListAsync(cancellationToken))
            {
                model.Add(new RecentlyRequestedModel
                {
                    RequestId = item.Id,
                    Available = item.Available,
                    Overview = item.ArtistName,
                    Approved = item.Approved,
                    ReleaseDate = item.ReleaseDate,
                    RequestDate = item.RequestedDate,
                    Title = item.Title,
                    Type = RequestType.Album,
                    UserId = hideUsers.Hide ? string.Empty : item.RequestedUserId,
                    Username = hideUsers.Hide ? string.Empty : item.RequestedUser.UserAlias,
                    MediaId = item.ForeignAlbumId,
                });
            }

            foreach (var item in await recentTvRequests.ToListAsync(cancellationToken))
            {
                var partialAvailability = item.SeasonRequests.SelectMany(x => x.Episodes).Any(e => e.Available);
                model.Add(new RecentlyRequestedModel
                {
                    RequestId = item.Id,
                    Available = item.Available,
                    Overview = item.ParentRequest.Overview,
                    ReleaseDate = item.ParentRequest.ReleaseDate,
                    Approved = item.Approved,
                    RequestDate = item.RequestedDate,
                    TvPartiallyAvailable = partialAvailability,
                    Title = item.ParentRequest.Title,
                    Type = RequestType.TvShow,
                    UserId = hideUsers.Hide ? string.Empty : item.RequestedUserId,
                    Username = hideUsers.Hide ? string.Empty : item.RequestedUser.UserAlias,
                    MediaId = item.ParentRequest.ExternalProviderId.ToString()
                });
            }

            return model.OrderByDescending(x => x.RequestDate);
        }

        private async Task<HideResult> HideFromOtherUsers()
        {
            var user = await GetUser();
            if (await IsInRole(OmbiRoles.Admin) || await IsInRole(OmbiRoles.PowerUser) || user.IsSystemUser)
            {
                return new HideResult
                {
                    UserId = user.Id
                };
            }
            var settings = await _ombiSettings.GetSettingsAsync();
            var result = new HideResult
            {
                Hide = settings.HideRequestsUsers,
                UserId = user.Id
            };
            return result;
        }
    }
}
