using System;
using Hangfire;
using Ombi.Schedule.Jobs;

namespace Ombi.Schedule
{
    public class JobSetup : IJobSetup
    {
        public JobSetup(IPlexContentCacher cacher)
        {
            Cacher = cacher;
        }

        private IPlexContentCacher Cacher { get; }
        public void Setup()
        {
            RecurringJob.AddOrUpdate(() => Cacher.CacheContent(), Cron.Minutely);
        }
    }
}
