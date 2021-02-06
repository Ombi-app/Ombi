using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models.Movie;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;
using EmbyMediaType = Ombi.Store.Entities.EmbyMediaType;

namespace Ombi.Schedule.Jobs.Emby
{
    public class EmbyContentSync : IEmbyContentSync
    {
        public EmbyContentSync(ISettingsService<EmbySettings> settings, IEmbyApiFactory api, ILogger<EmbyContentSync> logger,
            IEmbyContentRepository repo, IHubContext<NotificationHub> notification)
        {
            _logger = logger;
            _settings = settings;
            _apiFactory = api;
            _repo = repo;
            _notification = notification;
        }

        private readonly ILogger<EmbyContentSync> _logger;
        private readonly ISettingsService<EmbySettings> _settings;
        private readonly IEmbyApiFactory _apiFactory;
        private readonly IEmbyContentRepository _repo;
        private readonly IHubContext<NotificationHub> _notification;
        private IEmbyApi Api { get; set; }

        public async Task Execute(IJobExecutionContext job)
        {
            var embySettings = await _settings.GetSettingsAsync();
            if (!embySettings.Enable)
                return;
            
            Api = _apiFactory.CreateClient(embySettings);

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Emby Content Sync Started");

            foreach (var server in embySettings.Servers)
            {
                try
                {
                    await StartServerCache(server, embySettings);
                }
                catch (Exception e)
                {
                    await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                        .SendAsync(NotificationHub.NotificationEvent, "Emby Content Sync Failed");
                    _logger.LogError(e, "Exception when caching Emby for server {0}", server.Name);
                }
            }

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, "Emby Content Sync Finished");
            // Episodes

            await OmbiQuartz.TriggerJob(nameof(IEmbyEpisodeSync), "Emby");
        }


        private async Task StartServerCache(EmbyServers server, EmbySettings settings)
        {
            if (!ValidateSettings(server))
                return;

            //await _repo.ExecuteSql("DELETE FROM EmbyEpisode");
            //await _repo.ExecuteSql("DELETE FROM EmbyContent");

            var movies = await Api.GetAllMovies(server.ApiKey, 0, 200, server.AdministratorId, server.FullUri);
            var totalCount = movies.TotalRecordCount;
            var processed = 1;

            var mediaToAdd = new HashSet<EmbyContent>();

            while (processed < totalCount)
            {
                foreach (var movie in movies.Items)
                {
                    if (movie.Type.Equals("boxset", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var movieInfo =
                            await Api.GetCollection(movie.Id, server.ApiKey, server.AdministratorId, server.FullUri);
                        foreach (var item in movieInfo.Items)
                        {
                            await ProcessMovies(item, mediaToAdd, server);
                        }

                        processed++;
                    }
                    else
                    {
                        processed++;
                        // Regular movie
                        await ProcessMovies(movie, mediaToAdd, server);
                    }
                }

                // Get the next batch
                movies = await Api.GetAllMovies(server.ApiKey, processed, 200, server.AdministratorId, server.FullUri);
                await _repo.AddRange(mediaToAdd);
                mediaToAdd.Clear();

            }


            // TV Time
            var tv = await Api.GetAllShows(server.ApiKey, 0, 200, server.AdministratorId, server.FullUri);
            var totalTv = tv.TotalRecordCount;
            processed = 1;
            while (processed < totalTv)
            {
                foreach (var tvShow in tv.Items)
                {
                    try
                    {

                        processed++;
                        if (string.IsNullOrEmpty(tvShow.ProviderIds?.Tvdb))
                        {
                            _logger.LogInformation("Provider Id on tv {0} is null", tvShow.Name);
                            continue;
                        }

                        var existingTv = await _repo.GetByEmbyId(tvShow.Id);
                        if (existingTv == null)
                        {
                            _logger.LogDebug("Adding new TV Show {0}", tvShow.Name);
                            mediaToAdd.Add(new EmbyContent
                            {
                                TvDbId = tvShow.ProviderIds?.Tvdb,
                                ImdbId = tvShow.ProviderIds?.Imdb,
                                TheMovieDbId = tvShow.ProviderIds?.Tmdb,
                                Title = tvShow.Name,
                                Type = EmbyMediaType.Series,
                                EmbyId = tvShow.Id,
                                Url = EmbyHelper.GetEmbyMediaUrl(tvShow.Id, server?.ServerId, server.ServerHostname),
                                AddedAt = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            _logger.LogDebug("We already have TV Show {0}", tvShow.Name);
                        }

                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                // Get the next batch
                tv = await Api.GetAllShows(server.ApiKey, processed, 200, server.AdministratorId, server.FullUri);
                await _repo.AddRange(mediaToAdd);
                mediaToAdd.Clear();
            }

            if (mediaToAdd.Any())
                await _repo.AddRange(mediaToAdd);
        }

        private async Task ProcessMovies(EmbyMovie movieInfo, ICollection<EmbyContent> content, EmbyServers server)
        {
            // Check if it exists
            var existingMovie = await _repo.GetByEmbyId(movieInfo.Id);
            var alreadyGoingToAdd = content.Any(x => x.EmbyId == movieInfo.Id);
            if (existingMovie == null && !alreadyGoingToAdd)
            {
                _logger.LogDebug("Adding new movie {0}", movieInfo.Name);
                content.Add(new EmbyContent
                {
                    ImdbId = movieInfo.ProviderIds.Imdb,
                    TheMovieDbId = movieInfo.ProviderIds?.Tmdb,
                    Title = movieInfo.Name,
                    Type = EmbyMediaType.Movie,
                    EmbyId = movieInfo.Id,
                    Url = EmbyHelper.GetEmbyMediaUrl(movieInfo.Id, server?.ServerId, server.ServerHostname),
                    AddedAt = DateTime.UtcNow,
                });
            }
            else
            {
                // we have this
                _logger.LogDebug("We already have movie {0}", movieInfo.Name);
            }
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
                //_settings?.Dispose();
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
