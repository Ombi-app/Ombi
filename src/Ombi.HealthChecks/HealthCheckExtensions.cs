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

            return builder;
        }
    }
}
