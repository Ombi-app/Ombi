using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;

namespace Ombi.Helpers
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly AsyncLock _mutex = new AsyncLock();
        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<T> GetOrAdd<T>(string cacheKey, Func<Task<T>> factory, DateTime absoluteExpiration = default(DateTime), CancellationToken cancellationToken = default(CancellationToken))
        {
            if (absoluteExpiration == default(DateTime))
            {
                absoluteExpiration = DateTime.Now.AddHours(1);
            }
            // locks get and set internally
            if (_memoryCache.TryGetValue<T>(cacheKey, out var result))
            {
                return result;
            }

            if (_memoryCache.TryGetValue(cacheKey, out result))
            {
                return result;
            }

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            result = await factory();
            _memoryCache.Set(cacheKey, result, absoluteExpiration);

            return result;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }



        public T GetOrAdd<T>(string cacheKey, Func<T> factory, DateTime absoluteExpiration)
        {
            // locks get and set internally
            if (_memoryCache.TryGetValue<T>(cacheKey, out var result))
            {
                return result;
            }

            lock (TypeLock<T>.Lock)
            {
                if (_memoryCache.TryGetValue(cacheKey, out result))
                {
                    return result;
                }

                result = factory();
                _memoryCache.Set(cacheKey, result, absoluteExpiration);

                return result;
            }
        }

        private static class TypeLock<T>
        {
            public static object Lock { get; } = new object();
        }

    }
}
