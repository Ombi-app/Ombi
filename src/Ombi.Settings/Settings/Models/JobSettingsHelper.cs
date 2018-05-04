using System;
using Ombi.Helpers;

namespace Ombi.Settings.Settings.Models
{
    public static class JobSettingsHelper
    {
        public static string Radarr(JobSettings s)
        {
            return Get(s.RadarrSync, Cron.Hourly(15));
        }

        public static string Sonarr(JobSettings s)
        {
            return Get(s.SonarrSync, Cron.Hourly(10));
        }

        public static string EmbyContent(JobSettings s)
        {
            return Get(s.EmbyContentSync, Cron.Hourly(5));
        }
        public static string PlexContent(JobSettings s)
        {
            return Get(s.PlexContentSync, Cron.Daily(2));
        }
        public static string PlexRecentlyAdded(JobSettings s)
        {
            return Get(s.PlexRecentlyAddedSync, Cron.Hourly(0));
        }
        public static string CouchPotato(JobSettings s)
        {
            return Get(s.CouchPotatoSync, Cron.Hourly(30));
        }

        public static string Updater(JobSettings s)
        {
            return Get(s.AutomaticUpdater, Cron.HourInterval(6));
        }
        public static string UserImporter(JobSettings s)
        {
            return Get(s.UserImporter, Cron.Daily());
        }
        public static string Newsletter(JobSettings s)
        {
            return Get(s.Newsletter, Cron.Weekly(DayOfWeek.Friday, 12));
        }
        public static string SickRageSync(JobSettings s)
        {
            return Get(s.SickRageSync, Cron.Hourly(35));
        }
        public static string RefreshMetadata(JobSettings s)
        {
            return Get(s.RefreshMetadata, Cron.DayInterval(2));
        }

        private static string Get(string settings, string defaultCron)
        {
            return settings.HasValue() ? settings : defaultCron;
        }
    }
}