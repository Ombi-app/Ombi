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
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using NLog;
using Ombi.Api;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Store;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Core
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

            // Add the new 'running' item into the scheduled jobs so we can check if the cachers are running
            Db.DbConnection().AlterTable("ScheduledJobs", "ADD", "Running", true, "INTEGER");

            return Db.DbConnection().ConnectionString;
        }

        private void CreateDefaultSettingsPage(string baseUrl)
        {
            var defaultUserSettings = new UserManagementSettings
            {
                RequestMovies = true,
                RequestTvShows = true,
                ReportIssues = true,

            };
            var defaultSettings = new PlexRequestSettings
            {
                SearchForMovies = true,
                SearchForTvShows = true,
                BaseUrl = baseUrl ?? string.Empty,
                CollectAnalyticData = true,
            };
            var ctor = new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider());
            var s = new SettingsServiceV2<PlexRequestSettings>(ctor);
            s.SaveSettings(defaultSettings);

            var userSettings = new SettingsServiceV2<UserManagementSettings>(ctor);
            userSettings.SaveSettings(defaultUserSettings);


            var cron = (Quartz.Impl.Triggers.CronTriggerImpl)CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Friday, 7, 0).Build();
            var scheduled = new ScheduledJobsSettings
            {
                PlexAvailabilityChecker = 60,
                SickRageCacher = 60,
                SonarrCacher = 60,
                CouchPotatoCacher = 60,
                StoreBackup = 24,
                StoreCleanup = 24,
                UserRequestLimitResetter = 12,
                PlexEpisodeCacher = 12,
                RecentlyAddedCron = cron.CronExpressionString, // Weekly CRON at 7 am on Mondays
            };

            var scheduledSettings = new SettingsServiceV2<ScheduledJobsSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            scheduledSettings.SaveSettings(scheduled);
        }

        public void CacheQualityProfiles()
        {
            var mc = new MemoryCacheProvider();

            try
            {
                Task.Run(() => { CacheSonarrQualityProfiles(mc); });
                Task.Run(() => { CacheCouchPotatoQualityProfiles(mc); });
                // we don't need to cache sickrage profiles, those are static
            }
            catch (Exception)
            {
                Log.Error("Failed to cache quality profiles on startup!");
            }
        }

        private void CacheSonarrQualityProfiles(ICacheProvider cacheProvider)
        {
            try
            {
                Log.Info("Executing GetSettings call to Sonarr for quality profiles");
                var sonarrSettingsService = new SettingsServiceV2<SonarrSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), cacheProvider));
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

        private void CacheCouchPotatoQualityProfiles(ICacheProvider cacheProvider)
        {
            try
            {
                Log.Info("Executing GetSettings call to CouchPotato for quality profiles");
                var cpSettingsService = new SettingsServiceV2<CouchPotatoSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), cacheProvider));
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
    }
}
