using System;
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
using Ombi.Store.Repository.Requests;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class RefreshMetadata : IRefreshMetadata
    {
        public RefreshMetadata(IPlexContentRepository plexRepo, IEmbyContentRepository embyRepo,
            ILogger<RefreshMetadata> log, ITvMazeApi tvApi,
            IMovieDbApi movieApi)
        {
            _plexRepo = plexRepo;
            _embyRepo = embyRepo;
            _log = log;
            _movieApi = movieApi;
            _tvApi = tvApi;
        }

        private readonly IPlexContentRepository _plexRepo;
        private readonly IEmbyContentRepository _embyRepo;
        private readonly ILogger _log;
        private readonly IMovieDbApi _movieApi;
        private readonly ITvMazeApi _tvApi;

        public async Task Start()
        {
            _log.LogInformation("Starting the Metadata refresh");
            await StartPlex();
            await StartEmby();
        }

        private async Task StartPlex()
        {
            var allMovies = _plexRepo.GetAll().Where(x => x.Type == PlexMediaTypeEntity.Movie);

            foreach (var movie in allMovies)
            {
                var hasImdb = movie.ImdbId.HasValue();
                var hasTheMovieDb = movie.TheMovieDbId.HasValue();
                // Movies don't really use TheTvDb

                if (!hasImdb && hasTheMovieDb)
                {
                    var imdbId = await GetImdbWithTheMovieDbId("movie", movie.Title, movie.TheMovieDbId);
                    movie.ImdbId = imdbId;
                }
                if (!hasTheMovieDb && hasImdb)
                {
                    _log.LogInformation("The movie {0} has an ImdbId but not TheMovieDbId, searching for TheMovieDbId", movie.Title);
                    var result = await _movieApi.Find(movie.ImdbId, ExternalSource.imdb_id);
                    movie.TheMovieDbId = result.movie_results?[0]?.id.ToString() ?? string.Empty;
                    _log.LogInformation("Setting movie {0} TheMovieDbId to {1}", movie.Title, movie.TheMovieDbId);
                }
            }

            await _plexRepo.UpdateRange(allMovies);

            // Now Tv
            var allTv = _plexRepo.GetAll().Where(x => x.Type == PlexMediaTypeEntity.Show);
            foreach (var show in allTv)
            {
                var hasImdb = show.ImdbId.HasValue();
                var hasTheMovieDb = show.TheMovieDbId.HasValue();
                var hasTvDbId = show.TvDbId.HasValue();

                if (!hasTheMovieDb)
                {
                    _log.LogInformation("The TV Show {0} does not have a TheMovieDbId, searching for TheMovieDbId", show.Title);
                    FindResult result = null;
                    var hasResult = false;
                    if (hasTvDbId)
                    {
                        result = await _movieApi.Find(show.TvDbId, ExternalSource.tvdb_id);
                        hasResult = result != null;

                        _log.LogInformation("Setting Show {0} because we have TvDbId, result: {1}", show.Title, hasResult);
                    }
                    if (hasImdb && !hasResult)
                    {
                        result = await _movieApi.Find(show.ImdbId, ExternalSource.imdb_id);
                        hasResult = result != null;

                        _log.LogInformation("Setting Show {0} because we have ImdbId, result: {1}", show.Title, hasResult);
                    }
                    if (hasResult)
                    {
                        show.TheMovieDbId = result.tv_results?[0]?.id.ToString() ?? string.Empty;
                        _log.LogInformation("Setting Show {0} TheMovieDbId to {1}", show.Title, show.TheMovieDbId);
                    }
                }

                if (!hasImdb)
                {
                    var nowHasValue = false;
                    _log.LogInformation("The TV Show {0} does not have a ImdbId, searching for ImdbId", show.Title);
                    // Looks like TV Maze does not provide the moviedb id, neither does the TV endpoint on TheMovieDb
                    if (hasTheMovieDb)
                    {
                        _log.LogInformation("The show {0} has TheMovieDbId but not ImdbId, searching for ImdbId", show.Title);
                        if (int.TryParse(show.TheMovieDbId, out var id))
                        {
                            var result = await _movieApi.GetTvExternals(id);

                            show.ImdbId = result.imdb_id;
                            _log.LogInformation("Setting show {0} Imdbid to {1}", show.Title, show.ImdbId);
                            nowHasValue = true;
                        }
                    }

                    if (hasTvDbId && !nowHasValue)
                    {
                        _log.LogInformation("The show {0} has tvdbid but not ImdbId, searching for ImdbId", show.Title);
                        if (int.TryParse(show.TvDbId, out var id))
                        {
                            var result = await _tvApi.ShowLookupByTheTvDbId(id);
                            show.ImdbId = result?.externals?.imdb;
                            _log.LogInformation("Setting show {0} ImdbId to {1}", show.Title, show.ImdbId);
                        }
                    }
                }

                if (!hasTvDbId)
                {
                    _log.LogInformation("The TV Show {0} does not have a TvDbId, searching for TvDbId", show.Title);
                    var nowHasValue = false;
                    if (hasTheMovieDb)
                    {
                        _log.LogInformation("The show {0} has theMovieDBId but not ImdbId, searching for ImdbId", show.Title);
                        if (int.TryParse(show.TheMovieDbId, out var id))
                        {
                            var result = await _movieApi.GetTvExternals(id);

                            show.TvDbId = result.tvdb_id.ToString();
                            _log.LogInformation("Setting show {0} TvDbId to {1}", show.Title, show.TvDbId);
                            nowHasValue = true;
                        }
                    }

                    if (hasImdb && !nowHasValue)
                    {

                        _log.LogInformation("The show {0} has ImdbId but not ImdbId, searching for ImdbId", show.Title);
                        var result = await _movieApi.Find(show.ImdbId, ExternalSource.imdb_id);
                        var theMovieDbId = result.tv_results?[0]?.id ?? 0;

                        var externalResult = await _movieApi.GetTvExternals(theMovieDbId);

                        show.TvDbId = externalResult.imdb_id;
                        _log.LogInformation("Setting show {0} ImdbId to {1}", show.Title, show.TvDbId);
                    }
                }
            }

        }

        private async Task<string> GetImdbWithTheMovieDbId(string movieDbId, string title, string type)
        {
            _log.LogInformation("The {0} {1} has TheMovieDb but not ImdbId, searching for ImdbId", type, title);
            if (int.TryParse(movieDbId, out var id))
            {
                var result = await _movieApi.GetMovieInformation(id);
              
                _log.LogInformation("Setting {0} {1} Imdbid to {2}", type, title, result.ImdbId);
                return result.ImdbId;
            }
            return string.Empty;
        }

        private async Task StartEmby()
        {

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