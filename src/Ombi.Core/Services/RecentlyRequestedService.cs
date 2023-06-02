using Microsoft.EntityFrameworkCore;
using Ombi.Api.TheMovieDb;
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
        private readonly IMovieDbApi _movieDbApi;
        private readonly ICacheService _cache;
        private const int AmountToTake = 7;

        public RecentlyRequestedService(
            IMovieRequestRepository movieRequestRepository,
            ITvRequestRepository tvRequestRepository,
            IMusicRequestRepository musicRequestRepository,
            ISettingsService<CustomizationSettings> customizationSettings,
            ISettingsService<OmbiSettings> ombiSettings,
            ICurrentUser user,
            OmbiUserManager um,
            IRuleEvaluator rules,
            IMovieDbApi movieDbApi,
            ICacheService cache) : base(user, um, rules)
        {
            _movieRequestRepository = movieRequestRepository;
            _tvRequestRepository = tvRequestRepository;
            _musicRequestRepository = musicRequestRepository;
            _customizationSettings = customizationSettings;
            _ombiSettings = ombiSettings;
            _movieDbApi = movieDbApi;
            _cache = cache;
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

            var lang = await DefaultLanguageCode();
            foreach (var item in await recentMovieRequests.ToListAsync(cancellationToken))
            {
                if (hideUsers.Hide && item.RequestedUserId != hideUsers.UserId)
                {
                    continue;
                }
                var images = await _cache.GetOrAddAsync($"{CacheKeys.TmdbImages}movie{item.TheMovieDbId}", () => _movieDbApi.GetMovieImages(item.TheMovieDbId.ToString(), cancellationToken), DateTimeOffset.Now.AddDays(1));
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
                    Denied = item.Denied ?? false,
                    UserId = item.RequestedUserId,
                    Username = item.RequestedUser.UserAlias,
                    MediaId = item.TheMovieDbId.ToString(),
                    PosterPath = images?.posters?.Where(x => lang.Equals(x?.iso_639_1, StringComparison.InvariantCultureIgnoreCase))?.OrderByDescending(x => x.vote_count)?.Select(x => x.file_path)?.FirstOrDefault(),
                    Background = images?.backdrops?.Where(x => lang.Equals(x?.iso_639_1, StringComparison.InvariantCultureIgnoreCase))?.OrderByDescending(x => x.vote_count)?.Select(x => x.file_path)?.FirstOrDefault(),
                });
            }

            foreach (var item in await recentMusicRequests.ToListAsync(cancellationToken))
            {
                if (hideUsers.Hide && item.RequestedUserId != hideUsers.UserId)
                {
                    continue;
                }
                model.Add(new RecentlyRequestedModel
                {
                    RequestId = item.Id,
                    Available = item.Available,
                    Overview = item.ArtistName,
                    Approved = item.Approved,
                    Denied = item.Denied ?? false,
                    ReleaseDate = item.ReleaseDate,
                    RequestDate = item.RequestedDate,
                    Title = item.Title,
                    Type = RequestType.Album,
                    UserId = item.RequestedUserId,
                    Username = item.RequestedUser.UserAlias,
                    MediaId = item.ForeignAlbumId,
                });
            }

            foreach (var item in await recentTvRequests.ToListAsync(cancellationToken))
            {
                if (hideUsers.Hide && item.RequestedUserId != hideUsers.UserId)
                {
                    continue;
                }
                var providerId = item.ParentRequest.ExternalProviderId.ToString();
                var images = await _cache.GetOrAddAsync($"{CacheKeys.TmdbImages}tv{providerId}", () => _movieDbApi.GetTvImages(providerId.ToString(), cancellationToken), DateTimeOffset.Now.AddDays(1));
                
                var partialAvailability = item.SeasonRequests.SelectMany(x => x.Episodes).Any(e => e.Available);
                model.Add(new RecentlyRequestedModel
                {
                    RequestId = item.Id,
                    Available = item.Available,
                    Overview = item.ParentRequest.Overview,
                    ReleaseDate = item.ParentRequest.ReleaseDate,
                    Approved = item.Approved,
                    Denied = item.Denied ?? false,
                    RequestDate = item.RequestedDate,
                    TvPartiallyAvailable = partialAvailability,
                    Title = item.ParentRequest.Title,
                    Type = RequestType.TvShow,
                    UserId = item.RequestedUserId,
                    Username = item.RequestedUser.UserAlias,
                    MediaId = providerId.ToString(),
                    PosterPath = images?.posters?.Where(x => lang.Equals(x?.iso_639_1, StringComparison.InvariantCultureIgnoreCase))?.OrderByDescending(x => x.vote_count)?.Select(x => x.file_path)?.FirstOrDefault(),
                    Background = images?.backdrops?.Where(x => lang.Equals(x?.iso_639_1, StringComparison.InvariantCultureIgnoreCase))?.OrderByDescending(x => x.vote_count)?.Select(x => x.file_path)?.FirstOrDefault(),
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
            var settings = await GetOmbiSettings();
            var result = new HideResult
            {
                Hide = settings.HideRequestsUsers,
                UserId = user.Id
            };
            return result;
        }
        protected async Task<string> DefaultLanguageCode()
        {
            var user = await GetUser();
            if (user == null)
            {
                return "en";
            }

            if (string.IsNullOrEmpty(user.Language))
            {
                var s = await GetOmbiSettings();
                return s.DefaultLanguageCode;
            }

            return user.Language;
        }


        private OmbiSettings ombiSettings;
        protected async Task<OmbiSettings> GetOmbiSettings()
        {
            return ombiSettings ??= await _ombiSettings.GetSettingsAsync();
        }
    }
}
