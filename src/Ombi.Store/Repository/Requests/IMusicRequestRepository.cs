using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public interface IMusicRequestRepository : IRepository<AlbumRequest>
    {
        IQueryable<AlbumRequest> GetAll(string userId);
        AlbumRequest GetRequest(string foreignAlbumId);
        Task<AlbumRequest> GetRequestAsync(string foreignAlbumId);
        IQueryable<AlbumRequest> GetWithUser();
        IQueryable<AlbumRequest> GetWithUser(string userId);
        Task Save();
        Task Update(AlbumRequest request);
    }
}