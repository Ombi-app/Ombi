using System;
using System.Linq;
using System.Reflection;
using Hangfire;

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
            var i = type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault();
            return _container.GetService(i);
        }
    }
}