using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public class TvRequestRepository : ITvRequestRepository
    {
        public TvRequestRepository(IOmbiContext ctx)
        {
            Db = ctx;
        }

        public IOmbiContext Db { get; }

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
            return Db.TvRequests.Where(x => x.TvDbId == tvDbId)
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

        public async Task Save()
        {
            await Db.SaveChangesAsync();
        }

        public async Task<TvRequests> Add(TvRequests request)
        {
            await Db.TvRequests.AddAsync(request);
            await Db.SaveChangesAsync();
            return request;
        }

        public async Task<ChildRequests> AddChild(ChildRequests request)
        {
            await Db.ChildRequests.AddAsync(request);
            await Db.SaveChangesAsync();

            return request;
        }

        public async Task Delete(TvRequests request)
        {
            Db.TvRequests.Remove(request);
            await Db.SaveChangesAsync();
        }

        public async Task DeleteChild(ChildRequests request)
        {
            Db.ChildRequests.Remove(request);
            await Db.SaveChangesAsync();
        }

        public async Task DeleteChildRange(IEnumerable<ChildRequests> request)
        {
            Db.ChildRequests.RemoveRange(request);
            await Db.SaveChangesAsync();
        }

        public async Task Update(TvRequests request)
        {
            Db.Update(request);
            
            await Db.SaveChangesAsync();
        }
        
        public async Task UpdateChild(ChildRequests request)
        {
            Db.Update(request);

            await Db.SaveChangesAsync();
        }
    }
}