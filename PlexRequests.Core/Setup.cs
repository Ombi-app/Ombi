#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Setup.cs
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

using Mono.Data.Sqlite;
using NLog;
using PlexRequests.Api;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.Store.Repository;
using System.Threading.Tasks;

namespace PlexRequests.Core
{
    public class Setup
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private static DbConfiguration Db { get; set; }
        public string SetupDb(string urlBase)
        {
            Db = new DbConfiguration(new SqliteFactory());
            var created = Db.CheckDb();
            TableCreation.CreateTables(Db.DbConnection());

            if (created)
            {
                CreateDefaultSettingsPage(urlBase);
            }
            
            var version = CheckSchema();
            if (version > 0)
            {
                if (version > 1700 && version <= 1799)
                {
                    MigrateToVersion1700();
                }
            }

            return Db.DbConnection().ConnectionString;
        }

        public static string ConnectionString => Db.DbConnection().ConnectionString;


        private int CheckSchema()
        {
            var checker = new StatusChecker();
            var status = checker.GetStatus();

            var connection = Db.DbConnection();
            var schema = connection.GetSchemaVersion();
            if (schema == null)
            {
                connection.CreateSchema(status.DBVersion); // Set the default.
                schema = connection.GetSchemaVersion();
            }

            var version = schema.SchemaVersion;

            return version;
        }

        private void CreateDefaultSettingsPage(string baseUrl)
        {
            var defaultSettings = new PlexRequestSettings
            {
                RequireTvShowApproval = true,
                RequireMovieApproval = true,
                SearchForMovies = true,
                SearchForTvShows = true,
                WeeklyRequestLimit = 0,
                BaseUrl = baseUrl ?? string.Empty
            };
            var s = new SettingsServiceV2<PlexRequestSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            s.SaveSettings(defaultSettings);
        }

        public void CacheQualityProfiles()
        {
            var mc = new MemoryCacheProvider();

            try
            {
                Task.Run(() => { CacheSonarrQualityProfiles(mc); });
                Task.Run(() => { CacheCouchPotatoQualityProfiles(mc); });
                // we don't need to cache sickrage profiles, those are static
                // TODO: cache headphones profiles?
            }
            catch (Exception)
            {
                Log.Error("Failed to cache quality profiles on startup!");
            }
        }

        private void CacheSonarrQualityProfiles(MemoryCacheProvider cacheProvider)
        {
            try
            {
                Log.Info("Executing GetSettings call to Sonarr for quality profiles");
                var sonarrSettingsService = new SettingsServiceV2<SonarrSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
                var sonarrSettings = sonarrSettingsService.GetSettings();
                if (sonarrSettings.Enabled)
                {
                    Log.Info("Begin executing GetProfiles call to Sonarr for quality profiles");
                    SonarrApi sonarrApi = new SonarrApi();
                    var profiles = sonarrApi.GetProfiles(sonarrSettings.ApiKey, sonarrSettings.FullUri);
                    cacheProvider.Set(CacheKeys.SonarrQualityProfiles, profiles);
                    Log.Info("Finished executing GetProfiles call to Sonarr for quality profiles");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to cache Sonarr quality profiles!");
            }
        }

        private void CacheCouchPotatoQualityProfiles(MemoryCacheProvider cacheProvider)
        {
            try
            {
                Log.Info("Executing GetSettings call to CouchPotato for quality profiles");
                var cpSettingsService = new SettingsServiceV2<CouchPotatoSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
                var cpSettings = cpSettingsService.GetSettings();
                if (cpSettings.Enabled)
                {
                    Log.Info("Begin executing GetProfiles call to CouchPotato for quality profiles");
                    CouchPotatoApi cpApi = new CouchPotatoApi();
                    var profiles = cpApi.GetProfiles(cpSettings.FullUri, cpSettings.ApiKey);
                    cacheProvider.Set(CacheKeys.CouchPotatoQualityProfiles, profiles);
                    Log.Info("Finished executing GetProfiles call to CouchPotato for quality profiles");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to cache CouchPotato quality profiles!");
            }
        }
        public void MigrateToVersion1700()
        {
            // Drop old tables
            TableCreation.DropTable(Db.DbConnection(), "User");
            TableCreation.DropTable(Db.DbConnection(), "Log");
        }
    }
}
