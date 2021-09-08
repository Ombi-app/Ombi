using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;

namespace Ombi.Store.Repository.Requests
{
    public interface IMusicRequestRepository : IRepository<MusicRequests>
    {
        IQueryable<MusicRequests> GetAll(string userId);
        MusicRequests GetRequest(string foreignAlbumId);
        Task<MusicRequests> GetRequestAsync(string foreignAlbumId);
        IQueryable<MusicRequests> GetWithUser();
        IQueryable<MusicRequests> GetWithUser(string userId);
        Task Save();
        Task Update(MusicRequests request);
    }
}