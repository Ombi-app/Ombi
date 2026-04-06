#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: EmbyEpisodeCacher.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.External.MediaServers.Emby;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Hubs;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Api.External.MediaServers.Emby.Models;
using Ombi.Api.External.MediaServers.Emby.Models.Media.Tv;

namespace Ombi.Schedule.Jobs.Emby
{
    public class EmbyEpisodeSync : IEmbyEpisodeSync
    {
        public EmbyEpisodeSync(ISettingsService<EmbySettings> s, IEmbyApiFactory api, ILogger<EmbyEpisodeSync> l, IEmbyContentRepository repo
            , INotificationHubService notification)
        {
            _apiFactory = api;
            _logger = l;
            _settings = s;
            _repo = repo;
            _notification = notification;
        }

        private readonly ISettingsService<EmbySettings> _settings;
        private readonly IEmbyApiFactory _apiFactory;
        private readonly ILogger<EmbyEpisodeSync> _logger;
        private readonly IEmbyContentRepository _repo;
        private readonly INotificationHubService _notification;

        private const int AmountToTake = 500;
        private const int DatabaseBatchSize = 1000;

        private IEmbyApi Api { get; set; }


        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.MergedJobDataMap;
            var recentlyAddedSearch = false;
            if (dataMap.TryGetValue(JobDataKeys.EmbyRecentlyAddedSearch, out var recentlyAddedObj))
            {
                recentlyAddedSearch = Convert.ToBoolean(recentlyAddedObj);
            }

            var settings = await _settings.GetSettingsAsync();

            Api = _apiFactory.CreateClient(settings);
            await _notification.SendNotificationToAdmins("Emby Episode Sync Started");
            foreach (var server in settings.Servers)
            {
                if (server.EmbySelectedLibraries.Any() && server.EmbySelectedLibraries.Any(x => x.Enabled))
                {
                    var tvLibsToFilter = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType is "tvshows" or "mixed");
                    foreach (var tvParentIdFilter in tvLibsToFilter)
                    {
                        _logger.LogInformation($"Scanning Lib for episodes '{tvParentIdFilter.Title}'");
                        await CacheEpisodes(server, recentlyAddedSearch, tvParentIdFilter.Key);
                    }
                }
                else
                {
                    await CacheEpisodes(server, recentlyAddedSearch, string.Empty);
                }
            }

