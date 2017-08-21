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

        private IOmbiContext Db { get; }

        public async Task<TvRequests> GetRequest(int tvDbId)
        {
            return await Db.TvRequests.Where(x => x.TvDbId == tvDbId)
                .Include(x => x.ChildRequests)
                    .ThenInclude(x => x.RequestedUser)
                .Include(x => x.ChildRequests)
                .ThenInclude(x => x.SeasonRequests)
                .ThenInclude(x => x.Episodes)
                .FirstOrDefaultAsync();
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
        public IQueryable<ChildRequests> GetChild()
        {
            return Db.ChildRequests
                .Include(x => x.RequestedUser)
                .Include(x => x.SeasonRequests)
                .ThenInclude(x => x.Episodes)
                .AsQueryable();
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

        public async Task Update(TvRequests request)
        {
            Db.Attach(request).State = EntityState.Modified;
            
            await Db.SaveChangesAsync();
        }
        
        public async Task UpdateChild(ChildRequests request)
        {
            Db.Attach(request).State = EntityState.Modified;
            
            await Db.SaveChangesAsync();
        }
    }
}