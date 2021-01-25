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
    public class TvRequestRepository : BaseRepository<TvRequests, OmbiContext>, ITvRequestRepository
    {
        public TvRequestRepository(OmbiContext ctx) : base(ctx)
        {
            Db = ctx;
        }

        public OmbiContext Db { get; }

        public async Task<TvRequests> GetRequestAsync(int tvDbId)
        {
            return await Db.TvRequests.Where(x => x.TvDbId == tvDbId)
                .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.RequestedUser)
                .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.SeasonRequests)
                    .ThenInclude(x => x.Episodes)
                .FirstOrDefaultAsync();
        }

        public TvRequests GetRequest(int tvDbId)
        {
            return Db.TvRequests.Where(x => x.TvDbId == tvDbId).AsSplitQuery()
                .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.RequestedUser)
                .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.SeasonRequests)
                    .ThenInclude(x => x.Episodes)
                .FirstOrDefault();
        }

        public IQueryable<TvRequests> Get()
        {
            return Db.TvRequests
                .Include(x => x.ChildRequests)
                .ThenInclude(x => x.RequestedUser)
                .Include(x => x.ChildRequests)
                .ThenInclude(x => x.SeasonRequests)
                .ThenInclude(x => x.Episodes)
                .AsQueryable();
        }

        public IQueryable<TvRequests> Get(string userId)
        {
            return Db.TvRequests
                .Include(x => x.ChildRequests)
                .ThenInclude(x => x.RequestedUser)
                .Include(x => x.ChildRequests)
                .ThenInclude(x => x.SeasonRequests)
                .ThenInclude(x => x.Episodes)
                .Where(x => x.ChildRequests.Any(a => a.RequestedUserId == userId))
                .AsQueryable();
        }

        public IQueryable<TvRequests> GetLite(string userId)
        {
            return Db.TvRequests
                .Include(x => x.ChildRequests)
                .ThenInclude(x => x.RequestedUser)
                .Where(x => x.ChildRequests.Any(a => a.RequestedUserId == userId))
                .AsQueryable();
        }

        public IQueryable<TvRequests> GetLite()
        {
            return Db.TvRequests
                .Include(x => x.ChildRequests)
                .ThenInclude(x => x.RequestedUser)
                .AsQueryable();
        }

        public IQueryable<ChildRequests> GetChild()
        {
            return Db.ChildRequests
                .Include(x => x.RequestedUser)
                .Include(x => x.ParentRequest)
                .Include(x => x.SeasonRequests)
                .ThenInclude(x => x.Episodes)
                .AsQueryable();
        }

        public IQueryable<ChildRequests> GetChild(string userId)
        {
            return Db.ChildRequests
                .Where(x => x.RequestedUserId == userId)
                .Include(x => x.RequestedUser)
                .Include(x => x.ParentRequest)
                .Include(x => x.SeasonRequests)
                .ThenInclude(x => x.Episodes)
                .AsQueryable();
        }

        public async Task MarkChildAsAvailable(int id)
        {
            var request = new ChildRequests { Id = id, Available = true, MarkedAsAvailable = DateTime.UtcNow };
            var attached = Db.ChildRequests.Attach(request);
            attached.Property(x => x.Available).IsModified = true;
            attached.Property(x => x.MarkedAsAvailable).IsModified = true;
            await Db.SaveChangesAsync();
        }

        public async Task MarkEpisodeAsAvailable(int id)
        {
            var request = new EpisodeRequests { Id = id, Available = true };
            var attached = Db.EpisodeRequests.Attach(request);
            attached.Property(x => x.Available).IsModified = true;
            await Db.SaveChangesAsync();
        }

        public async Task Save()
        {
            await InternalSaveChanges();
        }

        public async Task<ChildRequests> AddChild(ChildRequests request)
        {
            await Db.ChildRequests.AddAsync(request);
            await InternalSaveChanges();

            return request;
        }

        public async Task DeleteChild(ChildRequests request)
        {
            Db.ChildRequests.Remove(request);
            await InternalSaveChanges();
        }

        public async Task DeleteChildRange(IEnumerable<ChildRequests> request)
        {
            Db.ChildRequests.RemoveRange(request);
            await InternalSaveChanges();
        }

        public async Task Update(TvRequests request)
        {
            Db.Update(request);

            await InternalSaveChanges();
        }

        public async Task UpdateChild(ChildRequests request)
        {
            Db.Update(request);

            await InternalSaveChanges();
        }
    }
}