using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LazyCache;

namespace Ombi.Helpers
{
    public interface IMediaCacheService
    {
        Task<T> GetOrAddAsync<T>(string cacheKey, System.Func<Task<T>> factory, DateTimeOffset absoluteExpiration = default);
        Task Purge();
    }
    public class MediaCacheService : CacheService, IMediaCacheService
    {
        private const string CacheKey = "MediaCacheServiceKeys";

        public MediaCacheService(IAppCache memoryCache) : base(memoryCache)
        {
        }

        public async override Task<T> GetOrAddAsync<T>(string cacheKey, System.Func<Task<T>> factory, DateTimeOffset absoluteExpiration = default)
        {
            if (absoluteExpiration == default)
            {
                absoluteExpiration = DateTimeOffset.Now.AddHours(1);
            }

            if (_memoryCache.TryGetValue<T>($"MediaCacheService_{cacheKey}", out var result))
            {
                return (T)result;
            }

            // Not in the cache, so add this Key into our MediaServiceCache
            await UpdateLocalCache(cacheKey);

            return await _memoryCache.GetOrAddAsync<T>(cacheKey, () => factory(), absoluteExpiration);
        }

        private async Task UpdateLocalCache(string cacheKey)
        {
            var mediaServiceCache = await _memoryCache.GetAsync<List<string>>(CacheKey);
            if (mediaServiceCache == null)
            {
                mediaServiceCache = new List<string>();
            }
            mediaServiceCache.Add(cacheKey);
            _memoryCache.Remove(CacheKey);
            _memoryCache.Add(CacheKey, mediaServiceCache);
        }

        public async Task Purge()
        {
            var keys = await _memoryCache.GetAsync<List<string>>(CacheKey);
            foreach (var key in keys)
            {
                base.Remove(key);
            }
        }

    }
}
