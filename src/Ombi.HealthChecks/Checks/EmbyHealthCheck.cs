using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ombi.Api.Emby;
using Ombi.Api.Emby.Models;
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
    public class EmbyHealthCheck : BaseHealthCheck
    {
        public EmbyHealthCheck(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }
        public override async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            using (var scope = CreateScope())
            {
                var settingsProvider = scope.ServiceProvider.GetRequiredService<ISettingsService<EmbySettings>>();
                var api = scope.ServiceProvider.GetRequiredService<IEmbyApiFactory>();
                var settings = await settingsProvider.GetSettingsAsync();
                if (settings == null)
                {
                    return HealthCheckResult.Healthy("Emby is not configured.");
                }
                
                var client = api.CreateClient(settings);
                var taskResult = new List<Task<EmbySystemInfo>>();
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
                    return HealthCheckResult.Unhealthy("Could not communicate with Emby", e);
                }
            }
        }
    }
}
