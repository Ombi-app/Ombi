using System;
using System.Threading;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;

namespace Ombi.Helpers
{
    public class CacheService : ICacheService
    {
        protected readonly IMemoryCache _memoryCache;
        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public virtual async Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, DateTimeOffset absoluteExpiration = default)
        {
            if (absoluteExpiration == default)
            {
                absoluteExpiration = DateTimeOffset.Now.AddHours(1);
            }

            return await _memoryCache.GetOrCreateAsync<T>(cacheKey, entry =>
            {
                entry.AbsoluteExpiration = absoluteExpiration;
                return factory();
            });
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public T GetOrAdd<T>(string cacheKey, Func<T> factory, DateTimeOffset absoluteExpiration)
        {
            // locks get and set internally
            return _memoryCache.GetOrCreate<T>(cacheKey, entry =>
            {
                entry.AbsoluteExpiration = absoluteExpiration;
                return factory();
            });
        }

        private static class TypeLock<T>
        {
            public static object Lock { get; } = new object();
        }

    }
}
