using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ombi.Api.CouchPotato;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models;
using Ombi.Api.Jellyfin;
using Ombi.Api.Jellyfin.Models;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models.Status;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Settings.Settings.Models.External;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.HealthChecks.Checks
{
    public class CouchPotatoHealthCheck : BaseHealthCheck
    {
        public CouchPotatoHealthCheck(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }
        public override async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using (var scope = CreateScope())
            {
                var settingsProvider = scope.ServiceProvider.GetRequiredService<ISettingsService<CouchPotatoSettings>>();
                var api = scope.ServiceProvider.GetRequiredService<ICouchPotatoApi>();
                var settings = await settingsProvider.GetSettingsAsync();
                                if (!settings.Enabled)
                {
                    return HealthCheckResult.Healthy("CouchPotato is not configured.");
                }

                try
                {
                    var result = await api.Status(settings.ApiKey, settings.FullUri);
                    if (result != null)
                    {
                        return HealthCheckResult.Healthy();
                    }
                    return HealthCheckResult.Degraded("Couldn't get the status from CouchPotato");
                }
                catch (Exception e)
                {
                    return HealthCheckResult.Unhealthy("Could not communicate with CouchPotato", e);
                }
            }
        }
    }
}
