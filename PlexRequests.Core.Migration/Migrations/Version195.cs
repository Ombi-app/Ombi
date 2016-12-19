#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Version195.cs
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
using System.Data;
using Ombi.Core.SettingModels;
using Quartz;

namespace Ombi.Core.Migration.Migrations
{
    [Migration(1950, "v1.9.5.0")]
    public class Version195 : BaseMigration, IMigration
    {
        public Version195(ISettingsService<PlexRequestSettings> plexRequestSettings, ISettingsService<NewletterSettings> news, ISettingsService<ScheduledJobsSettings> jobs)
        {
            PlexRequestSettings = plexRequestSettings;
            NewsletterSettings = news;
            Jobs = jobs;
        }
        public int Version => 1950;

        private ISettingsService<PlexRequestSettings> PlexRequestSettings { get; }
        private ISettingsService<NewletterSettings> NewsletterSettings { get; }
        private ISettingsService<ScheduledJobsSettings> Jobs { get; }

        public void Start(IDbConnection con)
        {
            UpdateApplicationSettings();
            UpdateDb(con);

            UpdateSchema(con, Version);
        }

        private void UpdateDb(IDbConnection con)
        {
        }

        private void UpdateApplicationSettings()
        {
            var plex = PlexRequestSettings.GetSettings();
            var jobSettings = Jobs.GetSettings();
            var newsLetter = NewsletterSettings.GetSettings();

            newsLetter.SendToPlexUsers = true;
            UpdateScheduledSettings(jobSettings);

            if (plex.SendRecentlyAddedEmail)
            {
                newsLetter.SendRecentlyAddedEmail = plex.SendRecentlyAddedEmail;
                plex.SendRecentlyAddedEmail = false;
                PlexRequestSettings.SaveSettings(plex);
            }


            NewsletterSettings.SaveSettings(newsLetter);
            Jobs.SaveSettings(jobSettings);
        }

        private void UpdateScheduledSettings(ScheduledJobsSettings settings)
        {
            settings.PlexAvailabilityChecker = 60;
            settings.SickRageCacher = 60;
            settings.SonarrCacher = 60;
            settings.CouchPotatoCacher = 60;
            settings.StoreBackup = 24;
            settings.StoreCleanup = 24;
            settings.UserRequestLimitResetter = 12;
            settings.PlexEpisodeCacher = 12;

            var cron = (Quartz.Impl.Triggers.CronTriggerImpl)CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Friday, 7, 0).Build();
            settings.RecentlyAddedCron = cron.CronExpressionString; // Weekly CRON at 7 am on Mondays
        }
    }
}