#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: PlexContentCacher.cs
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
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Plex
{
    public class PlexContentCacher : IPlexContentCacher
    {
        public PlexContentCacher(ISettingsService<PlexSettings> plex, IPlexApi plexApi, ILogger<PlexContentCacher> logger, IPlexContentRepository repo,
            IPlexEpisodeCacher epsiodeCacher)
        {
            Plex = plex;
            PlexApi = plexApi;
            Logger = logger;
            Repo = repo;
            EpisodeCacher = epsiodeCacher;
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private IPlexApi PlexApi { get; }
        private ILogger<PlexContentCacher> Logger { get; }
        private IPlexContentRepository Repo { get; }
        private IPlexEpisodeCacher EpisodeCacher { get; }

        public async Task CacheContent()
        {
            var plexSettings = await Plex.GetSettingsAsync();
            if (!plexSettings.Enable)
            {
                return;
            }
            if (!ValidateSettings(plexSettings))
            {
                return;
            }

            Logger.LogInformation("Starting Plex Content Cacher");
            try
            {
                await StartTheCache(plexSettings);

                BackgroundJob.Enqueue(() => EpisodeCacher.Start());
            }
            catch (Exception e)
            {
                Logger.LogWarning(LoggingEvents.Cacher, e, "Exception thrown when attempting to cache the Plex Content");
            }
        }

        private async Task StartTheCache(PlexSettings plexSettings)
        {
            foreach (var servers in plexSettings.Servers ?? new List<PlexServers>())
            {

                var allContent = GetAllContent(servers);

                // Let's now process this.
                var contentToAdd = new List<PlexContent>();
                foreach (var content in allContent)
                {
                    if (content.viewGroup.Equals(Jobs.PlexContentCacher.PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Process Shows
                        foreach (var show in content.Metadata)
                        {
                            var seasonList = await PlexApi.GetSeasons(servers.PlexAuthToken, servers.FullUri,
                                show.ratingKey);
                            var seasonsContent = new List<PlexSeasonsContent>();
                            foreach (var season in seasonList.MediaContainer.Metadata)
                            {
                                seasonsContent.Add(new PlexSeasonsContent
                                {
                                    ParentKey = int.Parse(season.parentRatingKey),
                                    SeasonKey = int.Parse(season.ratingKey),
                                    SeasonNumber = season.index,
                                    PlexContentId = int.Parse(show.ratingKey)
                                });
                            }

                            // Do we already have this item?
                            var existingContent = await Repo.GetByKey(show.ratingKey);
                            if (existingContent != null)
                            {
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
                            else
                            {

                                // Get the show metadata... This sucks since the `metadata` var contains all information about the show
                                // But it does not contain the `guid` property that we need to pull out thetvdb id...
                                var showMetadata = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                                    show.ratingKey);
                                var item = new PlexContent
                                {
                                    AddedAt = DateTime.Now,
                                    Key = show.ratingKey,
                                    ProviderId = PlexHelper.GetProviderIdFromPlexGuid(showMetadata.MediaContainer.Metadata
                                        .FirstOrDefault()
                                        .guid),
                                    ReleaseYear = show.year.ToString(),
                                    Type = PlexMediaTypeEntity.Show,
                                    Title = show.title,
                                    Url = PlexHelper.GetPlexMediaUrl(servers.MachineIdentifier, show.ratingKey),
                                    Seasons = new List<PlexSeasonsContent>()
                                };

                                item.Seasons.ToList().AddRange(seasonsContent);

                                contentToAdd.Add(item);
                            }
                        }
                    }
                    if (content.viewGroup.Equals(Jobs.PlexContentCacher.PlexMediaType.Movie.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (var movie in content.Metadata)
                        {
                            // Let's check if we have this movie
                            var existing = await Repo.GetByKey(movie.ratingKey);
                            if(existing != null)
                            {
                                continue;
                            }

                            var metaData = await PlexApi.GetMetadata(servers.PlexAuthToken, servers.FullUri,
                                movie.ratingKey);
                            var item = new PlexContent
                            {
                                AddedAt = DateTime.Now,
                                Key = movie.ratingKey,
                                ProviderId = PlexHelper.GetProviderIdFromPlexGuid(metaData.MediaContainer.Metadata
                                .FirstOrDefault()
                                .guid),
                                ReleaseYear = movie.year.ToString(),
                                Type = PlexMediaTypeEntity.Movie,
                                Title = movie.title,
                                Url = PlexHelper.GetPlexMediaUrl(servers.MachineIdentifier, movie.ratingKey),
                                Seasons = new List<PlexSeasonsContent>()
                            };

                            contentToAdd.Add(item);
                        }
                    }
                }

                if (contentToAdd.Any())
                {
                    
                    contentToAdd.ForEach(async x => await Repo.Add(x));
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
        private List<Mediacontainer> GetAllContent(PlexServers plexSettings)
        {
            var sections = PlexApi.GetLibrarySections(plexSettings.PlexAuthToken, plexSettings.FullUri).Result;

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
                                // We are not monitoring this lib
                                continue;
                            }
                        }
                    }
                    var lib = PlexApi.GetLibrary(plexSettings.PlexAuthToken, plexSettings.FullUri, dir.key).Result;
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
    }
}