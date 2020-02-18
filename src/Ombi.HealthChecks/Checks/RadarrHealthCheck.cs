using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ombi.Api.Radarr;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.HealthChecks.Checks
{
    public class RadarrHealthCheck : BaseHealthCheck, IHealthCheck
    {
        public RadarrHealthCheck(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }
        public override async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using (var scope = CreateScope())
            {
                var settingsProvider = scope.ServiceProvider.GetRequiredService<ISettingsService<RadarrSettings>>();
                var api = scope.ServiceProvider.GetRequiredService<IRadarrApi>();
                var settings = await settingsProvider.GetSettingsAsync();
                if (!settings.Enabled)
                {
                    return HealthCheckResult.Healthy("Radarr is not configured.");
                }

                try
                {
                    var result = await api.SystemStatus(settings.ApiKey, settings.FullUri);
                    if (result != null)
                    {
                        return HealthCheckResult.Healthy();
                    }
                    return HealthCheckResult.Degraded("Couldn't get the status from Radarr");
                }
                catch (Exception e)
                {
                    return HealthCheckResult.Unhealthy("Could not communicate with Radarr", e);
                }
            }
        }
    }
}
