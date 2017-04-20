using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Ombi.Schedule
{
    public class IoCJobActivator : JobActivator
    {
        private readonly IServiceProvider _container;
        public IoCJobActivator(IServiceProvider container)
        {
            _container = container;
        }

        public override object ActivateJob(Type type)
        {
            return _container.GetService(type);
        }
    }
}