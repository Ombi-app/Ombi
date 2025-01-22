#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: PlexServerContentCacher.cs
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
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TheMovieDb.Models;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Schedule.Jobs.Plex.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexContentSync : PlexLibrarySync, IPlexContentSync
    {
        private readonly IMovieDbApi _movieApi;
        private readonly IMediaCacheService _mediaCacheService;
        private readonly IFeatureService _feature;
        private ProcessedContent _processedContent;


        public PlexContentSync(
            ISettingsService<PlexSettings> plex, 
            IPlexApi plexApi, ILogger<PlexLibrarySync> logger, 
            IPlexContentRepository repo,
            IPlexEpisodeSync epsiodeSync, 
            INotificationHubService notificationHubService, 
            IMovieDbApi movieDbApi, 
            IMediaCacheService mediaCacheService, 
            IFeatureService feature):
            base(plex, plexApi, logger, notificationHubService)
        {
            Repo = repo;
            EpisodeSync = epsiodeSync;
            _movieApi = movieDbApi;
            _mediaCacheService = mediaCacheService;
            _feature = feature;
            Plex.ClearCache();
        }

        private IPlexContentRepository Repo { get; }
        private IPlexEpisodeSync EpisodeSync { get; }
        public async override Task Execute(IJobExecutionContext context)
        {
            
            _processedContent = new ProcessedContent();

            await base.Execute(context);
            
            if (!recentlyAddedSearch)
            {
                await NotifyClient("Plex Sync - Starting Episode Sync");
                Logger.LogInformation("Starting EP Cacher");
                await OmbiQuartz.TriggerJob(nameof(IPlexEpisodeSync), "Plex");
            }

            if ((_processedContent?.HasProcessedContent ?? false) && recentlyAddedSearch)
            {
                await NotifyClient("Plex Sync - Checking if any requests are now available");
                Logger.LogInformation("Kicking off Plex Availability Checker");
                await OmbiQuartz.TriggerJob(nameof(IPlexAvailabilityChecker), "Plex");
            }
            var processedCont = _processedContent?.Content?.Count() ?? 0;
            var processedEp = _processedContent?.Episodes?.Count() ?? 0;
            Logger.LogInformation("Finished Plex Content Cacher, with processed content: {0}, episodes: {1}. Recently Added Scan: {2}", processedCont, processedEp, recentlyAddedSearch);

            await NotifyClient(recentlyAddedSearch ? $"Plex Recently Added Sync Finished, We processed {processedCont}, and {processedEp} Episodes" : "Plex Content Sync Finished");

            // Played state
            var isPlayedSyncEnabled = await _feature.FeatureEnabled(FeatureNames.PlayedSync); 
            if(isPlayedSyncEnabled) 
            {
                await OmbiQuartz.Scheduler.TriggerJob(new JobKey(nameof(IPlexPlayedSync), "Plex"), new JobDataMap(new Dictionary<string, string> { { JobDataKeys.RecentlyAddedSearch, recentlyAddedSearch.ToString() } }));
            }

            await _mediaCacheService.Purge();
            
        }


        protected override async Task ProcessServer(PlexServers servers)
        {
            var contentProcessed = new Dictionary<int, string>();
            var episodesProcessed = new List<int>();
            Logger.LogDebug("Getting all content from server {0}", servers.Name);
            var allContent = await GetAllContent(servers, recentlyAddedSearch);
            Logger.LogDebug("We found {0} items", allContent.Count);


            // Let's now process this.
            var contentToAdd = new HashSet<PlexServerContent>();

            var allEps = Repo.GetAllEpisodes();

            foreach (var content in allContent.OrderByDescending(x => x.viewGroup))
            {
                Logger.LogDebug($"Got type '{content.viewGroup}' to process");
                if (content.viewGroup.Equals(PlexMediaType.Episode.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.LogDebug("Found some episodes, this must be a recently added sync");
                    var count = 0;
                    foreach (var epInfo in content.Metadata ?? new Metadata[] { })
                    {
                        count++;
                        var grandParentKey = epInfo.grandparentRatingKey;
                        // Lookup the rating key
                        var showMetadata = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri, grandParentKey);
                        var show = showMetadata.MediaContainer.Metadata.FirstOrDefault();
                        if (show == null)
                        {
                            continue;
                        }

                        await ProcessTvShow(servers, show, contentToAdd, contentProcessed);
                        if (contentToAdd.Any())
                        {
                            await Repo.AddRange(contentToAdd, recentlyAddedSearch ? true : false);
                            if (recentlyAddedSearch)
                            {
                                foreach (var plexServerContent in contentToAdd)
                                {
                                    if (plexServerContent.Id <= 0)
                                    {
                                        Logger.LogInformation($"Item '{plexServerContent.Title}' has an Plex ID of {plexServerContent.Id} and a Plex Key of {plexServerContent.Key}");
                                    }
                                    contentProcessed.Add(plexServerContent.Id, plexServerContent.Key);
                                }
                            }
                            contentToAdd.Clear();
                        }
                        if (count > 30)
                        {
                            await Repo.SaveChangesAsync();
                            count = 0;
                        }
                    }

                    // Save just to make sure we don't leave anything hanging
                    await Repo.SaveChangesAsync();
                    if (content.Metadata != null)
                    {
                        var episodesAdded = await EpisodeSync.ProcessEpsiodes(content.Metadata, (IQueryable<PlexEpisode>)allEps);
                        episodesProcessed.AddRange(episodesAdded.Select(x => x.Id));
                    }
                }
                if (content.viewGroup.Equals(PlexMediaType.Show.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    // Process Shows
                    Logger.LogDebug("Processing TV Shows");
                    var count = 0;
                    foreach (var show in content.Metadata ?? new Metadata[] { })
                    {
                        count++;
                        await ProcessTvShow(servers, show, contentToAdd, contentProcessed);

                        if (contentToAdd.Any())
                        {
                            await Repo.AddRange(contentToAdd, false);
                            if (recentlyAddedSearch)
                            {
                                foreach (var plexServerContent in contentToAdd)
                                {
                                    contentProcessed.Add(plexServerContent.Id, plexServerContent.Key);
                                }
                            }
                            contentToAdd.Clear();
                        }
                        if (count > 30)
                        {
                            await Repo.SaveChangesAsync();
                        }
                    }

                    await Repo.SaveChangesAsync();
                }
                if (content.viewGroup.Equals(PlexMediaType.Movie.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    await MovieLoop(servers, content, contentToAdd, contentProcessed);
                }
                if (contentToAdd.Count > 500)
                {
                    await Repo.AddRange(contentToAdd);
                    foreach (var c in contentToAdd)
                    {
                        contentProcessed.Add(c.Id, c.Key);
                    }
                    contentToAdd.Clear();
                }
            }

            if (contentToAdd.Any())
            {
                await Repo.AddRange(contentToAdd);
                foreach (var c in contentToAdd)
                {
                    if (contentProcessed.ContainsKey(c.Id)) {
                        continue;
                    }

                    contentProcessed.Add(c.Id, c.Key);
                }
            }

            _processedContent.Content = contentProcessed.Values;
            _processedContent.Episodes = episodesProcessed;
        }

        public async Task MovieLoop(PlexServers servers, Mediacontainer content, HashSet<PlexServerContent> contentToAdd,
            Dictionary<int, string> contentProcessed)
        {
            Logger.LogDebug("Processing Movies");
            foreach (var movie in content?.Metadata ?? Array.Empty<Metadata>())
            {
                // Let's check if we have this movie

                try
                {
                    var existing = await Repo.GetFirstContentByCustom(x => x.Title == movie.title
                                                                           && x.ReleaseYear == movie.year.ToString()
                                                                           && x.Type == MediaType.Movie);


                    if (existing == null)
                    {
                        // Let's just check the key
                        existing = await Repo.GetByKey(movie.ratingKey);
                    }
                    if (existing != null)
                    {
                        // We need to see if this is a different quality,
                        // We want to know if this is a 4k content for example
                        var foundQualities = movie.Media?.Select(x => x.videoResolution);
                        var qualitySaved = false;
                        foreach (var quality in foundQualities)
                        {
                            if (qualitySaved)
                            {
                                break;
                            }
                            if (quality == null || quality.Equals(existing.Quality))
                            {
                                // We got it
                                continue;
                            }

                            // We don't have this quality
                            if (quality.Equals("4k", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Logger.LogDebug($"We already have movie {movie.title}, But found a 4K version!");
                                existing.Has4K = true;
                                await Repo.Update(existing);
                            } 
                            else
                            {
                                qualitySaved = true;
                                existing.Quality = quality;
                                await Repo.Update(existing);
                            }
                        }


                        Logger.LogDebug($"We already have movie {movie.title}");
                        continue;
                    } 

                    Logger.LogDebug("Adding movie {0}", movie.title);
                    var guids = new List<string>();
                    if (!movie.Guid.Any())
                    {
                        var metaData = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                            movie.ratingKey);

                        var meta = metaData.MediaContainer.Metadata.FirstOrDefault();
                        guids.Add(meta.guid);
                        if (meta.Guid != null)
                        {
                            foreach (var g in meta.Guid)
                            {
                                guids.Add(g.Id);
                            }
                        }
                    }
                    else
                    {
                        // Currently a Plex Pass feature only
                        foreach (var g in movie.Guid)
                        {
                            guids.Add(g.Id);
                        }
                    }

                    if (!guids.Any())
                    {
                        Logger.LogWarning($"Movie {movie.title} has no relevant metadata. Skipping.");
                        continue;
                    }
                    var providerIds = PlexHelper.GetProviderIdsFromMetadata(guids.ToArray());

                    if (!providerIds.Any())
                    {
                        Logger.LogWarning($"Movie {movie.title} has no External Ids in Plex (ImdbId, TheMovieDbId). Skipping.");
                        continue;
                    }

                    var qualities = movie?.Media?.Select(x => x?.videoResolution ?? string.Empty) ?? Enumerable.Empty<string>();
                    var is4k = qualities != null && qualities.Any(x => x.Equals("4k", StringComparison.InvariantCultureIgnoreCase));
                    var selectedQuality = is4k ? null : qualities?.OrderBy(x => x)?.FirstOrDefault() ?? string.Empty;

                    var item = new PlexServerContent
                    {
                        AddedAt = DateTime.Now,
                        Key = movie.ratingKey,
                        ReleaseYear = movie.year.ToString(),
                        Type = MediaType.Movie,
                        Title = movie.title,
                        Url = PlexHelper.GetPlexMediaUrl(servers.MachineIdentifier, movie.ratingKey, servers.ServerHostname),
                        Seasons = new List<PlexSeasonsContent>(),
                        Quality = selectedQuality,
                        Has4K = is4k,
                    };
                    if (providerIds.ImdbId.HasValue())
                    {
                        item.ImdbId = providerIds.ImdbId;
                    }

                    if (providerIds.TheMovieDb.HasValue())
                    {
                        item.TheMovieDbId = providerIds.TheMovieDb;
                    }

                    if (providerIds.TheTvDb.HasValue())
                    {
                        item.TvDbId = providerIds.TheTvDb;
                    }

                    contentToAdd.Add(item);
                }
                catch (Exception e)
                {
                    Logger.LogError(LoggingEvents.PlexContentCacher, e, "Exception when adding new Movie {0}",
                        movie.title);
                }

                if (contentToAdd.Count > 500)
                {
                    await Repo.AddRange(contentToAdd);
                    foreach (var c in contentToAdd)
                    {
                        contentProcessed.Add(c.Id, c.Key);
                    }

                    contentToAdd.Clear();
                }
            }
        }

        private async Task ProcessTvShow(PlexServers servers, Metadata show, HashSet<PlexServerContent> contentToAdd, Dictionary<int, string> contentProcessed)
        {
            var seasonList = await PlexApi.GetSeasons(servers.PlexAuthToken, servers.FullUri,
                show.ratingKey);
            var seasonsContent = new List<PlexSeasonsContent>();
            foreach (var season in seasonList.MediaContainer.Metadata)
            {
                seasonsContent.Add(new PlexSeasonsContent
                {
                    ParentKey = season.parentRatingKey,
                    SeasonKey = season.ratingKey,
                    SeasonNumber = season.index,
                    PlexContentId = show.ratingKey
                });
            }

            // Do we already have this item?
            // Let's try and match 
            var existingContent = await Repo.GetFirstContentByCustom(x => x.Title == show.title
                                                                          && x.ReleaseYear == show.year.ToString()
                                                                          && x.Type == MediaType.Series);

            // Just double check the rating key, since this is our unique constraint
            var existingKey = await Repo.GetByKey(show.ratingKey);

            if (existingKey != null)
            {
                // Damn son.
                // Let's check if they match up
                var doesMatch = show.title.Equals(existingKey.Title,
                    StringComparison.CurrentCulture);
                if (!doesMatch)
                {
                    // Something fucked up on Plex at somepoint... Damn, rebuild of lib maybe?
                    // Lets delete the matching key
                    await Repo.Delete(existingKey);
                    existingKey = null;
                }
                else if (existingContent == null)
                {
                    existingContent = await Repo.GetFirstContentByCustom(x => x.Key == show.ratingKey);
                }
            }

            if (existingContent != null)
            {
                // Let's make sure that we have some sort of ID e.g. Imdbid for this,
                // Looks like it's possible to not have an Id for a show
                // I suspect we cached that show just as it was added to Plex.

                if (!existingContent.HasImdb && !existingContent.HasTheMovieDb && !existingContent.HasTvDb)
                {
                    var showMetadata = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                        existingContent.Key);
                    await GetProviderIds(showMetadata, existingContent);

                    await Repo.Update(existingContent);
                }

                //// Just check the key
                //if (existingKey != null)
                //{
                //    // The rating key is all good!
                //}
                //else
                //{
                //    // This means the rating key has changed somehow.
                //    // Should probably delete this and get the new one
                //    var oldKey = existingContent.Key;
                //    Repo.DeleteWithoutSave(existingContent);

                //    // Because we have changed the rating key, we need to change all children too
                //    var episodeToChange = Repo.GetAllEpisodes().Cast<PlexEpisode>().Where(x => x.GrandparentKey == oldKey);
                //    if (episodeToChange.Any())
                //    {
                //        foreach (var e in episodeToChange)
                //        {
                //            Repo.DeleteWithoutSave(e);
                //        }
                //    }

                //    await Repo.SaveChangesAsync();
                //    existingContent = null;
                //}
            }

            // Also make sure it's not already being processed...
            var alreadyProcessed = contentProcessed.Select(x => x.Value).Any(x => x == show.ratingKey);
            if (alreadyProcessed)
            {
                return;
            }

            // The ratingKey keeps changing...
            //var existingContent = await Repo.GetByKey(show.ratingKey);
            if (existingContent != null)
            {
                try
                {
                    Logger.LogDebug("We already have show {0} checking for new seasons",
                        existingContent.Title);
                    // Ok so we have it, let's check if there are any new seasons
                    var itemAdded = false;
                    foreach (var season in seasonsContent)
                    {
                        var seasonExists =
                            existingContent.Seasons.FirstOrDefault(x => x.SeasonKey == season.SeasonKey);

                        if (seasonExists != null)
                        {
                            // We already have this season
                            // check if we have the episode
                            //if (episode != null)
                            //{
                            //    var existing = existingContent.Episodes.Any(x =>
                            //        x.SeasonNumber == episode.parentIndex && x.EpisodeNumber == episode.index);
                            //    if (!existing)
                            //    {
                            //        // We don't have this episode, lets add it
                            //        existingContent.Episodes.Add(new PlexEpisode
                            //        {
                            //            EpisodeNumber = episode.index,
                            //            SeasonNumber = episode.parentIndex,
                            //            GrandparentKey = episode.grandparentRatingKey,
                            //            ParentKey = episode.parentRatingKey,
                            //            Key = episode.ratingKey,
                            //            Title = episode.title
                            //        });
                            //        itemAdded = true;
                            //    }
                            //}
                            continue;
                        }

                        existingContent.Seasons.Add(season);
                        itemAdded = true;
                    }

                    if (itemAdded) await Repo.Update(existingContent);
                }
                catch (Exception e)
                {
                    Logger.LogError(LoggingEvents.PlexContentCacher, e,
                        "Exception when adding new seasons to title {0}", existingContent.Title);
                }
            }
            else
            {
                try
                {
                    Logger.LogDebug("New show {0}, so add it", show.title);

                    // Get the show metadata... This sucks since the `metadata` var contains all information about the show
                    // But it does not contain the `guid` property that we need to pull out thetvdb id...
                    var showMetadata = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                        show.ratingKey);

                    var item = new PlexServerContent
                    {
                        AddedAt = DateTime.Now,
                        Key = show.ratingKey,
                        ReleaseYear = show.year.ToString(),
                        Type = MediaType.Series,
                        Title = show.title,
                        Url = PlexHelper.GetPlexMediaUrl(servers.MachineIdentifier, show.ratingKey, servers.ServerHostname),
                        Seasons = new List<PlexSeasonsContent>()
                    };
                    await GetProviderIds(showMetadata, item);

                    // Let's just double check to make sure we do not have it now we have some id's
                    var existingImdb = false;
                    var existingMovieDbId = false;
                    var existingTvDbId = false;
                    if (item.ImdbId.HasValue())
                    {
                        existingImdb = await Repo.GetAll().AnyAsync(x =>
                            x.ImdbId == item.ImdbId && x.Type == MediaType.Series);
                    }

                    if (item.TheMovieDbId.HasValue())
                    {
                        existingMovieDbId = await Repo.GetAll().AnyAsync(x =>
                            x.TheMovieDbId == item.TheMovieDbId && x.Type == MediaType.Series);
                    }

                    if (item.TvDbId.HasValue())
                    {
                        existingTvDbId = await Repo.GetAll().AnyAsync(x =>
                            x.TvDbId == item.TvDbId && x.Type == MediaType.Series);
                    }

                    if (existingImdb || existingTvDbId || existingMovieDbId)
                    {
                        // We already have it!
                        return;
                    }

                    item.Seasons.ToList().AddRange(seasonsContent);

                    contentToAdd.Add(item);
                }
                catch (Exception e)
                {
                    Logger.LogError(LoggingEvents.PlexContentCacher, e, "Exception when adding tv show {0}",
                        show.title);
                }
            }
        }

        private async Task GetProviderIds(PlexMetadata showMetadata, PlexServerContent existingContent)
        {
            var metadata = showMetadata.MediaContainer.Metadata.FirstOrDefault();
            var guids = new List<string>
            {
                metadata.guid
            };
            if (metadata.Guid != null)
            {
                foreach (var g in metadata.Guid)
                {
                    guids.Add(g.Id);
                }
            }
            var providerIds =
                PlexHelper.GetProviderIdsFromMetadata(guids.ToArray());
            if (providerIds.ImdbId.HasValue())
            {
                existingContent.ImdbId = providerIds.ImdbId;
            }

            if (providerIds.TheMovieDb.HasValue())
            {
                existingContent.TheMovieDbId = providerIds.TheMovieDb;
            }

            if (providerIds.TheTvDb.HasValue())
            {
                // Lookup TheMovieDbId
                var findResult = await _movieApi.Find(providerIds.TheTvDb, ExternalSource.tvdb_id);
                var tvResult = findResult.tv_results.FirstOrDefault();
                if (tvResult != null)
                {
                    existingContent.TheMovieDbId = tvResult.id.ToString();
                }
                existingContent.TvDbId = providerIds.TheTvDb;
            }
        }

        /// <summary>
        /// Gets all the library sections.
        /// If the user has specified only certain libraries then we will only look for those
        /// If they have not set the settings then we will monitor them all
        /// </summary>
        /// <param name="plexSettings">The plex settings.</param>
        /// <param name="recentlyAddedSearch"></param>
        /// <returns></returns>
        private async Task<List<Mediacontainer>> GetAllContent(PlexServers plexSettings, bool recentlyAddedSearch)
        {
            var libs = new List<Mediacontainer>();

            var directories = await GetEnabledLibraries(plexSettings);

            foreach (var directory in directories)
            {
                if (recentlyAddedSearch)
                {
                    var container = await PlexApi.GetRecentlyAdded(plexSettings.PlexAuthToken, plexSettings.FullUri,
                        directory.key);
                    if (container != null)
                    {
                        libs.Add(container.MediaContainer);
                    }
                }
                else
                {
                    var lib = await PlexApi.GetLibrary(plexSettings.PlexAuthToken, plexSettings.FullUri, directory.key);
                    if (lib != null)
                    {
                        libs.Add(lib.MediaContainer);
                    }
                }
            }

            return libs;
        }


        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //Plex?.Dispose();
                EpisodeSync?.Dispose();
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