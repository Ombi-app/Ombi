using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.HealthChecks.Checks
{
    public abstract class BaseHealthCheck : IHealthCheck
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public BaseHealthCheck(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public abstract Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default);

        protected IServiceScope CreateScope()
        {
            return _serviceScopeFactory.CreateScope();
        }
    }
}
