using Hangfire;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Schedule.Ombi;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentCacher plexContentCacher, IRadarrCacher radarrCacher,
            IOmbiAutomaticUpdater updater, IEmbyContentCacher embyCacher)
        {
            PlexContentCacher = plexContentCacher;
            RadarrCacher = radarrCacher;
            Updater = updater;
            EmbyContentCacher = embyCacher;
        }

        private IPlexContentCacher PlexContentCacher { get; }
        private IRadarrCacher RadarrCacher { get; }
        private IOmbiAutomaticUpdater Updater { get; }
        private IEmbyContentCacher EmbyContentCacher { get; }

        public void Setup()
        {
            RecurringJob.AddOrUpdate(() => PlexContentCacher.CacheContent(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => EmbyContentCacher.Start(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => RadarrCacher.CacheContent(), Cron.Hourly);
            //RecurringJob.AddOrUpdate(() => Updater.Update(), Cron.Daily);

            BackgroundJob.Enqueue(() => Updater.Update());
        }
    }
}
