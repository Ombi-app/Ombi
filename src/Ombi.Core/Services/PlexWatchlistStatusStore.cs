using Microsoft.EntityFrameworkCore;
using Ombi.Core.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Services
{
    public class PlexWatchlistStatusStore : IPlexWatchlistStatusStore
    {
        private readonly IRepository<PlexWatchlistUserStatus> _repo;

        public PlexWatchlistStatusStore(IRepository<PlexWatchlistUserStatus> repo)
        {
            _repo = repo;
        }

        public async Task SetAsync(string ombiUserId, WatchlistSyncStatus status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ombiUserId)) return;

            var existing = await _repo.GetAll().FirstOrDefaultAsync(x => x.UserId == ombiUserId, cancellationToken);
            if (existing != null)
            {
                existing.SyncStatus = (int)status;
                existing.LastSyncedAt = DateTime.UtcNow;
                await _repo.SaveChangesAsync();
                return;
            }

            await _repo.Add(new PlexWatchlistUserStatus
            {
                UserId = ombiUserId,
                SyncStatus = (int)status,
                LastSyncedAt = DateTime.UtcNow,
            });
        }

        public async Task<WatchlistSyncStatus?> GetAsync(string ombiUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ombiUserId)) return null;
            var row = await _repo.GetAll().FirstOrDefaultAsync(x => x.UserId == ombiUserId, cancellationToken);
            return row == null ? (WatchlistSyncStatus?)null : (WatchlistSyncStatus)row.SyncStatus;
        }

        public async Task<IReadOnlyDictionary<string, WatchlistSyncStatus>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _repo.GetAll().ToListAsync(cancellationToken);
            return rows.ToDictionary(r => r.UserId, r => (WatchlistSyncStatus)r.SyncStatus);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _repo.GetAll().ToListAsync(cancellationToken);
            if (rows.Count > 0)
            {
                await _repo.DeleteRange(rows);
            }
        }
    }
}
