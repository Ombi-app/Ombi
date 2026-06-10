using System;
using System.Threading.Tasks;
using Ombi.Helpers;

namespace Ombi.Api.IntegrationTests.Harness
{
    /// <summary>
    /// Replaces the real <see cref="IMediaCacheService"/> in tests so every controller call goes
    /// straight to the (mocked) engine. This keeps each test deterministic and avoids cross-test
    /// cache contamination.
    /// </summary>
    public class PassThroughMediaCacheService : IMediaCacheService
    {
        public Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, DateTimeOffset absoluteExpiration = default)
        {
            return factory();
        }

        public Task Purge() => Task.CompletedTask;
    }
}
