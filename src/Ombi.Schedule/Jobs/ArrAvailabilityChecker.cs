using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ombi.Core;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Notifications.Models;
using Ombi.Schedule.Jobs.Plex.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Radarr
{
    public class ArrAvailabilityChecker
    {
        public ArrAvailabilityChecker(
            IExternalRepository<RadarrCache> radarrRepo,
            IExternalRepository<SonarrCache> sonarrRepo,
            IExternalRepository<SonarrEpisodeCache> sonarrEpisodeRepo,
            INotificationHelper notification, IHubContext<NotificationHub> hub,
            ITvRequestRepository tvRequest, IMovieRequestRepository movies,
            ILogger<ArrAvailabilityChecker> log)
        {
            _radarrRepo = radarrRepo;
            _sonarrRepo = sonarrRepo;
            _sonarrEpisodeRepo = sonarrEpisodeRepo;
            _notification = notification;
            _hub = hub;
            _tvRequest = tvRequest;
            _movies = movies;
            _logger = log;
        }

        private readonly IExternalRepository<RadarrCache> _radarrRepo;
        private readonly IExternalRepository<SonarrCache> _sonarrRepo;
        private readonly ILogger<ArrAvailabilityChecker> _logger;
        private readonly IExternalRepository<SonarrEpisodeCache> _sonarrEpisodeRepo;
        private readonly INotificationHelper _notification;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ITvRequestRepository _tvRequest;
        private readonly IMovieRequestRepository _movies;

        public async Task Execute(IJobExecutionContext job)
        {
            
            await ProcessMovies();
            await ProcessTvShows();

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

        }

    }
}
