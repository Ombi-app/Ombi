#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: JellyfinAvaliabilityCheker.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
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

namespace Ombi.Schedule.Jobs.Jellyfin
{
    public class JellyfinAvaliabilityChecker : IJellyfinAvaliabilityChecker
    {
        public JellyfinAvaliabilityChecker(IJellyfinContentRepository repo, ITvRequestRepository t, IMovieRequestRepository m,
            INotificationHelper n, ILogger<JellyfinAvaliabilityChecker> log, IHubContext<NotificationHub> notification)
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
        private readonly IJellyfinContentRepository _repo;
        private readonly INotificationHelper _notificationService;
        private readonly ILogger<JellyfinAvaliabilityChecker> _log;
        private readonly IHubContext<NotificationHub> _notification;

        public async Task Execute(IJobExecutionContext job)
        {
            _log.LogInformation("Starting Jellyfin Availability Check");
            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Jellyfin Availability Checker Started");
            await ProcessMovies();
            await ProcessTv();

            _log.LogInformation("Finished Jellyfin Availability Check");
            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Jellyfin Availability Checker Finished");
        }

        private async Task ProcessMovies()
        {
            var movies = _movieRepo.GetAll().Include(x => x.RequestedUser).Where(x => !x.Available);

            foreach (var movie in movies)
            {
                JellyfinContent jellyfinContent = null;
                if (movie.TheMovieDbId > 0)
                {
                    jellyfinContent = await _repo.GetByTheMovieDbId(movie.TheMovieDbId.ToString());
                }
                else if(movie.ImdbId.HasValue())
                {
                    jellyfinContent = await _repo.GetByImdbId(movie.ImdbId);
                }
                
                if (jellyfinContent == null)
                {
                    // We don't have this yet
                    continue;
                }

                _log.LogInformation("We have found the request {0} on Jellyfin, sending the notification", movie?.Title ?? string.Empty);

                movie.Available = true;
                movie.MarkedAsAvailable = DateTime.Now;
                if (movie.Available)
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
            var jellyfinEpisodes = _repo.GetAllEpisodes().Include(x => x.Series);

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
                IQueryable<JellyfinEpisode> seriesEpisodes = null;
                if (useImdb)
                {
                    seriesEpisodes = jellyfinEpisodes.Where(x => x.Series.ImdbId == imdbId.ToString());
                }

                if (useTvDb && (seriesEpisodes == null || !seriesEpisodes.Any()))
                {
                    seriesEpisodes = jellyfinEpisodes.Where(x => x.Series.TvDbId == tvDbId.ToString());
                }

                if (seriesEpisodes == null)
                {
                    continue;
                }

                if (!seriesEpisodes.Any())
                {
                    // Let's try and match the series by name
                    seriesEpisodes = jellyfinEpisodes.Where(x =>
                        x.Series.Title == child.Title);
                }

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
                            episode.Available = true;
                        }
                    }
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