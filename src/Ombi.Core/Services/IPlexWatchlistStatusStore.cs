using System.Collections.Generic;
using Ombi.Core.Models;

namespace Ombi.Core.Services
{
    public interface IPlexWatchlistStatusStore
    {
        void Set(string ombiUserId, WatchlistSyncStatus status);
        WatchlistSyncStatus? Get(string ombiUserId);
        IReadOnlyDictionary<string, WatchlistSyncStatus> Snapshot();
        void Clear();
    }
}
