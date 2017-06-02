using System;
using Hangfire;
using Ombi.Schedule.Jobs;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentCacher cacher, IRadarrCacher radarrCache)
        {
            PlexCacher = cacher;
            RadarrCacher = radarrCache;
        }

        private IPlexContentCacher PlexCacher { get; }
        private IRadarrCacher RadarrCacher { get; }
        public void Setup()
        {
            RecurringJob.AddOrUpdate(() => PlexCacher.CacheContent(), Cron.Hourly);
            RecurringJob.AddOrUpdate(() => RadarrCacher.Start(), Cron.MonthInterval(61));
        }
    }
}
