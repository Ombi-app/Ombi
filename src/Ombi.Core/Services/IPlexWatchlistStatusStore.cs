using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Core.Models;

namespace Ombi.Core.Services
{
    public interface IPlexWatchlistStatusStore
    {
        Task SetAsync(string ombiUserId, WatchlistSyncStatus status, CancellationToken cancellationToken = default);
        Task<WatchlistSyncStatus?> GetAsync(string ombiUserId, CancellationToken cancellationToken = default);
        Task<IReadOnlyDictionary<string, WatchlistSyncStatus>> GetAllAsync(CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
