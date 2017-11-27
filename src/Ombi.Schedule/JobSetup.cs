using Hangfire;
using Ombi.Core.Settings;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Couchpotato;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Schedule.Jobs.SickRage;
using Ombi.Schedule.Jobs.Sonarr;
using Ombi.Settings.Settings.Models;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentSync plexContentSync, IRadarrSync radarrSync,
            IOmbiAutomaticUpdater updater, IEmbyContentSync embySync, IPlexUserImporter userImporter,
            IEmbyUserImporter embyUserImporter, ISonarrSync cache, ICouchPotatoSync cpCache,
            ISettingsService<JobSettings> jobsettings, ISickRageSync srSync)
        {
            PlexContentSync = plexContentSync;
            RadarrSync = radarrSync;
            Updater = updater;
            EmbyContentSync = embySync;
            PlexUserImporter = userImporter;
            EmbyUserImporter = embyUserImporter;
            SonarrSync = cache;
            CpCache = cpCache;
            JobSettings = jobsettings;
        }

        private IPlexContentSync PlexContentSync { get; }
        private IRadarrSync RadarrSync { get; }
        private IOmbiAutomaticUpdater Updater { get; }
        private IPlexUserImporter PlexUserImporter { get; }
        private IEmbyContentSync EmbyContentSync { get; }
        private IEmbyUserImporter EmbyUserImporter { get; }
        private ISonarrSync SonarrSync { get; }
        private ICouchPotatoSync CpCache { get; }
        private ISickRageSync SrSync { get; }
        private ISettingsService<JobSettings> JobSettings { get; set; }

        public void Setup()
        {
            var s = JobSettings.GetSettings();

            RecurringJob.AddOrUpdate(() => EmbyContentSync.Start(), JobSettingsHelper.EmbyContent(s));
            RecurringJob.AddOrUpdate(() => SonarrSync.Start(), JobSettingsHelper.Sonarr(s));
            RecurringJob.AddOrUpdate(() => RadarrSync.CacheContent(), JobSettingsHelper.Radarr(s));
            RecurringJob.AddOrUpdate(() => PlexContentSync.CacheContent(), JobSettingsHelper.PlexContent(s));
            RecurringJob.AddOrUpdate(() => CpCache.Start(), JobSettingsHelper.CouchPotato(s));
            RecurringJob.AddOrUpdate(() => SrSync.Start(), JobSettingsHelper.SickRageSync(s));

            RecurringJob.AddOrUpdate(() => Updater.Update(null), JobSettingsHelper.Updater(s));

            RecurringJob.AddOrUpdate(() => EmbyUserImporter.Start(), JobSettingsHelper.UserImporter(s));
            RecurringJob.AddOrUpdate(() => PlexUserImporter.Start(), JobSettingsHelper.UserImporter(s));
        }
    }
}
