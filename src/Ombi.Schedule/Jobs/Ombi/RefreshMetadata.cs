using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Api.TvMaze;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class RefreshMetadata : IRefreshMetadata
    {
        public RefreshMetadata(IPlexContentRepository plexRepo, IEmbyContentRepository embyRepo,
            ILogger<RefreshMetadata> log, ITvMazeApi tvApi, ISettingsService<PlexSettings> plexSettings,
            IMovieDbApi movieApi, ISettingsService<EmbySettings> embySettings, IPlexAvailabilityChecker plexAvailability, IEmbyAvaliabilityChecker embyAvaliability,
            IEmbyApi embyApi)
        {
            _plexRepo = plexRepo;
            _embyRepo = embyRepo;
            _log = log;
            _movieApi = movieApi;
            _tvApi = tvApi;
            _plexSettings = plexSettings;
            _embySettings = embySettings;
            _plexAvailabilityChecker = plexAvailability;
            _embyAvaliabilityChecker = embyAvaliability;
            _embyApi = embyApi;
        }

        private readonly IPlexContentRepository _plexRepo;
        private readonly IEmbyContentRepository _embyRepo;
        private readonly IPlexAvailabilityChecker _plexAvailabilityChecker;
        private readonly IEmbyAvaliabilityChecker _embyAvaliabilityChecker;
        private readonly ILogger _log;
        private readonly IMovieDbApi _movieApi;
        private readonly ITvMazeApi _tvApi;
        private readonly ISettingsService<PlexSettings> _plexSettings;
        private readonly ISettingsService<EmbySettings> _embySettings;
        private readonly IEmbyApi _embyApi;

        public async Task Start()
        {
            _log.LogInformation("Starting the Metadata refresh");
            try
            {
                var settings = await _plexSettings.GetSettingsAsync();
                if (settings.Enable)
                {
                    await StartPlex();
                }
                
                var embySettings = await _embySettings.GetSettingsAsync();
                if (embySettings.Enable)
                {
                    await StartEmby(embySettings);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Exception when refreshing the Plex Metadata");
                throw;
            }
        }

        public async Task ProcessPlexServerContent(IEnumerable<int> contentIds)
        {
            _log.LogInformation("Starting the Metadata refresh from RecentlyAddedSync");
            var plexSettings = await _plexSettings.GetSettingsAsync();
            var embySettings = await _embySettings.GetSettingsAsync();
            try
            {
                if (plexSettings.Enable)
                {
                    await StartPlexWithKnownContent(contentIds);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Exception when refreshing the Plex Metadata");
                throw;
            }
            finally
            {
                if (plexSettings.Enable)
                {
                    BackgroundJob.Enqueue(() => _plexAvailabilityChecker.Start());
                }

                if (embySettings.Enable)
                {
                    BackgroundJob.Enqueue(() => _embyAvaliabilityChecker.Start());

                }
            }
        }

        private async Task StartPlexWithKnownContent(IEnumerable<int> contentids)
        {
            var everything = _plexRepo.GetAll().Where(x => contentids.Contains(x.Id));
            var allMovies = everything.Where(x => x.Type == PlexMediaTypeEntity.Movie);
            await StartPlexMovies(allMovies);

            // Now Tv
            var allTv = everything.Where(x => x.Type == PlexMediaTypeEntity.Show);
            await StartPlexTv(allTv);
        }

        private async Task StartPlex()
        {
            var allMovies = _plexRepo.GetAll().Where(x =>
                x.Type == PlexMediaTypeEntity.Movie && (!x.TheMovieDbId.HasValue() || !x.ImdbId.HasValue()));
            await StartPlexMovies(allMovies);

            // Now Tv
            var allTv = _plexRepo.GetAll().Where(x =>
                x.Type == PlexMediaTypeEntity.Show && (!x.TheMovieDbId.HasValue() || !x.ImdbId.HasValue() || !x.TvDbId.HasValue()));
            await StartPlexTv(allTv);
        }

        private async Task StartEmby(EmbySettings s)
        {
            await StartEmbyMovies(s);
            await StartEmbyTv();
        }

        private async Task StartPlexTv(IQueryable<PlexServerContent> allTv)
        {
            var tvCount = 0;
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
                    var id = await GetImdbId(hasTheMovieDb, hasTvDbId, show.Title, show.TheMovieDbId, show.TvDbId);
                    show.ImdbId = id;
                    _plexRepo.UpdateWithoutSave(show);
                }

                if (!hasTvDbId)
                {
                    var id = await GetTvDbId(hasTheMovieDb, hasImdb, show.TheMovieDbId, show.ImdbId, show.Title);
                    show.TvDbId = id;
                    _plexRepo.UpdateWithoutSave(show);
                }
                tvCount++;
                if (tvCount >= 75)
                {
                    await _plexRepo.SaveChangesAsync();
                    tvCount = 0;
                }
            }
            await _plexRepo.SaveChangesAsync();
        }

        private async Task StartEmbyTv()
        {
            var allTv = _embyRepo.GetAll().Where(x =>
                x.Type == EmbyMediaType.Series && (!x.TheMovieDbId.HasValue() || !x.ImdbId.HasValue() || !x.TvDbId.HasValue()));
            var tvCount = 0;
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
                    var id = await GetImdbId(hasTheMovieDb, hasTvDbId, show.Title, show.TheMovieDbId, show.TvDbId);
                    show.ImdbId = id;
                    _embyRepo.UpdateWithoutSave(show);
                }

                if (!hasTvDbId)
                {
                    var id = await GetTvDbId(hasTheMovieDb, hasImdb, show.TheMovieDbId, show.ImdbId, show.Title);
                    show.TvDbId = id;
                    _embyRepo.UpdateWithoutSave(show);
                }
                tvCount++;
                if (tvCount >= 75)
                {
                    await _embyRepo.SaveChangesAsync();
                    tvCount = 0;
                }
            }
            await _embyRepo.SaveChangesAsync();
        }

        private async Task StartPlexMovies(IQueryable<PlexServerContent> allMovies)
        {
            int movieCount = 0;
            foreach (var movie in allMovies)
            {
                var hasImdb = movie.ImdbId.HasValue();
                var hasTheMovieDb = movie.TheMovieDbId.HasValue();
                // Movies don't really use TheTvDb

                if (!hasImdb)
                {
                    var imdbId = await GetImdbId(hasTheMovieDb, false, movie.Title, movie.TheMovieDbId, string.Empty);
                    movie.ImdbId = imdbId;
                    _plexRepo.UpdateWithoutSave(movie);
                }
                if (!hasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(false, hasImdb, string.Empty, movie.ImdbId, movie.Title, true);
                    movie.TheMovieDbId = id;
                    _plexRepo.UpdateWithoutSave(movie);
                }
                movieCount++;
                if (movieCount >= 75)
                {
                    await _plexRepo.SaveChangesAsync();
                    movieCount = 0;
                }
            }

            await _plexRepo.SaveChangesAsync();
        }

        private async Task StartEmbyMovies(EmbySettings settings)
        {
            var allMovies = _embyRepo.GetAll().Where(x =>
                x.Type == EmbyMediaType.Movie && (!x.TheMovieDbId.HasValue() || !x.ImdbId.HasValue()));
            int movieCount = 0;
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
                    _log.LogInformation($"Movie {movie.Title} does not have a ImdbId or TheMovieDbId, so rechecking emby");
                    foreach (var server in settings.Servers)
                    {
                        _log.LogInformation($"Checking server {server.Name} for upto date metadata");
                        var movieInfo = await _embyApi.GetMovieInformation(movie.EmbyId, server.ApiKey, server.AdministratorId,
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
                    var imdbId = await GetImdbId(movie.HasTheMovieDb, false, movie.Title, movie.TheMovieDbId, string.Empty);
                    movie.ImdbId = imdbId;
                    _embyRepo.UpdateWithoutSave(movie);
                }
                if (!movie.HasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(false, movie.HasImdb, string.Empty, movie.ImdbId, movie.Title, true);
                    movie.TheMovieDbId = id;
                    _embyRepo.UpdateWithoutSave(movie);
                }
                movieCount++;
                if (movieCount >= 75)
                {
                    await _embyRepo.SaveChangesAsync();
                    movieCount = 0;
                }
            }

            await _embyRepo.SaveChangesAsync();
        }

        private async Task<string> GetTheMovieDbId(bool hasTvDbId, bool hasImdb, string tvdbID, string imdbId, string title, bool movie)
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

        private async Task<string> GetImdbId(bool hasTheMovieDb, bool hasTvDbId, string title, string theMovieDbId, string tvDbId)
        {
            _log.LogInformation("The media item {0} does not have a ImdbId, searching for ImdbId", title);
            // Looks like TV Maze does not provide the moviedb id, neither does the TV endpoint on TheMovieDb
            if (hasTheMovieDb)
            {
                _log.LogInformation("The show {0} has TheMovieDbId but not ImdbId, searching for ImdbId", title);
                if (int.TryParse(theMovieDbId, out var id))
                {
                    var result = await _movieApi.GetTvExternals(id);

                    return result.imdb_id;
                }
            }

            if (hasTvDbId)
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
                _plexRepo?.Dispose();
                _embyRepo?.Dispose();
                _plexSettings?.Dispose();
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