using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models;
using Ombi.Api.Emby.Models.Media.Tv;
using Ombi.Api.Emby.Models.Movie;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;
using MediaType = Ombi.Store.Entities.MediaType;

namespace Ombi.Schedule.Jobs.Emby
{
    public class EmbyContentSync : EmbyLibrarySync, IEmbyContentSync
    {
        public EmbyContentSync(
            ISettingsService<EmbySettings> settings, 
            IEmbyApiFactory api, 
            ILogger<EmbyContentSync> logger,
            IEmbyContentRepository repo, 
            INotificationHubService notification, 
            IFeatureService feature):
            base(settings, api, logger, notification)
        {
            _repo = repo;
        }

        private readonly IEmbyContentRepository _repo;
        private readonly IFeatureService _feature;


        public async override Task Execute(IJobExecutionContext context)
        {

            await base.Execute(context);

            // Episodes
            await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IEmbyEpisodeSync), "Emby"), new JobDataMap(new Dictionary<string, string> { { JobDataKeys.EmbyRecentlyAddedSearch, recentlyAdded.ToString() } }));

            // Played state
            var isPlayedSyncEnabled = await _feature.FeatureEnabled(FeatureNames.PlayedSync); 
            if(isPlayedSyncEnabled) 
            {
            await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IEmbyPlayedSync), "Emby"), new JobDataMap(new Dictionary<string, string> { { JobDataKeys.EmbyRecentlyAddedSearch, recentlyAdded.ToString() } }));
            }
        }


        protected async override Task ProcessTv(EmbyServers server, string parentId = default)
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

                    if (existingTv != null &&
                        ( existingTv.ImdbId != tvShow.ProviderIds?.Imdb 
                        || existingTv.TheMovieDbId != tvShow.ProviderIds?.Tmdb
                        || existingTv.TvDbId != tvShow.ProviderIds?.Tvdb))
                    {
                        _logger.LogDebug($"Series '{tvShow.Name}' has different IDs, probably a reidentification.");
                        await _repo.DeleteTv(existingTv);
                        existingTv = null;
                    }

                    if (existingTv == null)
                    {
                        _logger.LogDebug("Adding TV Show {0}", tvShow.Name);
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

        protected override async Task ProcessMovies(EmbyServers server, string parentId = default)
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
                var newMovie = new EmbyContent();
                newMovie.AddedAt = DateTime.UtcNow;
                MapEmbyContent(newMovie, movieInfo, server, has4K, quality);
                content.Add(newMovie);
            }
            else
            {
                var movieHasChanged = false;
                if (existingMovie.ImdbId != movieInfo.ProviderIds.Imdb || existingMovie.TheMovieDbId != movieInfo.ProviderIds.Tmdb)
                {
                    _logger.LogDebug($"Updating existing movie '{movieInfo.Name}'");
                    MapEmbyContent(existingMovie, movieInfo, server, has4K, quality);
                    movieHasChanged = true;
                }
                else if (!quality.Equals(existingMovie?.Quality, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogDebug($"We have found another quality for Movie '{movieInfo.Name}', Quality: '{quality}'");
                    existingMovie.Quality = has4K ? null : quality;
                    existingMovie.Has4K = has4K;

                    // Probably could refactor here
                    // If a 4k movie comes in (we don't store the quality on 4k)
                    // it will always get updated even know it's not changed
                    movieHasChanged = true;
                }

                if (movieHasChanged)
                {
                    toUpdate.Add(existingMovie);
                }
                else
                {
                    // we have this
                    _logger.LogDebug($"We already have movie {movieInfo.Name}");
                }
            }
        }

        private void MapEmbyContent(EmbyContent content, EmbyMovie movieInfo, EmbyServers server, bool has4K, string quality){
            content.ImdbId = movieInfo.ProviderIds.Imdb;
            content.TheMovieDbId = movieInfo.ProviderIds?.Tmdb;
            content.Title = movieInfo.Name;
            content.Type = MediaType.Movie;
            content.EmbyId = movieInfo.Id;
            content.Url = EmbyHelper.GetEmbyMediaUrl(movieInfo.Id, server?.ServerId, server.ServerHostname);
            content.Quality = has4K ? null : quality;
            content.Has4K = has4K;
        }
    }

}
