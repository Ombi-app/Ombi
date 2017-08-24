using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository
{
    public interface IMovieRequestRepository
    {
        Task<MovieRequests> Add(MovieRequests request);
        Task Delete(MovieRequests request);
        IQueryable<MovieRequests> Get();
        Task<MovieRequests> GetRequest(int theMovieDbId);
        Task Update(MovieRequests request);
        Task Save();
    }
}