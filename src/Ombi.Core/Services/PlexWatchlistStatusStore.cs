using System.Collections.Concurrent;
using System.Collections.Generic;
using Ombi.Core.Models;

namespace Ombi.Core.Services
{
    public class PlexWatchlistStatusStore : IPlexWatchlistStatusStore
    {
        private readonly ConcurrentDictionary<string, WatchlistSyncStatus> _statuses = new();

        public void Set(string ombiUserId, WatchlistSyncStatus status)
        {
            if (string.IsNullOrEmpty(ombiUserId)) return;
            _statuses[ombiUserId] = status;
        }

        public WatchlistSyncStatus? Get(string ombiUserId)
        {
            if (string.IsNullOrEmpty(ombiUserId)) return null;
            return _statuses.TryGetValue(ombiUserId, out var status) ? status : (WatchlistSyncStatus?)null;
        }

        public IReadOnlyDictionary<string, WatchlistSyncStatus> Snapshot() => _statuses;

        public void Clear() => _statuses.Clear();
    }
}
