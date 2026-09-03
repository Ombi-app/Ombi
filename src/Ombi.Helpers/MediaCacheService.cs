using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;

namespace Ombi.Helpers
{
    public interface IMediaCacheService
    {
        Task<T> GetOrAddAsync<T>(string cacheKey, System.Func<Task<T>> factory, DateTimeOffset absoluteExpiration = default);
        Task Purge();
    }
    public class MediaCacheService : CacheService, IMediaCacheService
    {
        private const string _cacheKey = "MediaCacheServiceKeys";
        private static readonly object _lock = new object();

        public MediaCacheService(IMemoryCache memoryCache) : base(memoryCache)
        {
        }

        public async override Task<T> GetOrAddAsync<T>(string cacheKey, System.Func<Task<T>> factory, DateTimeOffset absoluteExpiration = default)
        {
            if (absoluteExpiration == default)
            {
                absoluteExpiration = DateTimeOffset.Now.AddHours(1);
            }

            // Keep track of the key so that we know what to remove when we purge
            UpdateLocalCache(cacheKey);

            return await _memoryCache.GetOrCreateAsync<T>(cacheKey, entry =>
            {
                entry.AbsoluteExpiration = absoluteExpiration;
                return factory();
            });
        }

        private void UpdateLocalCache(string cacheKey)
        {
            lock (_lock)
            {
                var mediaServiceCache = _memoryCache.Get<HashSet<string>>(_cacheKey);
                if (mediaServiceCache == null)
                {
                    mediaServiceCache = new HashSet<string>();
                }
                if (!mediaServiceCache.Add(cacheKey))
                {
                    // We are already tracking this key
                    return;
                }
                _memoryCache.Set(_cacheKey, mediaServiceCache);
            }
        }

        public Task Purge()
        {
            lock (_lock)
            {
                var keys = _memoryCache.Get<HashSet<string>>(_cacheKey);
                if (keys == null)
                {
                    return Task.CompletedTask;
                }
                foreach (var key in keys)
                {
                    base.Remove(key);
                }
                // We deliberately keep tracking the keys. A caller can register its key and then
                // be pre-empted by a purge before it has stored the value, forgetting the key here
                // would leave that value in the cache until it expires.
            }
            return Task.CompletedTask;
        }

    }
}
