﻿#region Copyright
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
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexContentSync : IPlexContentSync
    {
        public PlexContentSync(ISettingsService<PlexSettings> plex, IPlexApi plexApi, ILogger<PlexContentSync> logger, IPlexContentRepository repo,
            IPlexEpisodeSync epsiodeSync)
        {
            Plex = plex;
            PlexApi = plexApi;
            Logger = logger;
            Repo = repo;
            EpisodeSync = epsiodeSync;
            plex.ClearCache();
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private IPlexApi PlexApi { get; }
        private ILogger<PlexContentSync> Logger { get; }
        private IPlexContentRepository Repo { get; }
        private IPlexEpisodeSync EpisodeSync { get; }

        public async Task CacheContent()
        {
            var plexSettings = await Plex.GetSettingsAsync();
            if (!plexSettings.Enable)
            {
                return;
            }
            if (!ValidateSettings(plexSettings))
            {
                Logger.LogError("Plex Settings are not valid");
                return;
            }

            Logger.LogInformation("Starting Plex Content Cacher");
            try
            {
                await StartTheCache(plexSettings);
            }
            catch (Exception e)
            {
                Logger.LogWarning(LoggingEvents.PlexContentCacher, e, "Exception thrown when attempting to cache the Plex Content");
            }

            Logger.LogInformation("Starting EP Cacher");
            BackgroundJob.Enqueue(() => EpisodeSync.Start());
        }

        private async Task StartTheCache(PlexSettings plexSettings)
        {
            foreach (var servers in plexSettings.Servers ?? new List<PlexServers>())
            {

                Logger.LogInformation("Getting all content from server {0}", servers.Name);
                var allContent = await GetAllContent(servers);
                Logger.LogInformation("We found {0} items", allContent.Count);

                // Let's now process this.
                var contentToAdd = new List<PlexServerContent>();
                foreach (var content in allContent)
                {
                    if (content.viewGroup.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Process Shows
                        Logger.LogInformation("Processing TV Shows");
                        foreach (var show in content.Metadata ?? new Metadata[] { })
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

                            if (existingContent != null)
                            {
                                // Just check the key
                                var existingKey = await Repo.GetByKey(show.ratingKey);
                                if (existingKey != null)
                                {
                                    // The rating key is all good!
                                    existingContent = existingKey;
                                }
                                else
                                {
                                    // This means the rating key has changed somehow.
                                    // We need to reset the correct keys
                                    var oldKey = existingContent.Key;
                                    existingContent.Key = show.ratingKey;

                                    // Because we have changed the rating key, we need to change all children too
                                    var episodeToChange = Repo.GetAllEpisodes().Where(x => x.GrandparentKey == oldKey);
                                    if (episodeToChange.Any())
                                    {
                                        foreach (var e in episodeToChange)
                                        {
                                            e.GrandparentKey = existingContent.Key;
                                        }
                                    }
                                    await Repo.SaveChangesAsync();
                                }

                            }
                            // The ratingKey keeps changing...
                            //var existingContent = await Repo.GetByKey(show.ratingKey);
                            if (existingContent != null)
                            {
                                try
                                {
                                    Logger.LogInformation("We already have show {0} checking for new seasons", existingContent.Title);
                                    // Ok so we have it, let's check if there are any new seasons
                                    var itemAdded = false;
                                    foreach (var season in seasonsContent)
                                    {
                                        var seasonExists = existingContent.Seasons.FirstOrDefault(x => x.SeasonKey == season.SeasonKey);

                                        if (seasonExists != null)
                                        {
                                            // We already have this season
                                            continue;
                                        }

                                        existingContent.Seasons.Add(season);
                                        itemAdded = true;
                                    }

                                    if (itemAdded) await Repo.Update(existingContent);
                                }
                                catch (Exception e)
                                {
                                    Logger.LogError(LoggingEvents.PlexContentCacher, e, "Exception when adding new seasons to title {0}", existingContent.Title);
                                }
                            }
                            else
                            {
                                try
                                {

                                    Logger.LogInformation("New show {0}, so add it", show.title);

                                    // Get the show metadata... This sucks since the `metadata` var contains all information about the show
                                    // But it does not contain the `guid` property that we need to pull out thetvdb id...
                                    var showMetadata = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                                        show.ratingKey);
                                    var providerIds = PlexHelper.GetProviderIdFromPlexGuid(showMetadata.MediaContainer.Metadata.FirstOrDefault().guid);

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
                                    if (providerIds.Type == ProviderType.ImdbId)
                                    {
                                        item.ImdbId = providerIds.ImdbId;
                                    }
                                    if (providerIds.Type == ProviderType.TheMovieDbId)
                                    {
                                        item.TheMovieDbId = providerIds.TheMovieDb;
                                    }
                                    if (providerIds.Type == ProviderType.TvDbId)
                                    {
                                        item.TvDbId = providerIds.TheTvDb;
                                    }

                                    // Let's just double check to make sure we do not have it now we have some id's
                                    var existingImdb = false;
                                    var existingMovieDbId = false;
                                    var existingTvDbId = false;

                                    existingImdb = await Repo.GetAll().AnyAsync(x => x.ImdbId == item.ImdbId && x.Type == PlexMediaTypeEntity.Show);
                                    existingMovieDbId = await Repo.GetAll().AnyAsync(x => x.TheMovieDbId == item.TheMovieDbId && x.Type == PlexMediaTypeEntity.Show);
                                    existingTvDbId = await Repo.GetAll().AnyAsync(x => x.TvDbId == item.TvDbId && x.Type == PlexMediaTypeEntity.Show);

                                    if (existingImdb || existingTvDbId || existingMovieDbId)
                                    {
                                        // We already have it!
                                        continue;
                                    }

                                    item.Seasons.ToList().AddRange(seasonsContent);

                                    contentToAdd.Add(item);

                                }
                                catch (Exception e)
                                {
                                    Logger.LogError(LoggingEvents.PlexContentCacher, e, "Exception when adding tv show {0}", show.title);
                                }

                            }
                        }
                    }
                    if (content.viewGroup.Equals(PlexMediaType.Movie.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        Logger.LogInformation("Processing Movies");
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
                                    Logger.LogInformation("We already have movie {0}", movie.title);
                                    continue;
                                }

                                var hasSameKey = await Repo.GetByKey(movie.ratingKey);
                                if (hasSameKey != null)
                                {
                                    await Repo.Delete(hasSameKey);
                                }

                                Logger.LogInformation("Adding movie {0}", movie.title);
                                var metaData = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                                    movie.ratingKey);
                                var providerIds = PlexHelper.GetProviderIdFromPlexGuid(metaData.MediaContainer.Metadata
                                    .FirstOrDefault()
                                    .guid);

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
                                if (providerIds.Type == ProviderType.ImdbId)
                                {
                                    item.ImdbId = providerIds.ImdbId;
                                }
                                if (providerIds.Type == ProviderType.TheMovieDbId)
                                {
                                    item.TheMovieDbId = providerIds.TheMovieDb;
                                }
                                if (providerIds.Type == ProviderType.TvDbId)
                                {
                                    item.TvDbId = providerIds.TheTvDb;
                                }
                                contentToAdd.Add(item);
                            }
                            catch (Exception e)
                            {
                                Logger.LogError(LoggingEvents.PlexContentCacher, e, "Exception when adding new Movie {0}", movie.title);
                            }
                        }
                    }
                    if (contentToAdd.Count > 500)
                    {
                        await Repo.AddRange(contentToAdd);
                        contentToAdd = new List<PlexServerContent>();
                    }
                }

                if (contentToAdd.Any())
                {
                    await Repo.AddRange(contentToAdd);
                }

            }
        }

        /// <summary>
        /// Gets all the library sections.
        /// If the user has specified only certain libraries then we will only look for those
        /// If they have not set the settings then we will monitor them all
        /// </summary>
        /// <param name="plexSettings">The plex settings.</param>
        /// <returns></returns>
        private async Task<List<Mediacontainer>> GetAllContent(PlexServers plexSettings)
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
                                Logger.LogInformation("Lib {0} is not monitored, so skipping", dir.key);
                                // We are not monitoring this lib
                                continue;
                            }
                        }
                    }
                    var lib = await PlexApi.GetLibrary(plexSettings.PlexAuthToken, plexSettings.FullUri, dir.key);
                    if (lib != null)
                    {
                        libs.Add(lib.MediaContainer);
                    }
                }
            }

            return libs;
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
                Plex?.Dispose();
                Repo?.Dispose();
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