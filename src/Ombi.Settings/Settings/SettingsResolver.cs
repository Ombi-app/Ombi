using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Ombi.Core.Settings
{
    public class SettingsResolver : ISettingsResolver
    {
        public SettingsResolver(IServiceProvider services)
        {
            _services = services;
        }

        private readonly IServiceProvider _services;

        public ISettingsService<T> Resolve<T>()
        {
            var service = (ISettingsService<T>)_services.GetService(typeof(ISettingsService<T>));
            return service;;
        }
    }
}