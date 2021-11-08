using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models.Requests;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Services
{
    public class RecentlyRequestedService : IRecentlyRequestedService
    {
        private readonly IMovieRequestRepository _movieRequestRepository;
        private readonly ITvRequestRepository _tvRequestRepository;
        private readonly IMusicRequestRepository _musicRequestRepository;
        private readonly ISettingsService<CustomizationSettings> _customizationSettings;

        private const int AmountToTake = 7;

        public RecentlyRequestedService(
            IMovieRequestRepository movieRequestRepository,
            ITvRequestRepository tvRequestRepository,
            IMusicRequestRepository musicRequestRepository,
            ISettingsService<CustomizationSettings> customizationSettings)
        {
            _movieRequestRepository = movieRequestRepository;
            _tvRequestRepository = tvRequestRepository;
            _musicRequestRepository = musicRequestRepository;
            _customizationSettings = customizationSettings;
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
                    UserId = item.RequestedUserId,
                    Username = item.RequestedUser.UserAlias
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
                    UserId = item.RequestedUserId,
                    Username = item.RequestedUser.UserAlias
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
                    UserId = item.RequestedUserId,
                    Username = item.RequestedUser.UserAlias
                });
            }

            return model.OrderByDescending(x => x.RequestDate);
        }
    }
}
