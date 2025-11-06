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

        protected async Task<bool> ShouldDeferToRadarr(int theMovieDbId, bool is4K)
        {
            // If no Radarr settings service injected, don't defer
            if (_radarrSettings == null || _radarrCache == null)
            {
                return false;
            }

            var radarrSettings = await _radarrSettings.GetSettingsAsync();

            // Check if Radarr is enabled, scanning for availability, and prioritizing *Arr availability
            if (radarrSettings == null || !radarrSettings.Enabled || !radarrSettings.ScanForAvailability || !radarrSettings.PrioritizeArrAvailability)
            {
                return false;
            }

            // Check if content exists in Radarr cache with HasFile = true
            var cachedMovie = await _radarrCache.GetAll()
                .Where(x => x.TheMovieDbId == theMovieDbId)
                .FirstOrDefaultAsync();

            if (cachedMovie == null)
            {
                // Content not in Radarr cache at all, allow media server to mark as available
                return false;
            }

            // If is4K, check Has4K property; otherwise check HasRegular or HasFile
            if (is4K)
            {
                return cachedMovie.Has4K;
            }
            else
            {
                return cachedMovie.HasRegular || cachedMovie.HasFile;
            }
        }

        protected async Task<bool> ShouldDeferToSonarr(int tvDbId)
        {
            // If no Sonarr settings service injected, don't defer
            if (_sonarrSettings == null || _sonarrEpisodeCache == null)
            {
                return false;
            }

            var sonarrSettings = await _sonarrSettings.GetSettingsAsync();

            // Check if Sonarr is enabled, scanning for availability, and prioritizing *Arr availability
            if (sonarrSettings == null || !sonarrSettings.Enabled || !sonarrSettings.ScanForAvailability || !sonarrSettings.PrioritizeArrAvailability)
            {
                return false;
            }

            // Check if any episodes exist in Sonarr cache with HasFile = true
            var hasAnyEpisodes = await _sonarrEpisodeCache.GetAll()
                .Where(x => x.TvDbId == tvDbId && x.HasFile)
                .AnyAsync();

            return hasAnyEpisodes;
        }
    }
}
