using HealthChecks.Network;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.HealthChecks.Checks
{
   public class OmbiPingHealthCheck
        : IHealthCheck
    {
        private readonly OmbiPingHealthCheckOptions _options;
        public OmbiPingHealthCheck(OmbiPingHealthCheckOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var configuredHosts = _options.ConfiguredHosts.Values;

            try
            {
                foreach (var (host, timeout, status) in configuredHosts)
                {
                    using (var ping = new Ping())
                    {
                        var pingReply = await ping.SendPingAsync(host, timeout);

                        if (pingReply.Status != IPStatus.Success)
                        {
                            return new HealthCheckResult(status, description: $"Ping check for host {host} is failed with status reply:{pingReply.Status}");
                        }
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
