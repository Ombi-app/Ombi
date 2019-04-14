using System;
using Quartz;
using Quartz.Spi;

namespace Ombi.Schedule
{
    public class IoCJobFactory : IJobFactory
    {
        private readonly IServiceProvider _factory;

        public IoCJobFactory(IServiceProvider factory)
        {
            _factory = factory;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _factory.GetService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}