using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ombi.Api.Jellyfin;
using Ombi.Api.Jellyfin.Models;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models.Status;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.HealthChecks.Checks
{
    public class JellyfinHealthCheck : BaseHealthCheck
    {
        public JellyfinHealthCheck(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }
        public override async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using (var scope = CreateScope())
            {
                var settingsProvider = scope.ServiceProvider.GetRequiredService<ISettingsService<JellyfinSettings>>();
                var api = scope.ServiceProvider.GetRequiredService<IJellyfinApiFactory>();
                var settings = await settingsProvider.GetSettingsAsync();
                if (settings == null)
                {
                    return HealthCheckResult.Healthy("Jellyfin is not configured.");
                }
                
                var client = api.CreateClient(settings);
                var taskResult = new List<Task<JellyfinSystemInfo>>();
                foreach (var server in settings.Servers)
                {
                    taskResult.Add(client.GetSystemInformation(server.ApiKey, server.FullUri));
                }

                try
                {
                    var result = await Task.WhenAll(taskResult.ToArray());
                    return HealthCheckResult.Healthy();
                }
                catch (Exception e)
                {
                    return HealthCheckResult.Unhealthy("Could not communicate with Jellyfin", e);
                }
            }
        }
    }
}
