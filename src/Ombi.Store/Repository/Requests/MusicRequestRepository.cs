using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;
using Ombi.Store.Context;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public class MusicRequestRepository : Repository<MusicRequests>, IMusicRequestRepository
    {
        public MusicRequestRepository(OmbiContext ctx) : base(ctx)
        {
            Db = ctx;
        }

        private OmbiContext Db { get; }

        public Task<MusicRequests> GetRequestAsync(string foreignAlbumId)
        {
                return Db.MusicRequests.Where(x => x.ForeignAlbumId == foreignAlbumId)
                    .Include(x => x.RequestedUser)
                    .FirstOrDefaultAsync();
        }

        public IQueryable<MusicRequests> GetAll(string userId)
        {
            return GetWithUser().Where(x => x.RequestedUserId == userId);
        }

        public MusicRequests GetRequest(string foreignAlbumId)
        {
            return Db.MusicRequests.Where(x => x.ForeignAlbumId == foreignAlbumId)
                .Include(x => x.RequestedUser)
                .FirstOrDefault();
        }

        public IQueryable<MusicRequests> GetWithUser()
        {
            return Db.MusicRequests
                .Include(x => x.RequestedUser)
                .ThenInclude(x => x.NotificationUserIds)
                .AsQueryable();
        }


        public IQueryable<MusicRequests> GetWithUser(string userId)
        {
            return Db.MusicRequests
                .Where(x => x.RequestedUserId == userId)
                .Include(x => x.RequestedUser)
                .ThenInclude(x => x.NotificationUserIds)
                .AsQueryable();
        }

        public async Task Update(MusicRequests request)
        {
            if (Db.Entry(request).State == EntityState.Detached)
            {
                Db.MusicRequests.Attach(request);
                Db.Update(request);
            }
            await InternalSaveChanges();
        }

        public async Task UpdateArtist(MusicRequests request)
        {
            if (Db.Entry(request).State == EntityState.Detached)
            {
                Db.MusicRequests.Attach(request);
                Db.Update(request);
            }
            await InternalSaveChanges();
        }

        public async Task Save()
        {
            await InternalSaveChanges();
        }
    }
}