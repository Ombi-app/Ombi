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
using Ombi.Schedule.Jobs.Plex.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Radarr
{
    public class ArrAvailabilityChecker : IArrAvailabilityChecker
    {
        private readonly IExternalRepository<RadarrCache> _radarrRepo;
        private readonly IExternalRepository<SonarrCache> _sonarrRepo;
        private readonly ILogger<ArrAvailabilityChecker> _logger;
        private readonly ISettingsService<RadarrSettings> _radarrSettings;
        private readonly ISettingsService<SonarrSettings> _sonarrSettings;
        private readonly IExternalRepository<SonarrEpisodeCache> _sonarrEpisodeRepo;
        private readonly INotificationHelper _notification;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ITvRequestRepository _tvRequest;
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
        {
            _radarrRepo = radarrRepo;
            _sonarrRepo = sonarrRepo;
            _sonarrEpisodeRepo = sonarrEpisodeRepo;
            _notification = notification;
            _hub = hub;
            _tvRequest = tvRequest;
            _movies = movies;
            _logger = log;
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
            var unavailableMovieRequests = _movies.GetAll().Where(x => !x.Available).ToImmutableHashSet();

            var itemsForAvailability = new List<AvailabilityModel>();
            foreach (var movieRequest in unavailableMovieRequests)
            {
                // Do we have an item in the radarr list
                var available = availableRadarrMovies.Any(x => x.TheMovieDbId == movieRequest.TheMovieDbId);
                if (available)
                {
                    _logger.LogInformation($"Found move '{movieRequest.Title}' available in Radarr");
                    movieRequest.Available = true;
                    movieRequest.MarkedAsAvailable = DateTime.UtcNow;
                    itemsForAvailability.Add(new AvailabilityModel
                    {
                        Id = movieRequest.Id,
                        RequestedUser = movieRequest.RequestedUser != null ? movieRequest.RequestedUser.Email : string.Empty
                    });
                }
            }

            if (itemsForAvailability.Any())
            {
                await _hub.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Radarr Availability Checker found some new available movies!");
                await _movies.SaveChangesAsync();
            }
            foreach (var item in itemsForAvailability)
            {
                await _notification.Notify(new NotificationOptions
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
            var tv = await _tvRequest.GetChild().Where(x => !x.Available).ToListAsync();
            var sonarrEpisodes = _sonarrEpisodeRepo.GetAll().Where(x => x.HasFile);

            foreach (var child in tv)
            {
                var tvDbId = child.ParentRequest.TvDbId;
                IQueryable<SonarrEpisodeCache> seriesEpisodes = sonarrEpisodes.Where(x => x.TvDbId == tvDbId);

                if (seriesEpisodes == null || !seriesEpisodes.Any())
                {
                    continue;
                }

                //if (!seriesEpisodes.Any())
                //{
                //    // Let's try and match the series by name
                //    seriesEpisodes = sonarrEpisodes.Where(x =>
                //        x.EpisodeNumber == child.Title &&
                //        x.Series.ReleaseYear == child.ParentRequest.ReleaseDate.Year.ToString());

                //}

                var availableEpisode = new List<AvailabilityModel>();
                foreach (var season in child.SeasonRequests)
                {
                    foreach (var episode in season.Episodes)
                    {
                        if (episode.Available)
                        {
                            continue;
                        }
                        var foundEp = await seriesEpisodes.AnyAsync(
                            x => x.EpisodeNumber == episode.EpisodeNumber &&
                                 x.SeasonNumber == episode.Season.SeasonNumber);

                        if (foundEp)
                        {
                            availableEpisode.Add(new AvailabilityModel
                            {
                                Id = episode.Id
                            });
                            episode.Available = true;
                        }
                    }
                }

                //TODO Partial avilability notifications here
                if (availableEpisode.Any())
                {
                    //await _hub.Clients.Clients(NotificationHub.AdminConnectionIds)
                    //    .SendAsync(NotificationHub.NotificationEvent, "Sonarr Availability Checker found some new available episodes!");
                    await _tvRequest.Save();
                }
                //foreach(var c in availableEpisode)
                //{
                //    await _tvRepo.MarkEpisodeAsAvailable(c.Id);
                //}

                // Check to see if all of the episodes in all seasons are available for this request
                var allAvailable = child.SeasonRequests.All(x => x.Episodes.All(c => c.Available));
                if (allAvailable)
                {
                    await _hub.Clients.Clients(NotificationHub.AdminConnectionIds)
                        .SendAsync(NotificationHub.NotificationEvent, "Sonarr Availability Checker found some new available Shows!");
                    child.Available = true;
                    child.MarkedAsAvailable = DateTime.UtcNow;
                    _logger.LogInformation("[ARR_AC] - Child request {0} is now available, sending notification", $"{child.Title} - {child.Id}");
                    // We have ful-fulled this request!
                    await _tvRequest.Save();
                    await _notification.Notify(new NotificationOptions
                    {
                        DateTime = DateTime.Now,
                        NotificationType = NotificationType.RequestAvailable,
                        RequestId = child.Id,
                        RequestType = RequestType.TvShow,
                        Recipient = child.RequestedUser.Email
                    });
                }
            }

            await _tvRequest.Save();
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