            await _notification.SendNotificationToAdmins("Emby Episode Sync Finished");
            _logger.LogInformation("Emby Episode Sync Finished - Triggering Metadata refresh");
            await OmbiQuartz.TriggerJob(nameof(IRefreshMetadata), "System");
        }

        private async Task CacheEpisodes(EmbyServers server, bool recentlyAdded, string parentIdFilter)
        {
            // Preload existing data to eliminate N+1 queries
            var seriesLookup = await _repo.GetAllSeriesEmbyIds();
            var episodeMetadata = await _repo.GetAllEpisodeMetadata();

            var total = 0;
            var processed = 0;
            var epToAdd = new HashSet<EmbyEpisode>();
            var hasUpserts = false;
            var pendingUpdates = new Dictionary<string, (int EpisodeNumber, int SeasonNumber)>();
            var episodesInCurrentBatch = new HashSet<string>(); // Track episodes in current batch to avoid duplicates

            _logger.LogInformation($"Starting episode sync for server {server.Name}");

            // Get initial episode count
            EmbyItemContainer<EmbyEpisodes> allEpisodes;
            if (recentlyAdded)
            {
                var recentlyAddedAmountToTake = AmountToTake;
                allEpisodes = await FetchEpisodesWithRetry(() => Api.RecentlyAddedEpisodes(server.ApiKey, parentIdFilter, 0, recentlyAddedAmountToTake, server.AdministratorId, server.FullUri));
                total = allEpisodes.TotalRecordCount;
                if (total > recentlyAddedAmountToTake)
                {
                    total = recentlyAddedAmountToTake;
                }
            }
            else
            {
                allEpisodes = await FetchEpisodesWithRetry(() => Api.GetAllEpisodes(server.ApiKey, parentIdFilter, 0, AmountToTake, server.AdministratorId, server.FullUri));
                total = allEpisodes.TotalRecordCount;
            }

            _logger.LogInformation($"Processing {total} episodes in chunks of {AmountToTake}");

            while (processed < total)
            {
                _logger.LogInformation($"Processing chunk {processed}/{total}");
                // Process episodes in current chunk
                foreach (var ep in allEpisodes.Items)
                {
                    processed++;

                    // Check if parent series exists using preloaded HashSet (O(1) lookup)
                    if (!seriesLookup.Contains(ep.SeriesId))
                    {
                        _logger.LogInformation("The episode {0} does not relate to a series, so we cannot save this",
                            ep.Name);
                        continue;
                    }

                    // Create unique key for multi-episode files to prevent duplicates
                    var episodeKey = $"{ep.Id}_{ep.IndexNumber}_{ep.ParentIndexNumber}";

                    // Check if episode already exists using preloaded metadata (O(1) lookup)
                    var existingInDatabase = episodeMetadata.ContainsKey(ep.Id);
                    var existingInCurrentBatch = episodesInCurrentBatch.Contains(episodeKey);

                    if (existingInDatabase)
                    {
                        // Check if metadata has changed (e.g. Emby re-identified the file)
                        var existing = episodeMetadata[ep.Id];
                        if (existing.EpisodeNumber != ep.IndexNumber || existing.SeasonNumber != ep.ParentIndexNumber)
                        {
                            _logger.LogInformation("Episode {0} metadata changed (S{1}E{2} -> S{3}E{4}), queuing update",
                                ep.Name, existing.SeasonNumber, existing.EpisodeNumber, ep.ParentIndexNumber, ep.IndexNumber);
                            pendingUpdates[ep.Id] = (ep.IndexNumber, ep.ParentIndexNumber);
                            episodeMetadata[ep.Id] = (ep.IndexNumber, ep.ParentIndexNumber);
                        }
                    }
                    else if (!existingInCurrentBatch)
                    {
                        // Sanity checks - skip only true unindexed specials (no episode AND no season number)
                        if (ep.IndexNumber == 0 && ep.ParentIndexNumber == 0)
                        {
                            _logger.LogWarning($"Episode {ep.Name} has no episode or season number. Skipping.");
                            continue;
                        }

                        _logger.LogDebug("Adding new episode {0} to parent {1}", ep.Name, ep.SeriesName);
                        
                        // add it
                        epToAdd.Add(new EmbyEpisode
                        {
                            EmbyId = ep.Id,
                            EpisodeNumber = ep.IndexNumber,
                            SeasonNumber = ep.ParentIndexNumber,
                            ParentId = ep.SeriesId,
                            TvDbId = ep.ProviderIds.Tvdb,
                            TheMovieDbId = ep.ProviderIds.Tmdb,
                            ImdbId = ep.ProviderIds.Imdb,
                            Title = ep.Name,
                            AddedAt = DateTime.UtcNow
                        });
                        episodesInCurrentBatch.Add(episodeKey);

                        if (ep.IndexNumberEnd.HasValue && ep.IndexNumberEnd.Value != ep.IndexNumber)
                        {
                            var episodeFillCount = ep.IndexNumberEnd.Value - ep.IndexNumber;

                            if (episodeFillCount > 50)
                            {
                                _logger.LogWarning($"Episode {ep.Name} has {episodeFillCount} episodes! Skipping.");
                                continue;
                            }

                            int episodeNumber = ep.IndexNumber;
                            do
                            {
                                episodeNumber++;
                                var multiEpisodeKey = $"{ep.Id}_{episodeNumber}_{ep.ParentIndexNumber}";
                                
                                // Check if this multi-episode entry already exists
                                if (!episodesInCurrentBatch.Contains(multiEpisodeKey))
                                {
                                    _logger.LogDebug($"Multiple-episode file detected. Adding episode {episodeNumber}");
                                    epToAdd.Add(new EmbyEpisode
                                    {
                                        EmbyId = ep.Id,
                                        EpisodeNumber = episodeNumber,
                                        SeasonNumber = ep.ParentIndexNumber,
                                        ParentId = ep.SeriesId,
                                        TvDbId = ep.ProviderIds.Tvdb,
                                        TheMovieDbId = ep.ProviderIds.Tmdb,
                                        ImdbId = ep.ProviderIds.Imdb,
                                        Title = ep.Name,
                                        AddedAt = DateTime.UtcNow
                                    });
                                    episodesInCurrentBatch.Add(multiEpisodeKey);
                                }

                            } while (episodeNumber < ep.IndexNumberEnd.Value);
                        }
                    }
                }

                // Only commit to database when we reach the batch size or finish processing
                // Apply batched metadata updates
                if (pendingUpdates.Any())
                {
                    foreach (var update in pendingUpdates)
                    {
                        var entity = await _repo.GetEpisodeByEmbyId(update.Key);
                        if (entity != null)
                        {
                            entity.EpisodeNumber = update.Value.EpisodeNumber;
                            entity.SeasonNumber = update.Value.SeasonNumber;
                            hasUpserts = true;
                        }
                    }
                    pendingUpdates.Clear();
                }

                if (epToAdd.Count >= DatabaseBatchSize || processed >= total)
                {
                    if (epToAdd.Any())
                    {
                        await _repo.AddRange(epToAdd);
                        _logger.LogInformation($"Committed {epToAdd.Count} episodes to database. Progress: {processed}/{total}");

                        // Update the episode metadata with newly added episodes to prevent duplicates in subsequent batches
                        foreach (var episode in epToAdd)
                        {
                            episodeMetadata[episode.EmbyId] = (episode.EpisodeNumber, episode.SeasonNumber);
                        }
                    }
                    else if (hasUpserts)
                    {
                        // Save upserted episode metadata changes even if no new episodes were added
                        await _repo.SaveChangesAsync();
                        _logger.LogInformation($"Saved episode metadata updates. Progress: {processed}/{total}");
                    }
                    epToAdd.Clear();
                    hasUpserts = false;
                    episodesInCurrentBatch.Clear();
                }

                // Get next chunk of episodes for processing
                if (!recentlyAdded && processed < total)
                {
                    allEpisodes = await FetchEpisodesWithRetry(() => Api.GetAllEpisodes(server.ApiKey, parentIdFilter, processed, AmountToTake, server.AdministratorId, server.FullUri));
                }
            }
        }
        private async Task<T> FetchEpisodesWithRetry<T>(Func<Task<T>> apiCall, int maxAttempts = 3)
        {
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await apiCall();
                }
                catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
                {
                    if (attempt >= maxAttempts)
                    {
                        throw;
                    }

                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    _logger.LogWarning(ex, "Emby API call failed (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}s...",
                        attempt, maxAttempts, delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }

            throw new InvalidOperationException("Retry logic failed unexpectedly");
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