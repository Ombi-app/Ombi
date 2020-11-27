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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Schedule.Jobs.Plex.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexContentSync : IPlexContentSync
    {
        public PlexContentSync(ISettingsService<PlexSettings> plex, IPlexApi plexApi, ILogger<PlexContentSync> logger, IPlexContentRepository repo,
            IPlexEpisodeSync epsiodeSync, IHubContext<NotificationHub> hub)
        {
            Plex = plex;
            PlexApi = plexApi;
            Logger = logger;
            Repo = repo;
            EpisodeSync = epsiodeSync;
            Notification = hub;
            Plex.ClearCache();
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private IPlexApi PlexApi { get; }
        private ILogger<PlexContentSync> Logger { get; }
        private IPlexContentRepository Repo { get; }
        private IPlexEpisodeSync EpisodeSync { get; }
        private IHubContext<NotificationHub> Notification { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var recentlyAddedSearch = dataMap.GetBooleanValueFromString(JobDataKeys.RecentlyAddedSearch);

            var plexSettings = await Plex.GetSettingsAsync();
            if (!plexSettings.Enable)
            {
                return;
            }
            await NotifyClient(recentlyAddedSearch ? "Plex Recently Added Sync Started" : "Plex Content Sync Started");
            if (!ValidateSettings(plexSettings))
            {
                Logger.LogError("Plex Settings are not valid");
                await NotifyClient(recentlyAddedSearch ? "Plex Recently Added Sync, Settings Not Valid" : "Plex Content, Settings Not Valid");
                return;
            }
            var processedContent = new ProcessedContent();
            Logger.LogInformation(recentlyAddedSearch
                ? "Starting Plex Content Cacher Recently Added Scan"
                : "Starting Plex Content Cacher");
            try
            {
                if (recentlyAddedSearch)
                {
                    processedContent = await StartTheCache(plexSettings, true);
                }
                else
                {
                    await StartTheCache(plexSettings, false);
                }
            }
            catch (Exception e)
            {
                await NotifyClient(recentlyAddedSearch ? "Plex Recently Added Sync Errored" : "Plex Content Sync Errored");
                Logger.LogWarning(LoggingEvents.PlexContentCacher, e, "Exception thrown when attempting to cache the Plex Content");
            }

            if (!recentlyAddedSearch)
            {
                await NotifyClient("Plex Sync - Starting Episode Sync");
                Logger.LogInformation("Starting EP Cacher");
                await OmbiQuartz.TriggerJob(nameof(IPlexEpisodeSync), "Plex");
            }

            if ((processedContent?.HasProcessedContent ?? false) && recentlyAddedSearch)
            {
                await NotifyClient("Plex Sync - Checking if any requests are now available");
                Logger.LogInformation("Kicking off Plex Availability Checker");
                await OmbiQuartz.TriggerJob(nameof(IPlexAvailabilityChecker), "Plex");
            }
            var processedCont = processedContent?.Content?.Count() ?? 0;
            var processedEp = processedContent?.Episodes?.Count() ?? 0;
            Logger.LogInformation("Finished Plex Content Cacher, with processed content: {0}, episodes: {1}. Recently Added Scan: {2}", processedCont, processedEp, recentlyAddedSearch);

            await NotifyClient(recentlyAddedSearch ? $"Plex Recently Added Sync Finished, We processed {processedCont}, and {processedEp} Episodes" : "Plex Content Sync Finished");

        }

        private async Task<ProcessedContent> StartTheCache(PlexSettings plexSettings, bool recentlyAddedSearch)
        {
            var processedContent = new ProcessedContent();
            foreach (var servers in plexSettings.Servers ?? new List<PlexServers>())
            {
                try
                {
                    Logger.LogInformation("Starting to cache the content on server {0}", servers.Name);

                    if (recentlyAddedSearch)
                    {
                        // If it's recently added search then we want the results to pass to the metadata job
                        // This way the metadata job is smaller in size to process, it only need to look at newly added shit
                        return await ProcessServer(servers, true);
                    }
                    else
                    {
                        await ProcessServer(servers, false);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogWarning(LoggingEvents.PlexContentCacher, e, "Exception thrown when attempting to cache the Plex Content in server {0}", servers.Name);
                }
            }

            return processedContent;
        }

        private async Task<ProcessedContent> ProcessServer(PlexServers servers, bool recentlyAddedSearch)
        {
            var retVal = new ProcessedContent();
            var contentProcessed = new Dictionary<int, int>();
            var episodesProcessed = new List<int>();
            Logger.LogDebug("Getting all content from server {0}", servers.Name);
            var allContent = await GetAllContent(servers, recentlyAddedSearch);
            Logger.LogDebug("We found {0} items", allContent.Count);


            // Let's now process this.
            var contentToAdd = new HashSet<PlexServerContent>();

            var allEps = Repo.GetAllEpisodes();

            foreach (var content in allContent)
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
                        var episodesAdded = await EpisodeSync.ProcessEpsiodes(content.Metadata, allEps);
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
                    Logger.LogDebug("Processing Movies");
                    foreach (var movie in content?.Metadata ?? new Metadata[] { })
                    {
                        // Let's check if we have this movie

                        try
                        {
                            var existing = await Repo.GetFirstContentByCustom(x => x.Title == movie.title
                                                                                   && x.ReleaseYear == movie.year.ToString()
                                                                                   && x.Type == PlexMediaTypeEntity.Movie);
                            // The rating key keeps changing
                            //var existing = await Repo.GetByKey(movie.ratingKey);
                            if (existing != null)
                            {
                                Logger.LogDebug("We already have movie {0}", movie.title);
                                continue;
                            }

                            var hasSameKey = await Repo.GetByKey(movie.ratingKey);
                            if (hasSameKey != null)
                            {
                                await Repo.Delete(hasSameKey);
                            }

                            Logger.LogDebug("Adding movie {0}", movie.title);
                            var metaData = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                                movie.ratingKey);

                            var meta = metaData.MediaContainer.Metadata.FirstOrDefault();
                            var guids = new List<string>
                            {
                                meta.guid
                            };
                            if (meta.Guid != null)
                            {
                                foreach (var g in meta.Guid)
                                {
                                    guids.Add(g.Id);
                                }
                            }

                            var providerIds = PlexHelper.GetProviderIdsFromMetadata(guids.ToArray());

                            var item = new PlexServerContent
                            {
                                AddedAt = DateTime.Now,
                                Key = movie.ratingKey,
                                ReleaseYear = movie.year.ToString(),
                                Type = PlexMediaTypeEntity.Movie,
                                Title = movie.title,
                                Url = PlexHelper.GetPlexMediaUrl(servers.MachineIdentifier, movie.ratingKey),
                                Seasons = new List<PlexSeasonsContent>(),
                                Quality = movie.Media?.FirstOrDefault()?.videoResolution ?? string.Empty
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
                    contentProcessed.Add(c.Id, c.Key);
                }
            }

            retVal.Content = contentProcessed.Values;
            retVal.Episodes = episodesProcessed;
            return retVal;
        }

        private async Task ProcessTvShow(PlexServers servers, Metadata show, HashSet<PlexServerContent> contentToAdd, Dictionary<int, int> contentProcessed)
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
                                                                          && x.Type == PlexMediaTypeEntity.Show);

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
                    GetProviderIds(showMetadata, existingContent);

                    await Repo.Update(existingContent);
                }

                // Just check the key
                if (existingKey != null)
                {
                    // The rating key is all good!
                }
                else
                {
                    // This means the rating key has changed somehow.
                    // Should probably delete this and get the new one
                    var oldKey = existingContent.Key;
                    Repo.DeleteWithoutSave(existingContent);

                    // Because we have changed the rating key, we need to change all children too
                    var episodeToChange = Repo.GetAllEpisodes().Where(x => x.GrandparentKey == oldKey);
                    if (episodeToChange.Any())
                    {
                        foreach (var e in episodeToChange)
                        {
                            Repo.DeleteWithoutSave(e);
                        }
                    }

                    await Repo.SaveChangesAsync();
                    existingContent = null;
                }
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
                        Type = PlexMediaTypeEntity.Show,
                        Title = show.title,
                        Url = PlexHelper.GetPlexMediaUrl(servers.MachineIdentifier, show.ratingKey),
                        Seasons = new List<PlexSeasonsContent>()
                    };
                    GetProviderIds(showMetadata, item);

                    // Let's just double check to make sure we do not have it now we have some id's
                    var existingImdb = false;
                    var existingMovieDbId = false;
                    var existingTvDbId = false;
                    if (item.ImdbId.HasValue())
                    {
                        existingImdb = await Repo.GetAll().AnyAsync(x =>
                            x.ImdbId == item.ImdbId && x.Type == PlexMediaTypeEntity.Show);
                    }

                    if (item.TheMovieDbId.HasValue())
                    {
                        existingMovieDbId = await Repo.GetAll().AnyAsync(x =>
                            x.TheMovieDbId == item.TheMovieDbId && x.Type == PlexMediaTypeEntity.Show);
                    }

                    if (item.TvDbId.HasValue())
                    {
                        existingTvDbId = await Repo.GetAll().AnyAsync(x =>
                            x.TvDbId == item.TvDbId && x.Type == PlexMediaTypeEntity.Show);
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

        private static void GetProviderIds(PlexMetadata showMetadata, PlexServerContent existingContent)
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
            var sections = await PlexApi.GetLibrarySections(plexSettings.PlexAuthToken, plexSettings.FullUri);

            var libs = new List<Mediacontainer>();
            if (sections != null)
            {
                foreach (var dir in sections.MediaContainer.Directory ?? new List<Directory>())
                {
                    if (plexSettings.PlexSelectedLibraries.Any())
                    {
                        if (plexSettings.PlexSelectedLibraries.Any(x => x.Enabled))
                        {
                            // Only get the enabled libs
                            var keys = plexSettings.PlexSelectedLibraries.Where(x => x.Enabled)
                                .Select(x => x.Key.ToString()).ToList();
                            if (!keys.Contains(dir.key))
                            {
                                Logger.LogDebug("Lib {0} is not monitored, so skipping", dir.key);
                                // We are not monitoring this lib
                                continue;
                            }
                        }
                    }

                    if (recentlyAddedSearch)
                    {
                        var container = await PlexApi.GetRecentlyAdded(plexSettings.PlexAuthToken, plexSettings.FullUri,
                            dir.key);
                        if (container != null)
                        {
                            libs.Add(container.MediaContainer);
                        }
                    }
                    else
                    {
                        var lib = await PlexApi.GetLibrary(plexSettings.PlexAuthToken, plexSettings.FullUri, dir.key);
                        if (lib != null)
                        {
                            libs.Add(lib.MediaContainer);
                        }
                    }
                }
            }

            return libs;
        }

        private async Task NotifyClient(string message)
        {
            await Notification.Clients.Clients(NotificationHub.AdminConnectionIds)
                .SendAsync(NotificationHub.NotificationEvent, $"Plex Sync - {message}");
        }

        private static bool ValidateSettings(PlexSettings plex)
        {
            if (plex.Enable)
            {
                foreach (var server in plex.Servers ?? new List<PlexServers>())
                {
                    if (string.IsNullOrEmpty(server?.Ip) || string.IsNullOrEmpty(server?.PlexAuthToken))
                    {
                        return false;
                    }
                }
            }
            return plex.Enable;
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