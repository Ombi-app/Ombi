using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public interface IMovieRequestRepository : IRepository<MovieRequests>
    {
        Task<MovieRequests> GetRequestAsync(int theMovieDbId);
        MovieRequests GetRequest(int theMovieDbId);
        Task Update(MovieRequests request);
        Task Save();
        Task MarkAsAvailable(int id);
        IQueryable<MovieRequests> GetWithUser();
        IQueryable<MovieRequests> GetWithUser(string userId);
        IQueryable<MovieRequests> GetAll(string userId);
    }
}