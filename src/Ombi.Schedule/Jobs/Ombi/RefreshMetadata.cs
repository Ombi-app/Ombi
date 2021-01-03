using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.Jellyfin;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Api.TvMaze;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Jellyfin;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class RefreshMetadata : IRefreshMetadata
    {
        public RefreshMetadata(IPlexContentRepository plexRepo, IEmbyContentRepository embyRepo, IJellyfinContentRepository jellyfinRepo,
            ILogger<RefreshMetadata> log, ITvMazeApi tvApi, ISettingsService<PlexSettings> plexSettings,
            IMovieDbApi movieApi,
            ISettingsService<EmbySettings> embySettings, IEmbyApiFactory embyApi,
            ISettingsService<JellyfinSettings> jellyfinSettings, IJellyfinApiFactory jellyfinApi,
            IHubContext<NotificationHub> notification)
        {
            _plexRepo = plexRepo;
            _embyRepo = embyRepo;
            _jellyfinRepo = jellyfinRepo;
            _log = log;
            _movieApi = movieApi;
            _tvApi = tvApi;
            _plexSettings = plexSettings;
            _embySettings = embySettings;
            _embyApiFactory = embyApi;
            _jellyfinSettings = jellyfinSettings;
            _jellyfinApiFactory = jellyfinApi;
            _notification = notification;
        }

        private readonly IPlexContentRepository _plexRepo;
        private readonly IEmbyContentRepository _embyRepo;
        private readonly IJellyfinContentRepository _jellyfinRepo;
        private readonly ILogger _log;
        private readonly IMovieDbApi _movieApi;
        private readonly ITvMazeApi _tvApi;
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly ISettingsService<EmbySettings> _embySettings;
        private readonly ISettingsService<JellyfinSettings> _jellyfinSettings;
        private readonly IEmbyApiFactory _embyApiFactory;
        private readonly IJellyfinApiFactory _jellyfinApiFactory;
        private readonly IHubContext<NotificationHub> _notification;
        private IEmbyApi EmbyApi { get; set; }
        private IJellyfinApi JellyfinApi { get; set; }

        public async Task Execute(IJobExecutionContext job)
        {
            _log.LogInformation("Starting the Metadata refresh");

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Metadata Refresh Started");
            try
            {
                var settings = await _plexSettings.GetSettingsAsync();
                if (settings.Enable)
                {
                    await StartPlex();

                    await OmbiQuartz.TriggerJob(nameof(IPlexAvailabilityChecker), "Plex");
                }

                var embySettings = await _embySettings.GetSettingsAsync();
                if (embySettings.Enable)
                {
                    await StartEmby(embySettings);

                    await OmbiQuartz.TriggerJob(nameof(IEmbyAvaliabilityChecker), "Emby");
                }

                var jellyfinSettings = await _jellyfinSettings.GetSettingsAsync();
                if (jellyfinSettings.Enable)
                {
                    await StartJellyfin(jellyfinSettings);

                    await OmbiQuartz.TriggerJob(nameof(IJellyfinAvaliabilityChecker), "Jellyfin");
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, $"Exception when refreshing the Metadata Refresh");

                await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                    .SendAsync(NotificationHub.NotificationEvent, "Metadata Refresh Failed");
                return;
            }

            _log.LogInformation("Metadata refresh finished");
            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Metadata Refresh Finished");
        }

        private async Task StartPlex()
        {
            // Ensure we check that we have not linked this item to a request
            var allMovies = await _plexRepo.GetAll().Where(x =>
               x.Type == PlexMediaTypeEntity.Movie && x.RequestId == null && (x.TheMovieDbId == null || x.ImdbId == null)).ToListAsync();
            await StartPlexMovies(allMovies);

            // Now Tv
            var allTv = await _plexRepo.GetAll().Where(x =>
                x.Type == PlexMediaTypeEntity.Show && x.RequestId == null && (x.TheMovieDbId == null || x.ImdbId == null || x.TvDbId == null)).ToListAsync();
            await StartPlexTv(allTv);
        }

        private async Task StartEmby(EmbySettings s)
        {
            EmbyApi = _embyApiFactory.CreateClient(s);
            await StartEmbyMovies(s);
            await StartEmbyTv();
        }

        private async Task StartJellyfin(JellyfinSettings s)
        {
            JellyfinApi = _jellyfinApiFactory.CreateClient(s);
            await StartJellyfinMovies(s);
            await StartJellyfinTv();
        }

        private async Task StartPlexTv(List<PlexServerContent> allTv)
        {
            foreach (var show in allTv)
            {
                // Just double check there is no associated request id
                if (show.RequestId.HasValue)
                {
                    continue;
                }
                var hasImdb = show.ImdbId.HasValue();
                var hasTheMovieDb = show.TheMovieDbId.HasValue();
                var hasTvDbId = show.TvDbId.HasValue();

                if (!hasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(hasTvDbId, hasImdb, show.TvDbId, show.ImdbId, show.Title, false);
                    show.TheMovieDbId = id;
                }

                if (!hasImdb)
                {
                    var id = await GetImdbId(hasTheMovieDb, hasTvDbId, show.Title, show.TheMovieDbId, show.TvDbId, RequestType.TvShow);
                    show.ImdbId = id;
                    _plexRepo.UpdateWithoutSave(show);
                }

                if (!hasTvDbId)
                {
                    var id = await GetTvDbId(hasTheMovieDb, hasImdb, show.TheMovieDbId, show.ImdbId, show.Title);
                    show.TvDbId = id;
                    _plexRepo.UpdateWithoutSave(show);
                }
                await _plexRepo.SaveChangesAsync();
            }
        }

        private async Task StartEmbyTv()
        {
            var allTv = await _embyRepo.GetAll().Where(x =>
                x.Type == EmbyMediaType.Series && (x.TheMovieDbId == null || x.ImdbId == null || x.TvDbId == null)).ToListAsync();

            foreach (var show in allTv)
            {
                var hasImdb = show.ImdbId.HasValue();
                var hasTheMovieDb = show.TheMovieDbId.HasValue();
                var hasTvDbId = show.TvDbId.HasValue();

                if (!hasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(hasTvDbId, hasImdb, show.TvDbId, show.ImdbId, show.Title, false);
                    show.TheMovieDbId = id;
                }

                if (!hasImdb)
                {
                    var id = await GetImdbId(hasTheMovieDb, hasTvDbId, show.Title, show.TheMovieDbId, show.TvDbId, RequestType.TvShow);
                    show.ImdbId = id;
                    _embyRepo.UpdateWithoutSave(show);
                }

                if (!hasTvDbId)
                {
                    var id = await GetTvDbId(hasTheMovieDb, hasImdb, show.TheMovieDbId, show.ImdbId, show.Title);
                    show.TvDbId = id;
                    _embyRepo.UpdateWithoutSave(show);
                }

                await _embyRepo.SaveChangesAsync();
            }
        }

        private async Task StartJellyfinTv()
        {
            var allTv = await _jellyfinRepo.GetAll().Where(x =>
                x.Type == JellyfinMediaType.Series && (x.TheMovieDbId == null || x.ImdbId == null || x.TvDbId == null)).ToListAsync();

            foreach (var show in allTv)
            {
                var hasImdb = show.ImdbId.HasValue();
                var hasTheMovieDb = show.TheMovieDbId.HasValue();
                var hasTvDbId = show.TvDbId.HasValue();

                if (!hasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(hasTvDbId, hasImdb, show.TvDbId, show.ImdbId, show.Title, false);
                    show.TheMovieDbId = id;
                }

                if (!hasImdb)
                {
                    var id = await GetImdbId(hasTheMovieDb, hasTvDbId, show.Title, show.TheMovieDbId, show.TvDbId, RequestType.TvShow);
                    show.ImdbId = id;
                    _jellyfinRepo.UpdateWithoutSave(show);
                }

                if (!hasTvDbId)
                {
                    var id = await GetTvDbId(hasTheMovieDb, hasImdb, show.TheMovieDbId, show.ImdbId, show.Title);
                    show.TvDbId = id;
                    _jellyfinRepo.UpdateWithoutSave(show);
                }

                await _jellyfinRepo.SaveChangesAsync();
            }
        }

        private async Task StartPlexMovies(List<PlexServerContent> allMovies)
        {
            foreach (var movie in allMovies)
            {
                // Just double check there is no associated request id
                if (movie.RequestId.HasValue)
                {
                    continue;
                }
                var hasImdb = movie.ImdbId.HasValue();
                var hasTheMovieDb = movie.TheMovieDbId.HasValue();
                // Movies don't really use TheTvDb

                if (!hasImdb)
                {
                    var imdbId = await GetImdbId(hasTheMovieDb, false, movie.Title, movie.TheMovieDbId, string.Empty, RequestType.Movie);
                    movie.ImdbId = imdbId;
                    _plexRepo.UpdateWithoutSave(movie);
                }
                if (!hasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(false, hasImdb, string.Empty, movie.ImdbId, movie.Title, true);
                    movie.TheMovieDbId = id;
                    _plexRepo.UpdateWithoutSave(movie);
                }

                await _plexRepo.SaveChangesAsync();
            }
        }

        private async Task StartEmbyMovies(EmbySettings settings)
        {
            var allMovies = await _embyRepo.GetAll().Where(x =>
                x.Type == EmbyMediaType.Movie && (x.TheMovieDbId == null || x.ImdbId == null)).ToListAsync();
            foreach (var movie in allMovies)
            {
                movie.ImdbId.HasValue();
                movie.TheMovieDbId.HasValue();
                // Movies don't really use TheTvDb

                // Check if it even has 1 ID
                if (!movie.HasImdb && !movie.HasTheMovieDb)
                {
                    // Ok this sucks,
                    // The only think I can think that has happened is that we scanned Emby before Emby has got the metadata
                    // So let's recheck emby to see if they have got the metadata now
                    //
                    // Yeah your right that does suck - Future Jamie
                    _log.LogInformation($"Movie {movie.Title} does not have a ImdbId or TheMovieDbId, so rechecking emby");
                    foreach (var server in settings.Servers)
                    {
                        _log.LogInformation($"Checking server {server.Name} for upto date metadata");
                        var movieInfo = await EmbyApi.GetMovieInformation(movie.EmbyId, server.ApiKey, server.AdministratorId,
                            server.FullUri);

                        if (movieInfo.ProviderIds?.Imdb.HasValue() ?? false)
                        {
                            movie.ImdbId = movieInfo.ProviderIds.Imdb;
                        }

                        if (movieInfo.ProviderIds?.Tmdb.HasValue() ?? false)
                        {
                            movie.TheMovieDbId = movieInfo.ProviderIds.Tmdb;
                        }
                    }
                }

                if (!movie.HasImdb)
                {
                    var imdbId = await GetImdbId(movie.HasTheMovieDb, false, movie.Title, movie.TheMovieDbId, string.Empty, RequestType.Movie);
                    movie.ImdbId = imdbId;
                    _embyRepo.UpdateWithoutSave(movie);
                }
                if (!movie.HasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(false, movie.HasImdb, string.Empty, movie.ImdbId, movie.Title, true);
                    movie.TheMovieDbId = id;
                    _embyRepo.UpdateWithoutSave(movie);
                }

                await _embyRepo.SaveChangesAsync();

            }
        }

        private async Task StartJellyfinMovies(JellyfinSettings settings)
        {
            var allMovies = await _jellyfinRepo.GetAll().Where(x =>
                x.Type == JellyfinMediaType.Movie && (x.TheMovieDbId == null || x.ImdbId == null)).ToListAsync();
            foreach (var movie in allMovies)
            {
                movie.ImdbId.HasValue();
                movie.TheMovieDbId.HasValue();
                // Movies don't really use TheTvDb

                // Check if it even has 1 ID
                if (!movie.HasImdb && !movie.HasTheMovieDb)
                {
                    // Ok this sucks,
                    // The only think I can think that has happened is that we scanned Jellyfin before Jellyfin has got the metadata
                    // So let's recheck jellyfin to see if they have got the metadata now
                    //
                    // Yeah your right that does suck - Future Jamie
                    _log.LogInformation($"Movie {movie.Title} does not have a ImdbId or TheMovieDbId, so rechecking jellyfin");
                    foreach (var server in settings.Servers)
                    {
                        _log.LogInformation($"Checking server {server.Name} for upto date metadata");
                        var movieInfo = await JellyfinApi.GetMovieInformation(movie.JellyfinId, server.ApiKey, server.AdministratorId,
                            server.FullUri);

                        if (movieInfo.ProviderIds?.Imdb.HasValue() ?? false)
                        {
                            movie.ImdbId = movieInfo.ProviderIds.Imdb;
                        }

                        if (movieInfo.ProviderIds?.Tmdb.HasValue() ?? false)
                        {
                            movie.TheMovieDbId = movieInfo.ProviderIds.Tmdb;
                        }
                    }
                }

                if (!movie.HasImdb)
                {
                    var imdbId = await GetImdbId(movie.HasTheMovieDb, false, movie.Title, movie.TheMovieDbId, string.Empty, RequestType.Movie);
                    movie.ImdbId = imdbId;
                    _jellyfinRepo.UpdateWithoutSave(movie);
                }
                if (!movie.HasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(false, movie.HasImdb, string.Empty, movie.ImdbId, movie.Title, true);
                    movie.TheMovieDbId = id;
                    _jellyfinRepo.UpdateWithoutSave(movie);
                }

                await _jellyfinRepo.SaveChangesAsync();

            }
        }

        public async Task<string> GetTheMovieDbId(bool hasTvDbId, bool hasImdb, string tvdbID, string imdbId, string title, bool movie)
        {
            _log.LogInformation("The Media item {0} does not have a TheMovieDbId, searching for TheMovieDbId", title);
            FindResult result = null;
            var hasResult = false;
            if (hasTvDbId)
            {
                result = await _movieApi.Find(tvdbID, ExternalSource.tvdb_id);
                hasResult = result?.tv_results?.Length > 0;

                _log.LogInformation("Setting Show {0} because we have TvDbId, result: {1}", title, hasResult);
            }
            if (hasImdb && !hasResult)
            {
                result = await _movieApi.Find(imdbId, ExternalSource.imdb_id);
                if (movie)
                {
                    hasResult = result?.movie_results?.Length > 0;
                }
                else
                {
                    hasResult = result?.tv_results?.Length > 0;

                }

                _log.LogInformation("Setting Show {0} because we have ImdbId, result: {1}", title, hasResult);
            }
            if (hasResult)
            {
                if (movie)
                {
                    return result.movie_results?[0]?.id.ToString() ?? string.Empty;
                }
                else
                {

                    return result.tv_results?[0]?.id.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private async Task<string> GetImdbId(bool hasTheMovieDb, bool hasTvDbId, string title, string theMovieDbId, string tvDbId, RequestType type)
        {
            _log.LogInformation("The media item {0} does not have a ImdbId, searching for ImdbId", title);
            // Looks like TV Maze does not provide the moviedb id, neither does the TV endpoint on TheMovieDb
            if (hasTheMovieDb)
            {
                _log.LogInformation("The show {0} has TheMovieDbId but not ImdbId, searching for ImdbId", title);
                if (int.TryParse(theMovieDbId, out var id))
                {
                    switch (type)
                    {
                        case RequestType.TvShow:
                            var result = await _movieApi.GetTvExternals(id);
                            return result.imdb_id;
                        case RequestType.Movie:
                            var r = await _movieApi.GetMovieInformationWithExtraInfo(id);
                            return r.ImdbId;
                        default:
                            break;
                    }
                    
                }
            }

            if (hasTvDbId && type == RequestType.TvShow)
            {
                _log.LogInformation("The show {0} has tvdbid but not ImdbId, searching for ImdbId", title);
                if (int.TryParse(tvDbId, out var id))
                {
                    var result = await _tvApi.ShowLookupByTheTvDbId(id);
                    return result?.externals?.imdb;
                }
            }
            return string.Empty;
        }


        private async Task<string> GetTvDbId(bool hasTheMovieDb, bool hasImdb, string theMovieDbId, string imdbId, string title)
        {
            _log.LogInformation("The media item {0} does not have a TvDbId, searching for TvDbId", title);
            if (hasTheMovieDb)
            {
                _log.LogInformation("The show {0} has theMovieDBId but not ImdbId, searching for ImdbId", title);
                if (int.TryParse(theMovieDbId, out var id))
                {
                    var result = await _movieApi.GetTvExternals(id);

                    return result.tvdb_id.ToString();
                }
            }

            if (hasImdb)
            {
                _log.LogInformation("The show {0} has ImdbId but not ImdbId, searching for ImdbId", title);
                var result = await _movieApi.Find(imdbId, ExternalSource.imdb_id);
                if (result?.tv_results?.Length > 0)
                {
                    var movieId = result.tv_results?[0]?.id ?? 0;

                    var externalResult = await _movieApi.GetTvExternals(movieId);

                    return externalResult.imdb_id;
                }
            }
            return string.Empty;
        }


        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //_plexSettings?.Dispose();
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
