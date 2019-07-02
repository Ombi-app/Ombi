using System;
using Microsoft.Extensions.DependencyInjection;
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
            var scopeFactory = _factory.GetService<IServiceScopeFactory>(); 
            var scope = scopeFactory.CreateScope();
            var scopedContainer = scope.ServiceProvider;

            var implementation = scopedContainer.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            return implementation;
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}