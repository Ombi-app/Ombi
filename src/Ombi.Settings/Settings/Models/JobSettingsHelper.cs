using System;
using Ombi.Helpers;
using Quartz;

namespace Ombi.Settings.Settings.Models
{
    public static class JobSettingsHelper
    {
        public static string Radarr(JobSettings s)
        {
            return ValidateCron(Get(s.RadarrSync, Cron.Hourly(15)));
        }

        public static string Sonarr(JobSettings s)
        {
            return ValidateCron(Get(s.SonarrSync, Cron.Hourly(10)));
        }

        public static string EmbyContent(JobSettings s)
        {
            return ValidateCron(Get(s.EmbyContentSync, Cron.Hourly(5)));
        }

        public static string JellyfinContent(JobSettings s)
        {
            return ValidateCron(Get(s.JellyfinContentSync, Cron.Hourly(5)));
        }

        public static string PlexContent(JobSettings s)
        {
            return ValidateCron(Get(s.PlexContentSync, Cron.Daily(2)));
        }

        public static string PlexRecentlyAdded(JobSettings s)
        {
            return ValidateCron(Get(s.PlexRecentlyAddedSync, Cron.MinuteInterval(30)));
        }

        public static string CouchPotato(JobSettings s)
        {
            return ValidateCron(Get(s.CouchPotatoSync, Cron.Hourly(30)));
        }

        public static string Updater(JobSettings s)
        {
            return ValidateCron(Get(s.AutomaticUpdater, Cron.HourInterval(6)));
        }

        public static string UserImporter(JobSettings s)
        {
            return ValidateCron(Get(s.UserImporter, Cron.Daily()));
        }

        public static string Newsletter(JobSettings s)
        {
            return ValidateCron(Get(s.Newsletter, Cron.Weekly(Helpers.DayOfWeek.Friday, 12)));
        }

        public static string SickRageSync(JobSettings s)
        {
            return ValidateCron(Get(s.SickRageSync, Cron.Hourly(35)));
        }

        public static string LidarrArtistSync(JobSettings s)
        {
            return ValidateCron(Get(s.LidarrArtistSync, Cron.Hourly(40)));
        }

        public static string IssuePurge(JobSettings s)
        {
            return ValidateCron(Get(s.IssuesPurge, Cron.Daily()));
        }

        public static string ResendFailedRequests(JobSettings s)
        {
            return ValidateCron(Get(s.RetryRequests, Cron.Daily(6)));
        }

        public static string MediaDatabaseRefresh(JobSettings s)
        {
            return ValidateCron(Get(s.MediaDatabaseRefresh, Cron.DayInterval(5)));
        }

        public static string AutoDeleteRequests(JobSettings s)
        {
            return ValidateCron(Get(s.AutoDeleteRequests, Cron.Daily()));
        }

        private static string Get(string settings, string defaultCron)
        {
            return settings.HasValue() ? settings : defaultCron;
        }

        private const string _defaultCron = "0 0 12 1/1 * ? *";

        private static string ValidateCron(string cron)
        {
            if (CronExpression.IsValidExpression(cron))
            {
                return cron;
            }
            return _defaultCron;
        }
    }
}
