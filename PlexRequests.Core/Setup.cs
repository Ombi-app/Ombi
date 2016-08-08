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
using System.Text.RegularExpressions;

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
            else
            {
                // Shrink DB
                TableCreation.Vacuum(Db.DbConnection());
            }

            var version = CheckSchema();
            if (version > 0)
            {
                if (version > 1799 && version <= 1800)
                {
                    MigrateToVersion1800();
                }
                if (version > 1899 && version <= 1900)
                {
                    MigrateToVersion1900();
                }
            }

            return Db.DbConnection().ConnectionString;
        }

        public static string ConnectionString => Db.DbConnection().ConnectionString;


        private int CheckSchema()
        {
            var productVersion = AssemblyHelper.GetProductVersion();
            var trimStatus = new Regex("[^0-9]", RegexOptions.Compiled).Replace(productVersion, string.Empty).PadRight(4, '0');
            var version = int.Parse(trimStatus);

            var connection = Db.DbConnection();
            var schema = connection.GetSchemaVersion();
            if (schema == null)
            {
                connection.CreateSchema(version); // Set the default.
                schema = connection.GetSchemaVersion();
            }
            if (version > schema.SchemaVersion)
            {
                Db.DbConnection().UpdateSchemaVersion(version);
                schema = connection.GetSchemaVersion();
            }
            version = schema.SchemaVersion;

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
                BaseUrl = baseUrl ?? string.Empty,
                CollectAnalyticData = true,
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
        public void MigrateToVersion1900()
        {
            // Need to change the Plex Token location
            var authSettings = new SettingsServiceV2<AuthenticationSettings>(new SettingsJsonRepository(Db, new MemoryCacheProvider()));
            var auth = authSettings.GetSettings();
            var plexSettings = new SettingsServiceV2<PlexSettings>(new SettingsJsonRepository(Db, new MemoryCacheProvider()));

            var currentSettings = plexSettings.GetSettings();
            if (!string.IsNullOrEmpty(auth.OldPlexAuthToken))
            {
                currentSettings.PlexAuthToken = auth.OldPlexAuthToken;
                plexSettings.SaveSettings(currentSettings);

                // Clear out the old value
                auth.OldPlexAuthToken = string.Empty;
                authSettings.SaveSettings(auth);
            }
        }

        /// <summary>
        /// Migrates to version 1.8.
        /// <para>This includes updating the admin account to have all roles.</para>
        /// <para>Set the log level to Error</para>
        /// <para>Enable Analytics by default</para>
        /// </summary>
        private void MigrateToVersion1800()
        {

            // Give admin all roles/claims
            try
            {
                var userMapper = new UserMapper(new UserRepository<UsersModel>(Db, new MemoryCacheProvider()));
                var users = userMapper.GetUsers();

                foreach (var u in users)
                {
                    var claims = new[] { UserClaims.User, UserClaims.Admin, UserClaims.PowerUser };
                    u.Claims = ByteConverterHelper.ReturnBytes(claims);

                    userMapper.EditUser(u);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }


            // Set log level
            try
            {
                var settingsService = new SettingsServiceV2<LogSettings>(new SettingsJsonRepository(Db, new MemoryCacheProvider()));
                var logSettings = settingsService.GetSettings();
                logSettings.Level = LogLevel.Error.Ordinal;
                settingsService.SaveSettings(logSettings);

                LoggingHelper.ReconfigureLogLevel(LogLevel.FromOrdinal(logSettings.Level));

            }
            catch (Exception e)
            {
                Log.Error(e);
            }


            // Enable analytics;
            try
            {

                var prSettings = new SettingsServiceV2<PlexRequestSettings>(new SettingsJsonRepository(Db, new MemoryCacheProvider()));
                var settings = prSettings.GetSettings();
                settings.CollectAnalyticData = true;
                var updated = prSettings.SaveSettings(settings);

            }
            catch (Exception e)
            {
                Log.Error(e);
            }

        }
    }
}
