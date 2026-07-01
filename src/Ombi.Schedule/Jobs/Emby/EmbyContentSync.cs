using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.External.MediaServers.Emby;
using Ombi.Api.External.MediaServers.Emby.Models;
using Ombi.Api.External.MediaServers.Emby.Models.Media.Tv;
using Ombi.Api.External.MediaServers.Emby.Models.Movie;
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
            _feature = feature;
        }

        private readonly IEmbyContentRepository _repo;
        private readonly IFeatureService _feature;

        private const int DeleteBatchSize = 1000;

        // Every EmbyId the server reported during this run, used to work out which
        // database records no longer exist on the server.
        private readonly HashSet<string> _seenEmbyIds = new HashSet<string>();


        public async override Task Execute(IJobExecutionContext context)
        {

            await base.Execute(context);

            // A full sync has seen everything on the server, so anything in our database
            // that was not reported no longer exists in Emby (or lives in a library that
            // is not enabled) and would otherwise produce incorrect availability results.
            if (!recentlyAdded)
            {
                await RemoveStaleContent();
            }

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
                if (tv.Items == null || !tv.Items.Any())
                {
                    syncIncomplete = true;
                    _logger.LogWarning("Emby returned no TV shows at offset {0} but reported {1} total records. Stopping the sync for this library to avoid an infinite loop.",
                        processed, totalTv);
                    break;
                }

                foreach (var tvShow in tv.Items)
                {
                    processed++;
                    _seenEmbyIds.Add(tvShow.Id);

                    // ProviderIds can be missing entirely from the API response, so guard
                    // against null as well as empty. A series without provider ids is still
                    // added (with its EmbyId only) so that its episodes can be linked and
                    // the metadata refresh job can backfill the ids from the title later.
                    var hasProviderIds = tvShow.ProviderIds?.Any() == true;
                    if (!hasProviderIds)
                    {
                        _logger.LogInformation("Provider Id on tv {0} is null, adding it with its Emby Id only. The metadata refresh will attempt to backfill the provider ids.", tvShow.Name);
                    }

                    var existingTv = await _repo.GetByEmbyId(tvShow.Id);

                    // Only treat differing ids as a reidentification when the incoming item
                    // actually has provider ids, otherwise a series that lost its metadata
                    // would wipe the ids we already have (including ones backfilled by the
                    // metadata refresh job).
                    if (existingTv != null && hasProviderIds &&
                        ( existingTv.ImdbId != tvShow.ProviderIds?.Imdb
                        || existingTv.TheMovieDbId != tvShow.ProviderIds?.Tmdb
                        || existingTv.TvDbId != tvShow.ProviderIds?.Tvdb))
                    {
                        // TODO: Existing TvRequest records referencing the old TheMovieDbId via ExternalProviderId
                        // will not be updated here. This is a pre-existing limitation (the old delete+reinsert
                        // also left requests orphaned). Consider migrating TvRequest.ExternalProviderId in future.
                        _logger.LogDebug($"Series '{tvShow.Name}' has different IDs, probably a reidentification. Updating IDs in place.");
                        existingTv.ImdbId = tvShow.ProviderIds?.Imdb;
                        existingTv.TheMovieDbId = tvShow.ProviderIds?.Tmdb;
                        existingTv.TvDbId = tvShow.ProviderIds?.Tvdb;
                        _repo.UpdateWithoutSave(existingTv);
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
                if (movies.Items == null || !movies.Items.Any())
                {
                    syncIncomplete = true;
                    _logger.LogWarning("Emby returned no movies at offset {0} but reported {1} total records. Stopping the sync for this library to avoid an infinite loop.",
                        processed, totalCount);
                    break;
                }

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
            _seenEmbyIds.Add(movieInfo.Id);

            var quality = movieInfo.MediaStreams?.FirstOrDefault()?.DisplayTitle ?? string.Empty;
            var has4K = false;
            if (quality.Contains("4K", CompareOptions.IgnoreCase))
            {
                has4K = true;
            }

            // ProviderIds can be missing entirely from the API response
            var hasProviderIds = movieInfo.ProviderIds?.Any() == true;

            // Check if it exists
            var existingMovie = await _repo.GetByEmbyId(movieInfo.Id);
            var alreadyGoingToAdd = content.Any(x => x.EmbyId == movieInfo.Id);
            if (alreadyGoingToAdd)
            {
                _logger.LogDebug($"Detected duplicate for {movieInfo.Name}");
                return;
            }
            if (existingMovie == null)
            {
                if (!hasProviderIds)
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
                if (hasProviderIds && (existingMovie.ImdbId != movieInfo.ProviderIds.Imdb || existingMovie.TheMovieDbId != movieInfo.ProviderIds.Tmdb))
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
            content.ImdbId = movieInfo.ProviderIds?.Imdb;
            content.TheMovieDbId = movieInfo.ProviderIds?.Tmdb;
            content.Title = movieInfo.Name;
            content.Type = MediaType.Movie;
            content.EmbyId = movieInfo.Id;
            content.Url = EmbyHelper.GetEmbyMediaUrl(movieInfo.Id, server?.ServerId, server.ServerHostname);
            content.Quality = has4K ? null : quality;
            content.Has4K = has4K;
        }

        /// <summary>
        /// Removes content records (and their episodes) that were not reported by the
        /// server during this run. This clears out ghosts left behind by removed or
        /// reidentified items, which otherwise keep matching availability lookups forever.
        /// Only runs after a complete full sync so we never delete based on partial data.
        /// </summary>
        private async Task RemoveStaleContent()
        {
            if (syncIncomplete)
            {
                _logger.LogWarning("Skipping the stale Emby content cleanup because the sync did not fully complete. Removing records based on a partial sync could delete content that still exists on the server.");
                return;
            }

            if (!_seenEmbyIds.Any())
            {
                _logger.LogInformation("Skipping the stale Emby content cleanup because the server reported no content.");
                return;
            }

            var dbContent = await _repo.GetAllContentIdentifiers();
            var staleContent = dbContent.Where(x => string.IsNullOrEmpty(x.EmbyId) || !_seenEmbyIds.Contains(x.EmbyId)).ToList();
            if (!staleContent.Any())
            {
                return;
            }

            _logger.LogInformation("Removing {0} Emby content records that no longer exist on the server", staleContent.Count);

            var staleSeriesIds = staleContent
                .Where(x => x.Type == MediaType.Series && !string.IsNullOrEmpty(x.EmbyId))
                .Select(x => x.EmbyId)
                .ToHashSet();
            if (staleSeriesIds.Any())
            {
                var allEpisodes = await _repo.GetAllEpisodeIdentifiers();
                var orphanedEpisodes = allEpisodes.Where(x => staleSeriesIds.Contains(x.ParentId)).ToList();
                if (orphanedEpisodes.Any())
                {
                    _logger.LogInformation("Removing {0} episodes belonging to the removed series", orphanedEpisodes.Count);
                    foreach (var chunk in orphanedEpisodes.Chunk(DeleteBatchSize))
                    {
                        await _repo.DeleteEpisodes(chunk);
                    }
                }
            }

            foreach (var chunk in staleContent.Chunk(DeleteBatchSize))
            {
                await _repo.DeleteRange(chunk);
            }
        }
    }

}
