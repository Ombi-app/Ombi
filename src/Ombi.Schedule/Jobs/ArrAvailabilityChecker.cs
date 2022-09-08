using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Radarr
{
    public class ArrAvailabilityChecker : AvailabilityChecker, IArrAvailabilityChecker
    {
        private readonly IExternalRepository<RadarrCache> _radarrRepo;
        private readonly IExternalRepository<SonarrCache> _sonarrRepo;
        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly ISettingsService<SonarrSettings> _sonarrSettings;
        private readonly IExternalRepository<SonarrEpisodeCache> _sonarrEpisodeRepo;
        private readonly IMovieRequestRepository _movies;

        public ArrAvailabilityChecker(
            IExternalRepository<RadarrCache> radarrRepo,
            IExternalRepository<SonarrCache> sonarrRepo,
            IExternalRepository<SonarrEpisodeCache> sonarrEpisodeRepo,
            INotificationHelper notification, IHubContext<NotificationHub> hub,
            ITvRequestRepository tvRequest, IMovieRequestRepository movies,
            ILogger<ArrAvailabilityChecker> log,
            ISettingsService<RadarrSettings> radarrSettings,
            ISettingsService<SonarrSettings> sonarrSettings)
             : base(tvRequest, notification, log, hub)
        {
            _radarrRepo = radarrRepo;
            _sonarrRepo = sonarrRepo;
            _sonarrEpisodeRepo = sonarrEpisodeRepo;
            _movies = movies;
            _radarrSettings = radarrSettings;
            _sonarrSettings = sonarrSettings;
        }
        public async Task Execute(IJobExecutionContext job)
        {
            var radarrSettings = await _radarrSettings.GetSettingsAsync();
            var sonarrSettings = await _sonarrSettings.GetSettingsAsync();

            if (radarrSettings.ScanForAvailability)
            {
                await ProcessMovies();
            }

            if (sonarrSettings.ScanForAvailability)
            {
                await ProcessTvShows();
            }
        }

        private async Task ProcessMovies()
        {
            var availableRadarrMovies = _radarrRepo.GetAll().Where(x => x.HasFile).ToImmutableHashSet();
            var unavailableMovieRequests = _movies.GetAll().Where(x => !x.Available || (!x.Available4K && x.Has4KRequest)).ToImmutableHashSet();

            var itemsForAvailability = new List<AvailabilityModel>();
            foreach (var movieRequest in unavailableMovieRequests)
            {
                // Do we have an item in the radarr list
                var available = availableRadarrMovies.FirstOrDefault(x => x.TheMovieDbId == movieRequest.TheMovieDbId);
                if (available != null)
                {
                    _log.LogInformation($"Found move '{movieRequest.Title}' available in Radarr");
                    if (available.Has4K && !movieRequest.Available4K)
                    {
                        itemsForAvailability.Add(new AvailabilityModel
                        {
                            Id = movieRequest.Id,
                            RequestedUser = movieRequest.RequestedUser != null ? movieRequest.RequestedUser.Email : string.Empty
                        });
                        movieRequest.Available4K = true;
                        movieRequest.MarkedAsAvailable4K = DateTime.UtcNow;
                    }
                    if (available.HasRegular)
                    {
                        itemsForAvailability.Add(new AvailabilityModel
                        {
                            Id = movieRequest.Id,
                            RequestedUser = movieRequest.RequestedUser != null ? movieRequest.RequestedUser.Email : string.Empty
                        });
                        movieRequest.Available = true;
                        movieRequest.MarkedAsAvailable = DateTime.UtcNow;
                    }
                    await _movies.SaveChangesAsync();
                }
            }

            if (itemsForAvailability.Any())
            {
                await _hub.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Radarr Availability Checker found some new available movies!");
            }
            foreach (var item in itemsForAvailability)
            {
                await _notificationService.Notify(new NotificationOptions
                {
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.RequestAvailable,
                    RequestId = item.Id,
                    RequestType = RequestType.Movie,
                    Recipient = item.RequestedUser
                });
            }
        }

        public async Task ProcessTvShows()
        {
            var tv = await _tvRepo.GetChild().Where(x => !x.Available).ToListAsync();
            var sonarrEpisodes = _sonarrEpisodeRepo.GetAll().Where(x => x.HasFile);
            
            foreach (var child in tv)
            {
                var tvDbId = child.ParentRequest.TvDbId;
                IQueryable<SonarrEpisodeCache> seriesEpisodes = sonarrEpisodes.Where(x => x.TvDbId == tvDbId);

                if (seriesEpisodes == null || !seriesEpisodes.Any())
                {
                    continue;
                }

                ProcessTvShow(seriesEpisodes, child);
            }

            await _tvRepo.Save();
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
