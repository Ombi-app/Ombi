using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core;
using Ombi.Core.Services;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Quartz;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexAvailabilityChecker : AvailabilityChecker, IPlexAvailabilityChecker
    {
        public PlexAvailabilityChecker(IPlexContentRepository repo, ITvRequestRepository tvRequest, IMovieRequestRepository movies,
            INotificationHelper notification, ILogger<PlexAvailabilityChecker> log, IHubContext<NotificationHub> hub, IFeatureService featureService)
            : base(tvRequest, notification, log, hub)
        {
            _repo = repo;
            _movieRepo = movies;
            _featureService = featureService;
        }

        private readonly IMovieRequestRepository _movieRepo;
        private readonly IPlexContentRepository _repo;
        private readonly IFeatureService _featureService;

        public async Task Execute(IJobExecutionContext job)
        {
            try
            {
                await _hub.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Plex Availability Check Started");
                await ProcessMovies();
                await ProcessTv();
            }
            catch (Exception e)
            {
                await _hub.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Plex Availability Check Failed");
                _log.LogError(e, "Exception thrown in Plex availbility checker");
                return;
            }

            await _hub.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Plex Availability Check Finished");
        }

        private async Task ProcessTv()
        {
            var tv = await _tvRepo.GetChild().Where(x => !x.Available).ToListAsync();
            await ProcessTv(tv);
        }

        private async Task ProcessTv(List<ChildRequests> tv)
        {
            var plexEpisodes = _repo.GetAllEpisodes().Include(x => x.Series);

            foreach (var child in tv)
            {
                var useImdb = false;
                var useTvDb = false;
                var useMovieDb = false;
                if (child.ParentRequest.ImdbId.HasValue())
                {
                    useImdb = true;
                }

                if (child.ParentRequest.TvDbId > 0)
                {
                    useTvDb = true;
                }

                if (child.ParentRequest.ExternalProviderId > 0)
                {
                    useMovieDb = true;
                }

                var tvDbId = child.ParentRequest.TvDbId;
                var imdbId = child.ParentRequest.ImdbId;
                IQueryable<IMediaServerEpisode> seriesEpisodes = null;
                if (useImdb)
                {
                    seriesEpisodes = plexEpisodes.Where(x => x.Series.ImdbId == imdbId.ToString());
                }
                if (useTvDb && (seriesEpisodes == null || !seriesEpisodes.Any()))
                {
                    seriesEpisodes = plexEpisodes.Where(x => x.Series.TvDbId == tvDbId.ToString());
                }
                if (useMovieDb && (seriesEpisodes == null || !seriesEpisodes.Any()))
                {
                    seriesEpisodes = plexEpisodes.Where(x => x.Series.TheMovieDbId == child.ParentRequest.ExternalProviderId.ToString());
                }

                if (seriesEpisodes == null || !seriesEpisodes.Any())
                {
                    // Let's try and match the series by name
                    seriesEpisodes = plexEpisodes.Where(x =>
                        x.Series.Title.Equals(child.Title, StringComparison.InvariantCultureIgnoreCase));
                }

                await ProcessTvShow(seriesEpisodes, child);
            }

            await _tvRepo.Save();
        }

        private async Task ProcessMovies()
        {
            var feature4kEnabled = await _featureService.FeatureEnabled(FeatureNames.Movie4KRequests);
            // Get all non available
            var movies = _movieRepo.GetAll().Include(x => x.RequestedUser).Where(x => !x.Available || (!x.Available4K && x.Has4KRequest));
            var itemsForAvailbility = new List<AvailabilityModel>();

            foreach (var movie in movies)
            {
                var has4kRequest = movie.Has4KRequest;
                PlexServerContent item = null;
                if (movie.ImdbId.HasValue())
                {
                    item = await _repo.Get(movie.ImdbId, ProviderType.ImdbId);
                }
                if (item == null)
                {
                    if (movie.TheMovieDbId.ToString().HasValue())
                    {
                        item = await _repo.Get(movie.TheMovieDbId.ToString(), ProviderType.TheMovieDbId);
                    }
                }
                if (item == null)
                {
                    // We don't yet have this
                    continue;
                }

                _log.LogInformation($"[PAC] - Movie request {movie.Title} - {movie.Id} is now available, sending notification");

                var notify = false;

                if (has4kRequest && item.Has4K && !movie.Available4K && feature4kEnabled)
                {
                    movie.Available4K = true;
                    movie.Approved4K = true;
                    movie.MarkedAsAvailable4K = DateTime.Now;
                    await _movieRepo.SaveChangesAsync();
                    notify = true;
                }

                if (!feature4kEnabled && !movie.Available)
                {
                    movie.Available = true;
                    movie.MarkedAsAvailable = DateTime.Now;
                    await _movieRepo.SaveChangesAsync();
                    notify = true;
                }

                // If we have a non-4k versison then mark as available
                if (item.Quality != null && !movie.Available)
                {
                    movie.Available = true;
                    movie.Approved = true;
                    movie.MarkedAsAvailable = DateTime.Now;
                    await _movieRepo.SaveChangesAsync();
                    notify = true;
                }

                if (notify)
                {
                    itemsForAvailbility.Add(new AvailabilityModel
                    {
                        Id = movie.Id,
                        RequestedUser = movie.RequestedUser != null ? movie.RequestedUser.Email : string.Empty
                    });
                }
            }

            foreach (var i in itemsForAvailbility.DistinctBy(x => x.Id))
            {
                await _notificationService.Notify(new NotificationOptions
                {
                    DateTime = DateTime.Now,
                    NotificationType = NotificationType.RequestAvailable,
                    RequestId = i.Id,
                    RequestType = RequestType.Movie,
                    Recipient = i.RequestedUser
                });
            }
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