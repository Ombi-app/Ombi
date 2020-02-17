using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models.Status;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.HealthChecks
{
    public class PlexHealthCheck : IHealthCheck
    {
        private readonly IPlexApi _plexApi;
        private readonly ISettingsService<PlexSettings> _settings;
        public PlexHealthCheck(IPlexApi plexApi, ISettingsService<PlexSettings> settings)
        {
            _plexApi = plexApi;
            _settings = settings;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var settings = await _settings.GetSettingsAsync();
            if (settings == null)
            {
                return HealthCheckResult.Healthy("Plex is not confiured.");
            }

            var taskResult = new List<Task<PlexStatus>>();
            foreach (var server in settings.Servers)
            {
                taskResult.Add(_plexApi.GetStatus(server.PlexAuthToken, server.FullUri));
            }

            try
            {
                var result = await Task.WhenAll(taskResult.ToArray());
                return HealthCheckResult.Healthy();
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy("Could not communicate with Plex", e);
            }
        }
    }
}
