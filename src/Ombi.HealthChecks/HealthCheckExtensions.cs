using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ombi.HealthChecks.Checks;
using System;
using System.Collections.Generic;

namespace Ombi.HealthChecks
{
    public static class HealthCheckExtensions
    {
        public static IHealthChecksBuilder AddOmbiHealthChecks(this IHealthChecksBuilder builder)
        {
            builder.AddCheck<PlexHealthCheck>("Plex", tags: new string[] { "MediaServer" });
            builder.AddCheck<EmbyHealthCheck>("Emby", tags: new string[] { "MediaServer" });
            builder.AddCheck<JellyfinHealthCheck>("Jellyfin", tags: new string[] { "MediaServer" });
            builder.AddCheck<LidarrHealthCheck>("Lidarr", tags: new string[] { "DVR" });
            builder.AddCheck<SonarrHealthCheck>("Sonarr", tags: new string[] { "DVR" });
            builder.AddCheck<RadarrHealthCheck>("Radarr", tags: new string[] { "DVR" });
            builder.AddCheck<CouchPotatoHealthCheck>("CouchPotato", tags: new string[] { "DVR" });
            builder.AddCheck<SickrageHealthCheck>("SickRage", tags: new string[] { "DVR" });
            builder.AddOmbiPingHealthCheck(options =>
            {
                options.AddHost("www.google.co.uk", 5000, HealthStatus.Unhealthy);
                options.AddHost("www.google.com", 3000, HealthStatus.Degraded);
            }, "External Ping", tags: new string[] { "System" });

            return builder;
        }

        /// <summary>
        /// Add a health check for network ping.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the ping parameters.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'ping' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns></param>
        public static IHealthChecksBuilder AddOmbiPingHealthCheck(this IHealthChecksBuilder builder, Action<OmbiPingHealthCheckOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var options = new OmbiPingHealthCheckOptions();
            setup?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
               name,
               sp => new OmbiPingHealthCheck(options),
               failureStatus,
               tags,
               timeout));
        }
    }
}
