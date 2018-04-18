﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Api.TvMaze;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class RefreshMetadata : IRefreshMetadata
    {
        public RefreshMetadata(IPlexContentRepository plexRepo, IEmbyContentRepository embyRepo,
            ILogger<RefreshMetadata> log, ITvMazeApi tvApi, ISettingsService<PlexSettings> plexSettings,
            IMovieDbApi movieApi)
        {
            _plexRepo = plexRepo;
            _embyRepo = embyRepo;
            _log = log;
            _movieApi = movieApi;
            _tvApi = tvApi;
            _plexSettings = plexSettings;
        }

        private readonly IPlexContentRepository _plexRepo;
        private readonly IEmbyContentRepository _embyRepo;
        private readonly ILogger _log;
        private readonly IMovieDbApi _movieApi;
        private readonly ITvMazeApi _tvApi;
        private readonly ISettingsService<PlexSettings> _plexSettings;

        public async Task Start()
        {
            _log.LogInformation("Starting the Metadata refresh");
            try
            {
                var settings = await _plexSettings.GetSettingsAsync();
                if (settings.Enable)
                {
                    await StartPlex();
                    await StartEmby();
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Exception when refreshing the Plex Metadata");
                throw;
            }
        }

        private async Task StartPlex()
        {
            await StartPlexMovies();

            // Now Tv
            await StartPlexTv();
        }

        private async Task StartEmby()
        {
            await StartEmbyMovies();
            await StartEmbyTv();
        }

        private async Task StartPlexTv()
        {
            var allTv = _plexRepo.GetAll().Where(x =>
                x.Type == PlexMediaTypeEntity.Show && (!x.TheMovieDbId.HasValue() || !x.ImdbId.HasValue() || !x.TvDbId.HasValue()));
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
                if (tvCount >= 20)
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
                if (tvCount >= 20)
                {
                    await _embyRepo.SaveChangesAsync();
                    tvCount = 0;
                }
            }
            await _embyRepo.SaveChangesAsync();
        }

        private async Task StartPlexMovies()
        {
            var allMovies = _plexRepo.GetAll().Where(x =>
                x.Type == PlexMediaTypeEntity.Movie && (!x.TheMovieDbId.HasValue() || !x.ImdbId.HasValue()));
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
                if (movieCount >= 20)
                {
                    await _plexRepo.SaveChangesAsync();
                    movieCount = 0;
                }
            }

            await _plexRepo.SaveChangesAsync();
        }

        private async Task StartEmbyMovies()
        {
            var allMovies = _embyRepo.GetAll().Where(x =>
                x.Type == EmbyMediaType.Movie && (!x.TheMovieDbId.HasValue() || !x.ImdbId.HasValue()));
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
                    _embyRepo.UpdateWithoutSave(movie);
                }
                if (!hasTheMovieDb)
                {
                    var id = await GetTheMovieDbId(false, hasImdb, string.Empty, movie.ImdbId, movie.Title, true);
                    movie.TheMovieDbId = id;
                    _embyRepo.UpdateWithoutSave(movie);
                }
                movieCount++;
                if (movieCount >= 20)
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