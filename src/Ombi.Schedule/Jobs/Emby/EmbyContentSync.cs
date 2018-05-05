using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models.Movie;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Serilog;
using EmbyMediaType = Ombi.Store.Entities.EmbyMediaType;

namespace Ombi.Schedule.Jobs.Emby
{
    public class EmbyContentSync : IEmbyContentSync
    {
        public EmbyContentSync(ISettingsService<EmbySettings> settings, IEmbyApi api, ILogger<EmbyContentSync> logger,
            IEmbyContentRepository repo, IEmbyEpisodeSync epSync, IRefreshMetadata metadata)
        {
            _logger = logger;
            _settings = settings;
            _api = api;
            _repo = repo;
            _episodeSync = epSync;
            _metadata = metadata;
            _settings.ClearCache();
        }

        private readonly ILogger<EmbyContentSync> _logger;
        private readonly ISettingsService<EmbySettings> _settings;
        private readonly IEmbyApi _api;
        private readonly IEmbyContentRepository _repo;
        private readonly IEmbyEpisodeSync _episodeSync;
        private readonly IRefreshMetadata _metadata;


        public async Task Start()
        {
            var embySettings = await _settings.GetSettingsAsync();
            if (!embySettings.Enable)
                return;

            foreach (var server in embySettings.Servers)
            {
                try
                {
                    await StartServerCache(server);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception when caching Emby for server {0}", server.Name);
                }
            }

            // Episodes
            BackgroundJob.Enqueue(() => _episodeSync.Start());
            BackgroundJob.Enqueue(() => _metadata.Start());
        }


        private async Task StartServerCache(EmbyServers server)
        {
            if (!ValidateSettings(server))
                return;

            await _repo.ExecuteSql("DELETE FROM EmbyEpisode");
            await _repo.ExecuteSql("DELETE FROM EmbyContent");

            var movies = await _api.GetAllMovies(server.ApiKey, server.AdministratorId, server.FullUri);
            var mediaToAdd = new HashSet<EmbyContent>();
            foreach (var movie in movies.Items)
            {
                if (movie.Type.Equals("boxset", StringComparison.CurrentCultureIgnoreCase))
                {
                    var movieInfo =
                        await _api.GetCollection(movie.Id, server.ApiKey, server.AdministratorId, server.FullUri);
                    foreach (var item in movieInfo.Items)
                    {
                        var info = await _api.GetMovieInformation(item.Id, server.ApiKey,
                            server.AdministratorId, server.FullUri);
                        await ProcessMovies(info, mediaToAdd);
                    }
                }
                else
                {
                    // Regular movie
                    var movieInfo = await _api.GetMovieInformation(movie.Id, server.ApiKey,
                        server.AdministratorId, server.FullUri);

                    await ProcessMovies(movieInfo, mediaToAdd);
                }
            }
            // TV Time
            var tv = await _api.GetAllShows(server.ApiKey, server.AdministratorId, server.FullUri);

            foreach (var tvShow in tv.Items)
            {
                var tvInfo = await _api.GetSeriesInformation(tvShow.Id, server.ApiKey, server.AdministratorId,
                    server.FullUri);
                if (string.IsNullOrEmpty(tvInfo.ProviderIds?.Tvdb))
                {
                    Log.Error("Provider Id on tv {0} is null", tvShow.Name);
                    continue;
                }

                var existingTv = await _repo.GetByEmbyId(tvShow.Id);
                if (existingTv == null)
                    mediaToAdd.Add(new EmbyContent
                    {
                        TvDbId = tvInfo.ProviderIds?.Tvdb,
                        ImdbId = tvInfo.ProviderIds?.Imdb,
                        TheMovieDbId = tvInfo.ProviderIds?.Tmdb,
                        Title = tvInfo.Name,
                        Type = EmbyMediaType.Series,
                        EmbyId = tvShow.Id,
                        Url = EmbyHelper.GetEmbyMediaUrl(tvShow.Id),
                        AddedAt = DateTime.UtcNow
                    });
            }

            if (mediaToAdd.Any())
                await _repo.AddRange(mediaToAdd);
        }

        private async Task ProcessMovies(MovieInformation movieInfo, ICollection<EmbyContent> content)
        {
            // Check if it exists
            var existingMovie = await _repo.GetByEmbyId(movieInfo.Id);

            if (existingMovie == null)
                content.Add(new EmbyContent
                {
                    ImdbId = movieInfo.ProviderIds.Imdb,
                    TheMovieDbId = movieInfo.ProviderIds?.Tmdb,
                    Title = movieInfo.Name,
                    Type = EmbyMediaType.Movie,
                    EmbyId = movieInfo.Id,
                    Url = EmbyHelper.GetEmbyMediaUrl(movieInfo.Id),
                    AddedAt = DateTime.UtcNow,
                });
        }

        private bool ValidateSettings(EmbyServers server)
        {
            if (server?.Ip == null || string.IsNullOrEmpty(server?.ApiKey))
            {
                _logger.LogInformation(LoggingEvents.EmbyContentCacher, $"Server {server?.Name} is not configured correctly");
                return false;
            }

            return true;
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _settings?.Dispose();
                _repo?.Dispose();
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