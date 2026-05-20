using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Schedule.Jobs
{
    public abstract class AvailabilityChecker
    {
        protected readonly ITvRequestRepository _tvRepo;
        protected readonly INotificationHelper _notificationService;
        protected readonly ILogger _log;
        protected readonly INotificationHubService NotificationHubService;
        protected readonly ISettingsService<RadarrSettings> _radarrSettings;
        protected readonly ISettingsService<SonarrSettings> _sonarrSettings;
        protected readonly IExternalRepository<RadarrCache> _radarrCache;
        protected readonly IExternalRepository<SonarrEpisodeCache> _sonarrEpisodeCache;

        public AvailabilityChecker(ITvRequestRepository tvRequest, INotificationHelper notification,
        ILogger log, INotificationHubService notificationHubService,
        ISettingsService<RadarrSettings> radarrSettings = null,
        ISettingsService<SonarrSettings> sonarrSettings = null,
        IExternalRepository<RadarrCache> radarrCache = null,
        IExternalRepository<SonarrEpisodeCache> sonarrEpisodeCache = null)
        {
            _tvRepo = tvRequest;
            _notificationService = notification;
            _log = log;
            NotificationHubService = notificationHubService;
            _radarrSettings = radarrSettings;
            _sonarrSettings = sonarrSettings;
            _radarrCache = radarrCache;
            _sonarrEpisodeCache = sonarrEpisodeCache;
        }

        protected async Task ProcessTvShow(IQueryable<IBaseMediaServerEpisode> seriesEpisodes, ChildRequests child)
        {
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
                            Id = episode.Id,
                            EpisodeNumber = episode.EpisodeNumber,
                            SeasonNumber = episode.Season.SeasonNumber
                        });
                        episode.Available = true;
                    }
                }
            }

            if (availableEpisode.Any())
            {
                await _tvRepo.Save();
            }

            // Check to see if all of the episodes in all seasons are available for this request
            var allAvailable = child.SeasonRequests.All(x => x.Episodes.All(c => c.Available));
            if (allAvailable)
            {
                // We have ful-fulled this request!
                child.Available = true;
                child.MarkedAsAvailable = DateTime.UtcNow;
                await NotificationHubService.SendNotificationToAdmins("Availability Checker found some new available Shows!");
                _log.LogInformation("Child request {0} is now available, sending notification", $"{child.Title} - {child.Id}");
                
                await _tvRepo.Save();
                await _notificationService.Notify(new NotificationOptions
                {
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.RequestAvailable,
                    RequestId = child.Id,
                    RequestType = RequestType.TvShow,
                    Recipient = child.RequestedUser.Email
                });
            }
            else if (availableEpisode.Any())
            {
                var notification = new NotificationOptions
                {
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.PartiallyAvailable,
                    RequestId = child.Id,
                    RequestType = RequestType.TvShow,
                    Recipient = child.RequestedUser.Email,
                };
                notification.Substitutes.Add("Season", availableEpisode.First().SeasonNumber.ToString());
                notification.Substitutes.Add("Episodes", string.Join(", ", availableEpisode.Select(x => x.EpisodeNumber)));
                notification.Substitutes.Add("EpisodesCount", $"{availableEpisode.Count}");
                notification.Substitutes.Add("SeasonEpisodes", string.Join(", ", availableEpisode.Select(x => $"{x.SeasonNumber}x{x.EpisodeNumber}")));
                await _notificationService.Notify(notification);
            }
        }

        /// <summary>
        /// Determines if movie availability checking should be skipped entirely because Radarr is the source of truth
        /// </summary>
        protected async Task<bool> ShouldSkipMovieAvailabilityCheck()
        {
            if (_radarrSettings == null)
            {
                return false;
            }

            // Check regular Radarr settings
            var radarrSettings = await _radarrSettings.GetSettingsAsync();
            if (radarrSettings != null && radarrSettings.Enabled &&
                radarrSettings.ScanForAvailability && radarrSettings.PrioritizeArrAvailability)
            {
                _log.LogInformation("[AvailabilityChecker] Radarr is configured to scan for availability with priority - skipping media server movie availability checks");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if TV availability checking should be skipped entirely because Sonarr is the source of truth
        /// </summary>
        protected async Task<bool> ShouldSkipTvAvailabilityCheck()
        {
            if (_sonarrSettings == null)
            {
                return false;
            }

            var sonarrSettings = await _sonarrSettings.GetSettingsAsync();
            if (sonarrSettings != null && sonarrSettings.Enabled &&
                sonarrSettings.ScanForAvailability && sonarrSettings.PrioritizeArrAvailability)
            {
                _log.LogInformation("[AvailabilityChecker] Sonarr is configured to scan for availability with priority - skipping media server TV availability checks");
                return true;
            }

            return false;
        }
    }
}
