#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: DatabaseConfiguration.cs
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
using Mono.Data.Sqlite;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.Store.Repository;

namespace PlexRequestes.Automation.Helpers
{
    public static class DatabaseConfiguration
    {
        private static SettingsJsonRepository _jsonRepository = new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider());

        public static void ResetDatabase()
        {
            var defaultSettings = new PlexRequestSettings
            {
                RequireTvShowApproval = true,
                RequireMovieApproval = true,
                SearchForMovies = true,
                SearchForTvShows = true,
                BaseUrl = string.Empty,
                CollectAnalyticData = true,
            };
            UpdateSettings(defaultSettings);

            LandingPageSettings lp = null;
            PlexSettings plexSettings = null;
            SonarrSettings sonarr = null;
            CouchPotatoSettings cp = null;
            SickRageSettings sr = null;
            UpdateSettings(lp);
            UpdateSettings(plexSettings);
            UpdateSettings(sonarr);
            UpdateSettings(cp);
            UpdateSettings(sr);


        }


        public static void UpdateSettings<T>(T settings) where T : Settings, new()
        {
            var service = new SettingsServiceV2<T>(_jsonRepository);
            if (settings == null)
            {
                var existing = service.GetSettings();
                service.Delete(existing);
                return;
            }
            service.SaveSettings(settings);
        }
    }
}