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

            try
            {
                await _repo.Add(new PlexWatchlistUserStatus
                {
                    UserId = ombiUserId,
                    SyncStatus = (int)status,
                    LastSyncedAt = DateTime.UtcNow,
                });
            }
            catch (DbUpdateException)
            {
                // Unique index raced with a concurrent insert — fetch the row we lost to and update it.
                var row = await _repo.GetAll().FirstOrDefaultAsync(x => x.UserId == ombiUserId, cancellationToken);
                if (row == null) throw;
                row.SyncStatus = (int)status;
                row.LastSyncedAt = DateTime.UtcNow;
                await _repo.SaveChangesAsync();
            }
        }

        public async Task<WatchlistSyncStatus?> GetAsync(string ombiUserId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ombiUserId)) return null;
            var rows = await _repo.GetAll().Where(x => x.UserId == ombiUserId).ToListAsync(cancellationToken);
            if (rows.Count == 0) return null;
            return (WatchlistSyncStatus)rows.OrderByDescending(r => r.LastSyncedAt).First().SyncStatus;
        }

        public async Task<IReadOnlyDictionary<string, WatchlistSyncStatus>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _repo.GetAll().ToListAsync(cancellationToken);
            // Tolerate duplicate rows (shouldn't happen with the unique index, but keeps the admin UI from
            // throwing if legacy duplicates ever exist).
            return rows
                .Where(r => !string.IsNullOrEmpty(r.UserId))
                .GroupBy(r => r.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => (WatchlistSyncStatus)g.OrderByDescending(r => r.LastSyncedAt).First().SyncStatus);
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
