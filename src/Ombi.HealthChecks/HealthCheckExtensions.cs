using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ombi.HealthChecks
{
    public static class HealthCheckExtensions
    {

        public static IHealthChecksBuilder AddOmbiHealthChecks(this IHealthChecksBuilder builder)
        {
            builder.AddCheck<PlexHealthCheck>("Plex", tags: new string[] { "MediaServer" });

            return builder;
        }
    }
}
