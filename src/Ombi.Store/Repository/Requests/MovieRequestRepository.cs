using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public class MovieRequestRepository : IMovieRequestRepository
    {
        public MovieRequestRepository(IOmbiContext ctx)
        {
            Db = ctx;
        }

        private IOmbiContext Db { get; }

        public async Task<MovieRequests> GetRequest(int theMovieDbId)
        {
            return await Db.MovieRequests.Where(x => x.TheMovieDbId == theMovieDbId)
                .Include(x => x.RequestedUser)
                .FirstOrDefaultAsync();
        }

        public IQueryable<MovieRequests> Get()
        {
            return Db.MovieRequests
                .Include(x => x.RequestedUser)
                .AsQueryable();
        }

        public async Task<MovieRequests> Add(MovieRequests request)
        {
            await Db.MovieRequests.AddAsync(request);
            await Db.SaveChangesAsync();
            return request;
        }

        public async Task Delete(MovieRequests request)
        {
            Db.MovieRequests.Remove(request);
            await Db.SaveChangesAsync();
        }

        public async Task Update(MovieRequests request)
        {
            await Db.SaveChangesAsync();
        }
     }
}