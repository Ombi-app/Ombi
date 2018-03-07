using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class RefreshMetadata : IBaseJob
    {
        public RefreshMetadata(IPlexContentRepository plexRepo, IEmbyContentRepository embyRepo,
            ILogger<RefreshMetadata> log,
            IMovieDbApi movieApi)
        {
            _plexRepo = plexRepo;
            _embyRepo = embyRepo;
            _log = log;
            _movieApi = movieApi;
        }

        private readonly IPlexContentRepository _plexRepo;
        private readonly IEmbyContentRepository _embyRepo;
        private readonly ILogger _log;
        private readonly IMovieDbApi _movieApi;

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
                    if (int.TryParse(movie.TheMovieDbId, out var id))
                    {
                        var result = await _movieApi.GetMovieInformation(id);
                        movie.ImdbId = result.ImdbId;
                    }
                }
                if (!hasTheMovieDb && hasImdb)
                {
                    var result = await _movieApi.Find(movie.ImdbId, ExternalSource.imdb_id);
                    movie.TheMovieDbId = result.movie_results?[0]?.id.ToString() ?? string.Empty;
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
                    FindResult result = null;
                    var hasResult = false;
                    if (hasTvDbId)
                    {
                        result = await _movieApi.Find(show.TvDbId, ExternalSource.tvdb_id);
                        hasResult = result != null;
                    }
                    if (hasImdb && !hasResult)
                    {
                        result = await _movieApi.Find(show.ImdbId, ExternalSource.imdb_id);
                        hasResult = result != null;
                    }
                    if (hasResult)
                    {
                        show.TheMovieDbId = result.tv_results?[0]?.id.ToString() ?? string.Empty;
                    }
                }

                if (!hasImdb)
                {
                    if (hasTheMovieDb)
                    {
                        // We can check here for the ID
                    }
                }
            }

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