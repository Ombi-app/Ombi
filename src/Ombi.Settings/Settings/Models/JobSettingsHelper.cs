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
            return Get(s.PlexContentSync, Cron.Hourly(20));
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
        public static string SickRageSync(JobSettings s)
        {
            return Get(s.SickRageSync, Cron.Hourly(35));
        }
        public static string RefreshMetadata(JobSettings s)
        {
            return Get(s.RefreshMetadata, Cron.Hourly(40));
        }


        private static string Get(string settings, string defaultCron)
        {
            return settings.HasValue() ? settings : defaultCron;
        }
    }
}