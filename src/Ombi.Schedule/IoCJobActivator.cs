using System;
using System.Linq;
using System.Reflection;
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
            var scopeFactory = _container.GetService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            var scopedContainer = scope.ServiceProvider;

            var interfaceType = type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault();
            var implementation = scopedContainer.GetRequiredService(interfaceType);
            return implementation;
        }
    }
}