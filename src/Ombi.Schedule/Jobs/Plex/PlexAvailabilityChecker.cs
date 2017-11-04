using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Notifications;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexAvailabilityChecker : IPlexAvailabilityChecker
    {
        public PlexAvailabilityChecker(IPlexContentRepository repo, ITvRequestRepository tvRequest, IMovieRequestRepository movies,
            INotificationService notification, IBackgroundJobClient background)
        {
            _tvRepo = tvRequest;
            _repo = repo;
            _movieRepo = movies;
            _notificationService = notification;
            _backgroundJobClient = background;
        }

        private readonly ITvRequestRepository _tvRepo;
        private readonly IMovieRequestRepository _movieRepo;
        private readonly IPlexContentRepository _repo;
        private readonly INotificationService _notificationService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public async Task Start()
        {
            await ProcessMovies();
            await ProcessTv();
        }

        private async Task ProcessTv()
        {
            var tv = _tvRepo.GetChild().Where(x => !x.Available);
            var plexEpisodes = _repo.GetAllEpisodes().Include(x => x.Series);

            foreach (var child in tv)
            {

                PlexServerContent item = null;
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
                IQueryable<PlexEpisode> seriesEpisodes = null;
                if (useImdb)
                {
                    seriesEpisodes = plexEpisodes.Where(x => x.Series.ImdbId == imdbId.ToString());
                }
                if (useTvDb)
                {
                    seriesEpisodes = plexEpisodes.Where(x => x.Series.TvDbId == tvDbId.ToString());
                }
                foreach (var season in child.SeasonRequests)
                {
                    foreach (var episode in season.Episodes)
                    {
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
                    _backgroundJobClient.Enqueue(() => _notificationService.Publish(new NotificationOptions
                    {
                        DateTime = DateTime.Now,
                        NotificationType = NotificationType.RequestAvailable,
                        RequestId = child.ParentRequestId,
                        RequestType = RequestType.TvShow,
                        Recipient = child.RequestedUser.Email
                    }));
                }
            }

            await _tvRepo.Save();
        }

        private async Task ProcessMovies()
        {
            // Get all non available
            var movies = _movieRepo.GetAll().Where(x => !x.Available);

            foreach (var movie in movies)
            {
                PlexServerContent item = null;
                if (movie.ImdbId.HasValue())
                {
                    item = await _repo.Get(movie.ImdbId);
                }
                if (item == null)
                {
                    if (movie.TheMovieDbId.ToString().HasValue())
                    {
                        item = await _repo.Get(movie.TheMovieDbId.ToString());
                    }
                }
                if (item == null)
                {
                    // We don't yet have this
                    continue;
                }

                movie.Available = true;
                if (movie.Available)
                {
                    _backgroundJobClient.Enqueue(() => _notificationService.Publish(new NotificationOptions
                    {
                        DateTime = DateTime.Now,
                        NotificationType = NotificationType.RequestAvailable,
                        RequestId = movie.Id,
                        RequestType = RequestType.Movie,
                        Recipient = movie.RequestedUser != null ? movie.RequestedUser.Email : string.Empty
                    }));
                }
            }

            await _movieRepo.Save();
        }
    }
}