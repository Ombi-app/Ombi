using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Helpers
{
    public interface ICacheService
    {
        Task<T> GetOrAddAsync<T>(string cacheKey, Func<Task<T>> factory, DateTimeOffset absoluteExpiration = default);
        T GetOrAdd<T>(string cacheKey, Func<T> factory, DateTimeOffset absoluteExpiration);
        void Remove(string key);
    }
}