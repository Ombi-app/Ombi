using Hangfire;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Couchpotato;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Schedule.Jobs.Sonarr;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentSync plexContentSync, IRadarrCacher radarrCacher,
            IOmbiAutomaticUpdater updater, IEmbyContentSync embySync, IPlexUserImporter userImporter,
            IEmbyUserImporter embyUserImporter, ISonarrCacher cache, ICouchPotatoCacher cpCache)
        {
            PlexContentSync = plexContentSync;
            RadarrCacher = radarrCacher;
            Updater = updater;
            EmbyContentSync = embySync;
            PlexUserImporter = userImporter;
            EmbyUserImporter = embyUserImporter;
            SonarrCacher = cache;
            CpCache = cpCache;
        }

        private IPlexContentSync PlexContentSync { get; }
        private IRadarrCacher RadarrCacher { get; }
        private IOmbiAutomaticUpdater Updater { get; }
        private IPlexUserImporter PlexUserImporter { get; }
        private IEmbyContentSync EmbyContentSync { get; }
        private IEmbyUserImporter EmbyUserImporter { get; }
        private ISonarrCacher SonarrCacher { get; }
        private ICouchPotatoCacher CpCache { get; }

        public void Setup()
        {
            RecurringJob.AddOrUpdate(() => EmbyContentSync.Start(), Cron.Hourly(5));
            RecurringJob.AddOrUpdate(() => SonarrCacher.Start(), Cron.Hourly(10));
            RecurringJob.AddOrUpdate(() => RadarrCacher.CacheContent(), Cron.Hourly(15));
            RecurringJob.AddOrUpdate(() => PlexContentSync.CacheContent(), Cron.Hourly(20));
            RecurringJob.AddOrUpdate(() => CpCache.Start(), Cron.Hourly(30));

            RecurringJob.AddOrUpdate(() => Updater.Update(null), Cron.HourInterval(6));

            RecurringJob.AddOrUpdate(() => EmbyUserImporter.Start(), Cron.Daily);
            RecurringJob.AddOrUpdate(() => PlexUserImporter.Start(), Cron.Daily(5));
        }
    }
}
