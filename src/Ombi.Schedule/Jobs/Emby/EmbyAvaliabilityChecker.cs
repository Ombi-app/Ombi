using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Notifications.Models;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Emby
{
    public class EmbyAvaliabilityChecker : IEmbyAvaliabilityChecker
    {
        public EmbyAvaliabilityChecker(IEmbyContentRepository repo, ITvRequestRepository t, IMovieRequestRepository m,
            INotificationHelper n, ILogger<EmbyAvaliabilityChecker> log, IHubContext<NotificationHub> notification)
        {
            _repo = repo;
            _tvRepo = t;
            _movieRepo = m;
            _notificationService = n;
            _log = log;
            _notification = notification;
        }

        private readonly ITvRequestRepository _tvRepo;
        private readonly IMovieRequestRepository _movieRepo;
        private readonly IEmbyContentRepository _repo;
        private readonly INotificationHelper _notificationService;
        private readonly ILogger<EmbyAvaliabilityChecker> _log;
        private readonly IHubContext<NotificationHub> _notification;

        public async Task Execute(IJobExecutionContext job)
        {
            _log.LogInformation("Starting Emby Availability Check");
            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Emby Availability Checker Started");
            await ProcessMovies();
            await ProcessTv();

            _log.LogInformation("Finished Emby Availability Check");
            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Emby Availability Checker Finished");
        }

        private async Task ProcessMovies()
        {
            var movies = _movieRepo.GetAll().Include(x => x.RequestedUser).Where(x => !x.Available || (!x.Available4K && x.Has4KRequest));

            foreach (var movie in movies)
            {
                var has4kRequest = movie.Has4KRequest;
                EmbyContent embyContent = null;
                if (movie.TheMovieDbId > 0)
                {
                    embyContent = await _repo.GetByTheMovieDbId(movie.TheMovieDbId.ToString());
                }
                else if(movie.ImdbId.HasValue())
                {
                    embyContent = await _repo.GetByImdbId(movie.ImdbId);
                }
                
                if (embyContent == null)
                {
                    // We don't have this yet
                    continue;
                }

                _log.LogInformation("We have found the request {0} on Emby, sending the notification", movie?.Title ?? string.Empty);

                var notify = false;

                if (has4kRequest && embyContent.Has4K && !movie.Available4K)
                {
                    movie.Available4K = true;
                    movie.MarkedAsAvailable4K = DateTime.Now;
                    notify = true;
                }

                // If we have a non-4k versison then mark as available
                if (embyContent.Quality.HasValue() && !movie.Available)
                {
                    movie.Available = true;
                    movie.MarkedAsAvailable = DateTime.Now;
                    notify = true;
                }
                if (notify)
                {
                    var recipient = movie.RequestedUser.Email.HasValue() ? movie.RequestedUser.Email : string.Empty;

                    _log.LogDebug("MovieId: {0}, RequestUser: {1}", movie.Id, recipient);

                    await _notificationService.Notify(new NotificationOptions
                    {
                        DateTime = DateTime.Now,
                        NotificationType = NotificationType.RequestAvailable,
                        RequestId = movie.Id,
                        RequestType = RequestType.Movie,
                        Recipient = recipient,
                    });
                }
            }
            await _movieRepo.Save();
        }



        /// <summary>
        /// TODO This is EXCATLY the same as the PlexAvailabilityChecker. Refactor Please future Jamie
        /// </summary>
        /// <returns></returns>
        private async Task ProcessTv()
        {
            var tv = _tvRepo.GetChild().Where(x => !x.Available);
            var embyEpisodes = _repo.GetAllEpisodes().Include(x => x.Series);

            foreach (var child in tv)
            {

                var useImdb = false;
                var useTvDb = false;
                if (child.ParentRequest.ImdbId.HasValue())
                {
                    useImdb = true;
                }

                if (child.ParentRequest.TvDbId.ToString().HasValue())
                {
                    useTvDb = true;
                }

                var tvDbId = child.ParentRequest.TvDbId;
                var imdbId = child.ParentRequest.ImdbId;
                IQueryable<IMediaServerEpisode> seriesEpisodes = null;
                if (useImdb)
                {
                    seriesEpisodes = embyEpisodes.Where(x => x.Series.ImdbId == imdbId.ToString());
                }

                if (useTvDb && (seriesEpisodes == null || !seriesEpisodes.Any()))
                {
                    seriesEpisodes = embyEpisodes.Where(x => x.Series.TvDbId == tvDbId.ToString());
                }

                if (seriesEpisodes == null)
                {
                    continue;
                }

                if (!seriesEpisodes.Any())
                {
                    // Let's try and match the series by name
                    seriesEpisodes = embyEpisodes.Where(x =>
                        x.Series.Title == child.Title);
                }

                var availableEpisode = new List<AvailabilityModel>();
                foreach (var season in child.SeasonRequests)
                {
                    foreach (var episode in season.Episodes)
                    {
                        if (episode.Available)
                        {
                            continue;
                        }

                        var foundEp = await seriesEpisodes.FirstOrDefaultAsync(
                            x => x.EpisodeNumber == episode.EpisodeNumber &&
                                 x.SeasonNumber == episode.Season.SeasonNumber);

                        if (foundEp != null)
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
                    // We have fulfulled this request!
                    child.Available = true;
                    child.MarkedAsAvailable = DateTime.Now;
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
                    await _notificationService.Notify(notification);
                }
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