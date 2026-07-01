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

        // A single video file spanning more episodes than this is treated as corrupt
        // metadata (e.g. absolute numbering leaking into IndexNumberEnd).
        private const int MaxEpisodeFillCount = 50;

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

            // Every EmbyId:EpisodeNumber the server reported during this run, used to work
            // out which episode records no longer exist on the server. Only trustworthy
            // when every server and library completed, tracked via syncIncomplete.
            var seenEpisodeKeys = new HashSet<string>();
            var syncIncomplete = false;

            foreach (var server in settings.Servers)
            {
                try
                {
                    if (server.EmbySelectedLibraries.Any() && server.EmbySelectedLibraries.Any(x => x.Enabled))
                    {
                        var tvLibsToFilter = server.EmbySelectedLibraries.Where(x => x.Enabled && x.CollectionType is "tvshows" or "mixed");
                        foreach (var tvParentIdFilter in tvLibsToFilter)
                        {
                            _logger.LogInformation($"Scanning Lib for episodes '{tvParentIdFilter.Title}'");
                            syncIncomplete |= !await CacheEpisodes(server, recentlyAddedSearch, tvParentIdFilter.Key, seenEpisodeKeys);
                        }
                    }
                    else
                    {
                        syncIncomplete |= !await CacheEpisodes(server, recentlyAddedSearch, string.Empty, seenEpisodeKeys);
                    }
                }
                catch (Exception e)
                {
                    syncIncomplete = true;
                    await _notification.SendNotificationToAdmins("Emby Episode Sync Failed");
                    _logger.LogError(e, "Exception when syncing Emby episodes for server {0}", server.Name);
                }
            }

            // A full sync has seen every episode on the server, so anything in our
            // database that was not reported no longer exists in Emby. This purges ghost
            // records left behind by reidentifications (changed episode numbers create a
            // new row and orphan the old one) which otherwise report availability for
            // episodes that no longer exist.
            if (!recentlyAddedSearch)
            {
                await RemoveStaleEpisodes(seenEpisodeKeys, syncIncomplete);
            }

            await _notification.SendNotificationToAdmins("Emby Episode Sync Finished");
            _logger.LogInformation("Emby Episode Sync Finished - Triggering Metadata refresh");
            await OmbiQuartz.TriggerJob(nameof(IRefreshMetadata), "System");
        }

        private async Task<bool> CacheEpisodes(EmbyServers server, bool recentlyAdded, string parentIdFilter, HashSet<string> seenEpisodeKeys)
        {
            // Preload existing data to eliminate N+1 queries
            var seriesLookup = await _repo.GetAllSeriesEmbyIds();
            var episodeMetadata = await _repo.GetAllEpisodeMetadata();

            var total = 0;
            var processed = 0;
            var epToAdd = new HashSet<EmbyEpisode>();
            var hasUpserts = false;
            var pendingUpdates = new Dictionary<string, (string EmbyId, int EpisodeNumber, int SeasonNumber)>();
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

            var completedWithoutGaps = true;
            while (processed < total)
            {
                if (allEpisodes.Items == null || !allEpisodes.Items.Any())
                {
                    completedWithoutGaps = false;
                    _logger.LogWarning("Emby returned no episodes at offset {0} but reported {1} total records. Stopping the sync for this library to avoid an infinite loop.",
                        processed, total);
                    break;
                }

                _logger.LogInformation($"Processing chunk {processed}/{total}");
                // Process episodes in current chunk
                foreach (var ep in allEpisodes.Items)
                {
                    processed++;

                    // Record everything the server reports, even episodes we go on to
                    // skip - "seen" means "exists in Emby", which is what the stale
                    // record cleanup needs to know.
                    RecordSeenEpisode(ep, seenEpisodeKeys);

                    try
                    {
                        ProcessEpisode(ep, seriesLookup, episodeMetadata, epToAdd, pendingUpdates, episodesInCurrentBatch);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception when processing episode {0} ({1}), skipping it and continuing with the rest of the sync", ep.Name, ep.Id);
                    }
                }

                // Only commit to database when we reach the batch size or finish processing
                // Apply batched metadata updates
                if (pendingUpdates.Any())
                {
                    // Group updates by EmbyId so we update all rows for multi-episode files
                    var updatesByEmbyId = pendingUpdates.GroupBy(u => u.Value.EmbyId);
                    foreach (var group in updatesByEmbyId)
                    {
                        var entities = await _repo.GetEpisodesByEmbyId(group.Key);
                        foreach (var entity in entities)
                        {
                            var matchingUpdate = group.FirstOrDefault(u => u.Value.EpisodeNumber == entity.EpisodeNumber);
                            if (matchingUpdate.Key != null)
                            {
                                entity.SeasonNumber = matchingUpdate.Value.SeasonNumber;
                                hasUpserts = true;
                            }
                            else
                            {
                                _logger.LogDebug("No matching update found for episode {EpisodeNumber} in EmbyId {EmbyId}",
                                    entity.EpisodeNumber, group.Key);
                            }
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
                            episodeMetadata[$"{episode.EmbyId}:{episode.EpisodeNumber}"] = (episode.EpisodeNumber, episode.SeasonNumber);
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

            return completedWithoutGaps;
        }

        private static void RecordSeenEpisode(EmbyEpisodes ep, HashSet<string> seenEpisodeKeys)
        {
            seenEpisodeKeys.Add($"{ep.Id}:{ep.IndexNumber}");

            // Multi-episode files produce one database row per episode in the span, all
            // sharing the same EmbyId. Mirror the sane-range rules used when inserting.
            if (ep.IndexNumberEnd.HasValue
                && ep.IndexNumberEnd.Value > ep.IndexNumber
                && ep.IndexNumberEnd.Value - ep.IndexNumber <= MaxEpisodeFillCount)
            {
                for (var episodeNumber = ep.IndexNumber + 1; episodeNumber <= ep.IndexNumberEnd.Value; episodeNumber++)
                {
                    seenEpisodeKeys.Add($"{ep.Id}:{episodeNumber}");
                }
            }
        }

        /// <summary>
        /// Removes episode records that were not reported by the server during a full
        /// sync. Skipped when the sync was incomplete or reported nothing, so records are
        /// never removed based on partial data.
        /// </summary>
        private async Task RemoveStaleEpisodes(HashSet<string> seenEpisodeKeys, bool syncIncomplete)
        {
            if (syncIncomplete)
            {
                _logger.LogWarning("Skipping the stale Emby episode cleanup because the sync did not fully complete. Removing records based on a partial sync could delete episodes that still exist on the server.");
                return;
            }

            if (!seenEpisodeKeys.Any())
            {
                _logger.LogInformation("Skipping the stale Emby episode cleanup because the server reported no episodes.");
                return;
            }

            var dbEpisodes = await _repo.GetAllEpisodeIdentifiers();
            var staleEpisodes = dbEpisodes.Where(x => !seenEpisodeKeys.Contains($"{x.EmbyId}:{x.EpisodeNumber}")).ToList();
            if (!staleEpisodes.Any())
            {
                return;
            }

            _logger.LogInformation("Removing {0} episode records that no longer exist on the Emby server", staleEpisodes.Count);
            foreach (var chunk in staleEpisodes.Chunk(DatabaseBatchSize))
            {
                await _repo.DeleteEpisodes(chunk);
            }
        }

        private void ProcessEpisode(
            EmbyEpisodes ep,
            HashSet<string> seriesLookup,
            Dictionary<string, (int EpisodeNumber, int SeasonNumber)> episodeMetadata,
            HashSet<EmbyEpisode> epToAdd,
            Dictionary<string, (string EmbyId, int EpisodeNumber, int SeasonNumber)> pendingUpdates,
            HashSet<string> episodesInCurrentBatch)
        {
            // Check if parent series exists using preloaded HashSet (O(1) lookup)
            if (!seriesLookup.Contains(ep.SeriesId))
            {
                _logger.LogInformation("The episode {0} does not relate to a series, so we cannot save this",
                    ep.Name);
                return;
            }

            // Create unique key for multi-episode files to prevent duplicates
            var episodeKey = $"{ep.Id}_{ep.IndexNumber}_{ep.ParentIndexNumber}";

            // Check if episode already exists using preloaded metadata (O(1) lookup)
            var metadataKey = $"{ep.Id}:{ep.IndexNumber}";
            var existingInDatabase = episodeMetadata.ContainsKey(metadataKey);
            var existingInCurrentBatch = episodesInCurrentBatch.Contains(episodeKey);

            if (existingInDatabase)
            {
                // Check if metadata has changed (e.g. Emby re-identified the file)
                var existing = episodeMetadata[metadataKey];
                if (existing.EpisodeNumber != ep.IndexNumber || existing.SeasonNumber != ep.ParentIndexNumber)
                {
                    _logger.LogInformation("Episode {0} metadata changed (S{1}E{2} -> S{3}E{4}), queuing update",
                        ep.Name, existing.SeasonNumber, existing.EpisodeNumber, ep.ParentIndexNumber, ep.IndexNumber);
                    pendingUpdates[metadataKey] = (ep.Id, ep.IndexNumber, ep.ParentIndexNumber);
                    episodeMetadata[metadataKey] = (ep.IndexNumber, ep.ParentIndexNumber);
                }
            }
            else if (!existingInCurrentBatch)
            {
                // Sanity checks - skip only true unindexed specials (no episode AND no season number)
                if (ep.IndexNumber == 0 && ep.ParentIndexNumber == 0)
                {
                    _logger.LogWarning($"Episode {ep.Name} has no episode or season number. Skipping.");
                    return;
                }

                _logger.LogDebug("Adding new episode {0} to parent {1}", ep.Name, ep.SeriesName);

                // add it
                epToAdd.Add(BuildEpisode(ep, ep.IndexNumber));
                episodesInCurrentBatch.Add(episodeKey);

                // A multi-episode file spans IndexNumber..IndexNumberEnd. Only fill the
                // additional episodes when the range is sane: IndexNumberEnd must be
                // greater than IndexNumber. Some Emby servers report a bogus
                // IndexNumberEnd (absolute numbering or corrupt metadata) that is far
                // larger than - or even smaller than - IndexNumber, which previously
                // either skipped the file entirely or fabricated phantom episode rows.
                if (ep.IndexNumberEnd.HasValue && ep.IndexNumberEnd.Value > ep.IndexNumber)
                {
                    var episodeFillCount = ep.IndexNumberEnd.Value - ep.IndexNumber;

                    if (episodeFillCount > MaxEpisodeFillCount)
                    {
                        // The primary episode has already been added above; we just skip
                        // the implausible fill rather than discarding the whole file.
                        _logger.LogWarning(
                            $"Episode {ep.Name} from series {ep.SeriesName} reports {episodeFillCount} episodes in a single file, which is almost certainly incorrect metadata. Only the primary episode was added.");
                    }
                    else
                    {
                        for (var episodeNumber = ep.IndexNumber + 1; episodeNumber <= ep.IndexNumberEnd.Value; episodeNumber++)
                        {
                            var multiEpisodeKey = $"{ep.Id}_{episodeNumber}_{ep.ParentIndexNumber}";
                            var multiEpisodeMetadataKey = $"{ep.Id}:{episodeNumber}";

                            // Skip if this filled episode already exists in the current
                            // batch or is already persisted in the database. EmbyEpisode
                            // has no uniqueness constraint, so an unguarded insert here
                            // would create a duplicate row.
                            if (!episodesInCurrentBatch.Contains(multiEpisodeKey)
                                && !episodeMetadata.ContainsKey(multiEpisodeMetadataKey))
                            {
                                _logger.LogDebug($"Multiple-episode file detected. Adding episode {episodeNumber}");
                                epToAdd.Add(BuildEpisode(ep, episodeNumber));
                                episodesInCurrentBatch.Add(multiEpisodeKey);
                            }
                        }
                    }
                }
            }
        }

        private static EmbyEpisode BuildEpisode(EmbyEpisodes ep, int episodeNumber)
        {
            return new EmbyEpisode
            {
                EmbyId = ep.Id,
                EpisodeNumber = episodeNumber,
                SeasonNumber = ep.ParentIndexNumber,
                ParentId = ep.SeriesId,
                TvDbId = ep.ProviderIds?.Tvdb,
                TheMovieDbId = ep.ProviderIds?.Tmdb,
                ImdbId = ep.ProviderIds?.Imdb,
                Title = ep.Name,
                AddedAt = DateTime.UtcNow
            };
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