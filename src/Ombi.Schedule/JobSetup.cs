using Hangfire;
using Ombi.Schedule.Jobs;
using Ombi.Schedule.Jobs.Radarr;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentCacher cacher, IRadarrCacher radarrCacher)
        {
            Cacher = cacher;
            RadarrCacher = radarrCacher;
        }

        private IPlexContentCacher Cacher { get; }
        private IRadarrCacher RadarrCacher { get; }
        
        public void Setup()
        {
            RecurringJob.AddOrUpdate(() => Cacher.CacheContent(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => RadarrCacher.CacheContent(), Cron.Hourly);
        }
    }
}
