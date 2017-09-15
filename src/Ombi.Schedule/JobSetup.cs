using Hangfire;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Schedule.Ombi;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentCacher plexContentCacher, IRadarrCacher radarrCacher,
            IOmbiAutomaticUpdater updater, IEmbyContentCacher embyCacher, IPlexUserImporter userImporter)
        {
            PlexContentCacher = plexContentCacher;
            RadarrCacher = radarrCacher;
            Updater = updater;
            EmbyContentCacher = embyCacher;
            PlexUserImporter = userImporter;
        }

        private IPlexContentCacher PlexContentCacher { get; }
        private IRadarrCacher RadarrCacher { get; }
        private IOmbiAutomaticUpdater Updater { get; }
        private IPlexUserImporter PlexUserImporter { get; }
        private IEmbyContentCacher EmbyContentCacher { get; }

        public void Setup()
        {
            RecurringJob.AddOrUpdate(() => PlexContentCacher.CacheContent(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => EmbyContentCacher.Start(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => RadarrCacher.CacheContent(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => PlexUserImporter.Start(), Cron.Daily);
            RecurringJob.AddOrUpdate(() => Updater.Update(), Cron.Daily);

            //BackgroundJob.Enqueue(() => PlexUserImporter.Start());
        }
    }
}
