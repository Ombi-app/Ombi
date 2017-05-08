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
using Microsoft.Extensions.Logging;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs
{
    public partial class PlexContentCacher : IPlexContentCacher
    {
        public PlexContentCacher(ISettingsService<PlexSettings> plex, IPlexApi plexApi, ILogger<PlexContentCacher> logger, IPlexContentRepository repo)
        {
            Plex = plex;
            PlexApi = plexApi;
            Logger = logger;
            Repo = repo;
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private IPlexApi PlexApi { get; }
        private ILogger<PlexContentCacher> Logger { get; }
        private IPlexContentRepository Repo { get; }

        public void CacheContent()
        {
            var plexSettings = Plex.GetSettings();
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

                StartTheCache(plexSettings).Wait();
            }
            catch (Exception e) { 
                Logger.LogWarning(LoggingEvents.CacherException, e, "Exception thrown when attempting to cache the Plex Content");
            }
        }
        //private List<PlexLibraries> CachedLibraries(PlexSettings plexSettings)
        //{
        //    var results = new List<PlexLibraries>();

        //    results = GetLibraries(plexSettings);
        //    foreach (PlexLibraries t in results)
        //    {
        //        foreach (var t1 in t.MediaContainer.Directory)
        //        {
        //            var currentItem = t1;
        //            var metaData = PlexApi.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
        //                currentItem.ratingKey).Result;

        //            // Get the seasons for each show
        //            if (currentItem.type.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase))
        //            {
        //                var seasons = PlexApi.GetSeasons(plexSettings.PlexAuthToken, plexSettings.FullUri,
        //                    currentItem.ratingKey).Result;

        //                // We do not want "all episodes" this as a season
        //                var filtered = seasons.MediaContainer.Directory.Where(x => !x.title.Equals("All episodes", StringComparison.CurrentCultureIgnoreCase));

        //                t1.seasons.AddRange(filtered);
        //            }

        //            var providerId = PlexHelper.GetProviderIdFromPlexGuid(metaData.MediaContainer);
        //            t1.providerId = providerId;
        //        }
        //        foreach (Video t1 in t.Video)
        //        {
        //            var currentItem = t1;
        //            var metaData = PlexApi.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
        //                currentItem.RatingKey);
        //            var providerId = PlexHelper.GetProviderIdFromPlexGuid(metaData.Video.Guid);
        //            t1.ProviderId = providerId;
        //        }
        //    }

        //}

        private async Task StartTheCache(PlexSettings plexSettings)
        {
            var allContent = GetAllContent(plexSettings);

            // Let's now process this.

            var contentToAdd = new List<PlexContent>();
            foreach (var content in allContent)
            {
                if (content.viewGroup.Equals(PlexMediaType.Show.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    // Process Shows
                    foreach (var metadata in content.Metadata)
                    {
                        var seasonList = await PlexApi.GetSeasons(plexSettings.PlexAuthToken, plexSettings.FullUri,
                            metadata.ratingKey);
                        var seasonsContent = new List<SeasonsContent>();
                        foreach (var season in seasonList.MediaContainer.Metadata)
                        {
                            seasonsContent.Add(new SeasonsContent
                            {
                                ParentKey = int.Parse(season.parentRatingKey),
                                SeasonKey = int.Parse(season.ratingKey),
                                SeasonNumber = season.index
                            });
                        }

                        // Do we already have this item?
                        var existingContent = await Repo.GetByKey(metadata.key);
                        if (existingContent != null)
                        {
                            // Ok so we have it, let's check if there are any new seasons
                            var seasonDifference = seasonsContent.Except(existingContent.Seasons).ToList();
                            if (seasonDifference.Any())
                            {
                                // We have new seasons on Plex, let's add them back into the entity
                                existingContent.Seasons.AddRange(seasonDifference);
                                await Repo.Update(existingContent);
                                continue;
                            }
                            else
                            {
                                // No changes, no need to do anything
                                continue;
                            }
                        }

                        // Get the show metadata... This sucks since the `metadata` var contains all information about the show
                        // But it does not contain the `guid` property that we need to pull out thetvdb id...
                        var showMetadata = await PlexApi.GetMetadata(plexSettings.PlexAuthToken, plexSettings.FullUri,
                            metadata.ratingKey);
                        var item = new PlexContent
                        {
                            AddedAt = DateTime.Now,
                            Key = metadata.ratingKey,
                            ProviderId = PlexHelper.GetProviderIdFromPlexGuid(showMetadata.MediaContainer.Metadata
                                .FirstOrDefault()
                                .guid),
                            ReleaseYear = metadata.year.ToString(),
                            Type = PlexMediaTypeEntity.Show,
                            Title = metadata.title,
                            Url = PlexHelper.GetPlexMediaUrl(plexSettings.MachineIdentifier, metadata.ratingKey),
                            Seasons = new List<SeasonsContent>()
                        };

                       
                        item.Seasons.AddRange(seasonsContent);

                        contentToAdd.Add(item);
                    }
                }
            }

            if (contentToAdd.Any())
            {
                await Repo.AddRange(contentToAdd);
            }
        }

        private List<Mediacontainer> GetAllContent(PlexSettings plexSettings)
        {
            var sections = PlexApi.GetLibrarySections(plexSettings.PlexAuthToken, plexSettings.FullUri).Result;
            
            var libs = new List<Mediacontainer>();
            if (sections != null)
            {
                foreach (var dir in sections.MediaContainer.Directory ?? new List<Directory>())
                {
                    if (plexSettings.PlexSelectedLibraries.Any())
                    {
                        // Only get the enabled libs
                        var keys = plexSettings.PlexSelectedLibraries.Where(x => x.Enabled).Select(x => x.Key.ToString()).ToList();
                        if (!keys.Contains(dir.key))
                        {
                            // We are not monitoring this lib
                            continue;
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
                if (string.IsNullOrEmpty(plex?.Ip) || string.IsNullOrEmpty(plex?.PlexAuthToken))
                {
                    return false;
                }
            }
            return plex.Enable;
        }
    }
}