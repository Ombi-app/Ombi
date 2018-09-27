using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public class MusicRequestRepository : Repository<AlbumRequest>, IMusicRequestRepository
    {
        public MusicRequestRepository(IOmbiContext ctx) : base(ctx)
        {
            Db = ctx;
        }

        private IOmbiContext Db { get; }

        public Task<AlbumRequest> GetRequestAsync(string foreignAlbumId)
        {
                return Db.AlbumRequests.Where(x => x.ForeignAlbumId == foreignAlbumId)
                    .Include(x => x.RequestedUser)
                    .FirstOrDefaultAsync();
        }

        public IQueryable<AlbumRequest> GetAll(string userId)
        {
            return GetWithUser().Where(x => x.RequestedUserId == userId);
        }

        public AlbumRequest GetRequest(string foreignAlbumId)
        {
            return Db.AlbumRequests.Where(x => x.ForeignAlbumId == foreignAlbumId)
                .Include(x => x.RequestedUser)
                .FirstOrDefault();
        }

        public IQueryable<AlbumRequest> GetWithUser()
        {
            return Db.AlbumRequests
                .Include(x => x.RequestedUser)
                .ThenInclude(x => x.NotificationUserIds)
                .AsQueryable();
        }


        public IQueryable<AlbumRequest> GetWithUser(string userId)
        {
            return Db.AlbumRequests
                .Where(x => x.RequestedUserId == userId)
                .Include(x => x.RequestedUser)
                .ThenInclude(x => x.NotificationUserIds)
                .AsQueryable();
        }

        public async Task Update(AlbumRequest request)
        {
            if (Db.Entry(request).State == EntityState.Detached)
            {
                Db.AlbumRequests.Attach(request);
                Db.Update(request);
            }
            await Db.SaveChangesAsync();
        }

        public async Task Save()
        {
            await Db.SaveChangesAsync();
        }
    }
}