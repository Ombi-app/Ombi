using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Generic;

namespace Ombi.HealthChecks.Checks
{
    public class OmbiPingHealthCheckOptions
    {
        internal Dictionary<string, (string Host, int TimeOut, HealthStatus status)> ConfiguredHosts { get; } = new Dictionary<string, (string, int, HealthStatus)>();

        public OmbiPingHealthCheckOptions AddHost(string host, int timeout, HealthStatus status)
        {
            ConfiguredHosts.Add(host, (host, timeout, status));
            return this;
        }
    }
}
