using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Helpers
{
    public interface ICacheService
    {
        Task<T> GetOrAdd<T>(string cacheKey, Func<Task<T>> factory, DateTime absoluteExpiration = default(DateTime), CancellationToken cancellationToken = default(CancellationToken));
        T GetOrAdd<T>(string cacheKey, Func<T> factory, DateTime absoluteExpiration);
        void Remove(string key);
    }
}