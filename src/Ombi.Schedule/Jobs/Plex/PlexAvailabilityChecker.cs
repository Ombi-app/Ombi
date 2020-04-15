using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Schedule.Jobs.Plex.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexAvailabilityChecker : IPlexAvailabilityChecker
    {
        public PlexAvailabilityChecker(IPlexContentRepository repo, ITvRequestRepository tvRequest, IMovieRequestRepository movies,
            INotificationHelper notification,  ILogger<PlexAvailabilityChecker> log)
        {
            _tvRepo = tvRequest;
            _repo = repo;
            _movieRepo = movies;
            _notificationService = notification;
            _log = log;
        }

        private readonly ITvRequestRepository _tvRepo;
        private readonly IMovieRequestRepository _movieRepo;
        private readonly IPlexContentRepository _repo;
        private readonly INotificationHelper _notificationService;
        private readonly ILogger _log;

        public async Task Execute(IJobExecutionContext job)
        {
            try
            {
                await ProcessMovies();
                await ProcessTv();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Exception thrown in Plex availability checker");
            }
        }

        private Task ProcessTv()
        {
            var tv = _tvRepo.GetChild().Where(x => !x.Available).AsNoTracking();
            return ProcessTv(tv);
        }

        private async Task ProcessTv(IQueryable<ChildRequests> tv)
        {
            var plexEpisodes = _repo.GetAllEpisodes().Include(x => x.Series).AsNoTracking();

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
                IQueryable<PlexEpisode> seriesEpisodes = null;
                if (useImdb)
                {
                    seriesEpisodes = plexEpisodes.Where(x => x.Series.ImdbId == imdbId.ToString());
                }
                if (useTvDb && (seriesEpisodes == null || !seriesEpisodes.Any()))
                {
                    seriesEpisodes = plexEpisodes.Where(x => x.Series.TvDbId == tvDbId.ToString());
                }

                if (seriesEpisodes == null)
                {
                    continue;
                }

                if (!seriesEpisodes.Any())
                {
                    // Let's try and match the series by name
                    seriesEpisodes = plexEpisodes.Where(x =>
                        x.Series.Title.Equals(child.Title, StringComparison.InvariantCultureIgnoreCase) &&
                        x.Series.ReleaseYear.Equals(child.ParentRequest.ReleaseDate.Year.ToString(), StringComparison.InvariantCultureIgnoreCase));

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
                                Id = episode.Id
                            });
                            episode.Available = true;
                        }
                    }
                }

                //TODO Partial avilability notifications here
                foreach(var c in availableEpisode)
                {
                    await _tvRepo.MarkEpisodeAsAvailable(c.Id);
                }
                
                // Check to see if all of the episodes in all seasons are available for this request
                var allAvailable = child.SeasonRequests.All(x => x.Episodes.All(c => c.Available));
                if (allAvailable)
                {
                    _log.LogInformation("[PAC] - Child request {0} is now available, sending notification", $"{child.Title} - {child.Id}");
                    // We have ful-fulled this request!
                    await _tvRepo.MarkChildAsAvailable(child.Id);
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

        private async Task ProcessMovies()
        {
            // Get all non available
            var movies = _movieRepo.GetAll().Include(x => x.RequestedUser).Where(x => !x.Available).AsNoTracking();
            var itemsForAvailbility = new List<AvailabilityModel>();

            foreach (var movie in movies)
            {
                if (movie.Available)
                {
                    return;
                }

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
                
                _log.LogInformation("[PAC] - Movie request {0} is now available, sending notification", $"{movie.Title} - {movie.Id}");
                itemsForAvailbility.Add(new AvailabilityModel
                {
                    Id = movie.Id,
                    RequestedUser = movie.RequestedUser != null ? movie.RequestedUser.Email : string.Empty
                });
            }

            foreach (var i in itemsForAvailbility)
            {
                await _movieRepo.MarkAsAvailable(i.Id);

                await _notificationService.Notify(new NotificationOptions
                {
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.RequestAvailable,
                    RequestId = i.Id,
                    RequestType = RequestType.Movie,
                    Recipient = i.RequestedUser
                });
            }

            await _repo.SaveChangesAsync();
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