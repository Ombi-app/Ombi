using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ombi.Api.Lidarr;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.HealthChecks.Checks
{
    public class LidarrHealthCheck : BaseHealthCheck
    {
        public LidarrHealthCheck(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        public override async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using (var scope = CreateScope())
            {
                var settingsProvider = scope.ServiceProvider.GetRequiredService<ISettingsService<LidarrSettings>>();
                var api = scope.ServiceProvider.GetRequiredService<ILidarrApi>();
                var settings = await settingsProvider.GetSettingsAsync();
                if (!settings.Enabled)
                {
                    return HealthCheckResult.Healthy("Lidarr is not configured.");
                }

                try
                {
                    var result = await api.Status(settings.ApiKey, settings.FullUri);
                    if (result != null)
                    {
                        return HealthCheckResult.Healthy();
                    }
                    return HealthCheckResult.Degraded("Couldn't get the status from Lidarr");
                }
                catch (Exception e)
                {
                    return HealthCheckResult.Unhealthy("Could not communicate with Lidarr", e);
                }
            }
        }
    }
}
