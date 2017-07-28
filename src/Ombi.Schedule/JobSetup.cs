using Hangfire;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Schedule.Ombi;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentCacher cacher, IRadarrCacher radarrCacher, IOmbiAutomaticUpdater updater)
        {
            Cacher = cacher;
            RadarrCacher = radarrCacher;
            Updater = updater;
        }

        private IPlexContentCacher Cacher { get; }
        private IRadarrCacher RadarrCacher { get; }
        private IOmbiAutomaticUpdater Updater { get; }
        public void Setup()
        {
            RecurringJob.AddOrUpdate(() => Cacher.CacheContent(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => RadarrCacher.CacheContent(), Cron.Hourly);
            //RecurringJob.AddOrUpdate(() => Updater.Update(), Cron.Hourly);
        }
    }
}
