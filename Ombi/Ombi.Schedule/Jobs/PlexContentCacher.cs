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
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;

namespace Ombi.Schedule.Jobs
{
    public partial class PlexContentCacher
    {
        public PlexContentCacher(ISettingsService<PlexSettings> plex, IPlexApi plexApi)
        {
            Plex = plex;
            PlexApi = plexApi;
        }

        private ISettingsService<PlexSettings> Plex { get; }
        private IPlexApi PlexApi { get; }

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
            //TODO
            //var libraries = CachedLibraries(plexSettings);

            //if (libraries == null || !libraries.Any())
            //{
            //    return;
            //}
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

        //            var providerId = PlexHelper.GetProviderIdFromPlexGuid(metaData.MediaContainer.);
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

        private List<PlexLibraries> GetLibraries(PlexSettings plexSettings)
        {
            var sections = PlexApi.GetLibrarySections(plexSettings.PlexAuthToken, plexSettings.FullUri).Result;
            
            var libs = new List<PlexLibraries>();
            if (sections != null)
            {
                foreach (var dir in sections.MediaContainer.Directory ?? new List<Directory>())
                {
                    var lib = PlexApi.GetLibrary(plexSettings.PlexAuthToken, plexSettings.FullUri, dir.key).Result;
                    if (lib != null)
                    {
                        libs.Add(lib);
                    }
                }
            }

            return libs;
        }



        private static bool ValidateSettings(PlexSettings plex)
        {
            if (plex.Enable)
            {
                if (plex?.Ip == null || plex?.PlexAuthToken == null)
                {
                    return false;
                }
            }
            return plex.Enable;
        }
    }
}