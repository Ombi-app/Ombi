using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Schedule.Jobs
{
    public class AvailabilityChecker
    {
        protected readonly ITvRequestRepository _tvRepo;
        protected readonly INotificationHelper _notificationService;
        protected readonly ILogger _log;
        protected readonly IHubContext<NotificationHub> _hub;

        public AvailabilityChecker(ITvRequestRepository tvRequest, INotificationHelper notification,
        ILogger log, IHubContext<NotificationHub> hub)
        {
            _tvRepo = tvRequest;
            _notificationService = notification;
            _log = log;
            _hub = hub;
        }

        protected async void ProcessTvShow(IQueryable<IBaseMediaServerEpisode> seriesEpisodes, ChildRequests child)
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
                 await _hub.Clients.Clients(NotificationHub.AdminConnectionIds)
                        .SendAsync(NotificationHub.NotificationEvent, "Availability Checker found some new available Shows!");
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
    }
}
