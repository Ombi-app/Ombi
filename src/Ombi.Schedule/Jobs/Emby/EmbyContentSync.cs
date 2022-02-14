using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models;
using Ombi.Api.Emby.Models.Media.Tv;
using Ombi.Api.Emby.Models.Movie;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;
using MediaType = Ombi.Store.Entities.MediaType;

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

        private const int AmountToTake = 100;

        private IEmbyApi Api { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var recentlyAddedSearch = false;
            if (dataMap.TryGetValue(JobDataKeys.EmbyRecentlyAddedSearch, out var recentlyAddedObj))
            {
                recentlyAddedSearch = Convert.ToBoolean(recentlyAddedObj);
            }

            var embySettings = await _settings.GetSettingsAsync();
            if (!embySettings.Enable)
                return;

            Api = _apiFactory.CreateClient(embySettings);

            await _notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, recentlyAddedSearch ? "Emby Recently Added Started" : "Emby Content Sync Started");

            foreach (var server in embySettings.Servers)
            {
                try
                {
                    await StartServerCache(server, recentlyAddedSearch);
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


            await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IEmbyEpisodeSync), "Emby"), new JobDataMap(new Dictionary<string, string> { { JobDataKeys.EmbyRecentlyAddedSearch, recentlyAddedSearch.ToString() } }));
        }


        private async Task StartServerCache(EmbyServers server, bool recentlyAdded)
        {
            if (!ValidateSettings(server))
            {
                return;
            }


            if (server.EmbySelectedLibraries.Any() && server.EmbySelectedLibraries.Any(x => x.Enabled))
            {
                var movieLibsToFilter = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType == "movies");

                foreach (var movieParentIdFilder in movieLibsToFilter)
                {
                    _logger.LogInformation($"Scanning Lib '{movieParentIdFilder.Title}'");
                    await ProcessMovies(server, recentlyAdded, movieParentIdFilder.Key);
                }

                var tvLibsToFilter = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType == "tvshows");
                foreach (var tvParentIdFilter in tvLibsToFilter)
                {
                    _logger.LogInformation($"Scanning Lib '{tvParentIdFilter.Title}'");
                    await ProcessTv(server, recentlyAdded, tvParentIdFilter.Key);
                }


                var mixedLibs = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType == "mixed");
                foreach (var m in mixedLibs)
                {
                    _logger.LogInformation($"Scanning Lib '{m.Title}'");
                    await ProcessTv(server, recentlyAdded, m.Key);
                    await ProcessMovies(server, recentlyAdded, m.Key);
                }
            }
            else
            {
                await ProcessMovies(server, recentlyAdded);
                await ProcessTv(server, recentlyAdded);
            }
        }

        private async Task ProcessTv(EmbyServers server, bool recentlyAdded, string parentId = default)
        {
            // TV Time
            var mediaToAdd = new HashSet<EmbyContent>();
            EmbyItemContainer<EmbySeries> tv;
            if (recentlyAdded)
            {
                var recentlyAddedAmountToTake = AmountToTake / 2;
                tv = await Api.RecentlyAddedShows(server.ApiKey, parentId, 0, recentlyAddedAmountToTake, server.AdministratorId, server.FullUri);
                if (tv.TotalRecordCount > recentlyAddedAmountToTake)
                {
                    tv.TotalRecordCount = recentlyAddedAmountToTake;
                }
            }
            else
            {
                tv = await Api.GetAllShows(server.ApiKey, parentId, 0, AmountToTake, server.AdministratorId, server.FullUri);
            }
            var totalTv = tv.TotalRecordCount;
            var processed = 0;
            while (processed < totalTv)
            {
                foreach (var tvShow in tv.Items)
                {
                    processed++;
                    if (!tvShow.ProviderIds.Any())
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
                            Type = MediaType.Series,
                            EmbyId = tvShow.Id,
                            Url = EmbyHelper.GetEmbyMediaUrl(tvShow.Id, server?.ServerId, server.ServerHostname),
                            AddedAt = DateTime.UtcNow,
                        });
                    }
                    else
                    {
                        _logger.LogDebug("We already have TV Show {0}", tvShow.Name);
                    }
                }
                // Get the next batch
                if (!recentlyAdded)
                {
                    tv = await Api.GetAllShows(server.ApiKey, parentId, processed, AmountToTake, server.AdministratorId, server.FullUri);
                }
                await _repo.AddRange(mediaToAdd);
                mediaToAdd.Clear();
            }

            if (mediaToAdd.Any())
                await _repo.AddRange(mediaToAdd);
        }

        private async Task ProcessMovies(EmbyServers server, bool recentlyAdded, string parentId = default)
        {
            EmbyItemContainer<EmbyMovie> movies;
            if (recentlyAdded)
            {
                var recentlyAddedAmountToTake = AmountToTake / 2;
                movies = await Api.RecentlyAddedMovies(server.ApiKey, parentId, 0, recentlyAddedAmountToTake, server.AdministratorId, server.FullUri);
                // Setting this so we don't attempt to grab more than we need
                if (movies.TotalRecordCount > recentlyAddedAmountToTake)
                {
                    movies.TotalRecordCount = recentlyAddedAmountToTake;
                }
            }
            else
            {
                movies = await Api.GetAllMovies(server.ApiKey, parentId, 0, AmountToTake, server.AdministratorId, server.FullUri);
            }
            var totalCount = movies.TotalRecordCount;
            var processed = 0;
            var mediaToAdd = new HashSet<EmbyContent>();
            var mediaToUpdate = new HashSet<EmbyContent>();
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
                            await ProcessMovies(item, mediaToAdd, mediaToUpdate, server);
                        }
                    }
                    else
                    {
                        // Regular movie
                        await ProcessMovies(movie, mediaToAdd, mediaToUpdate, server);
                    }

                    processed++;
                }

                // Get the next batch
                // Recently Added should never be checked as the TotalRecords should equal the amount to take
                if (!recentlyAdded)
                {
                    movies = await Api.GetAllMovies(server.ApiKey, parentId, processed, AmountToTake, server.AdministratorId, server.FullUri);
                }
                await _repo.AddRange(mediaToAdd);
                await _repo.UpdateRange(mediaToUpdate);
                mediaToAdd.Clear();
            }
        }

        private async Task ProcessMovies(EmbyMovie movieInfo, ICollection<EmbyContent> content, ICollection<EmbyContent> toUpdate, EmbyServers server)
        {
            var quality = movieInfo.MediaStreams?.FirstOrDefault()?.DisplayTitle ?? string.Empty;
            var has4K = false;
            if (quality.Contains("4K", CompareOptions.IgnoreCase))
            {
                has4K = true;
            }

            // Check if it exists
            var existingMovie = await _repo.GetByEmbyId(movieInfo.Id);
            var alreadyGoingToAdd = content.Any(x => x.EmbyId == movieInfo.Id);
            if (existingMovie == null && !alreadyGoingToAdd)
            {
                if (!movieInfo.ProviderIds.Any())
                {
                    _logger.LogWarning($"Movie {movieInfo.Name} has no relevant metadata. Skipping.");
                    return;
                }
                _logger.LogDebug($"Adding new movie {movieInfo.Name}");

                content.Add(new EmbyContent
                {
                    ImdbId = movieInfo.ProviderIds.Imdb,
                    TheMovieDbId = movieInfo.ProviderIds?.Tmdb,
                    Title = movieInfo.Name,
                    Type = MediaType.Movie,
                    EmbyId = movieInfo.Id,
                    Url = EmbyHelper.GetEmbyMediaUrl(movieInfo.Id, server?.ServerId, server.ServerHostname),
                    AddedAt = DateTime.UtcNow,
                    Quality = has4K ? null : quality,
                    Has4K = has4K
                });
            }
            else
            {
                if (!existingMovie.Quality.Equals(quality, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogDebug($"We have found another quality for Movie '{movieInfo.Name}', Quality: '{quality}'");
                    existingMovie.Quality = has4K ? null : quality;
                    existingMovie.Has4K = has4K;

                    toUpdate.Add(existingMovie);
                }
                else
                {
                    // we have this
                    _logger.LogDebug($"We already have movie {movieInfo.Name}");
                }
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
