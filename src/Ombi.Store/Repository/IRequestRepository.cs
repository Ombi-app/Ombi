using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IRequestRepository
    {
        void Delete(RequestBlobs entity);
        void DeleteAll(IEnumerable<RequestBlobs> entity);
        RequestBlobs Get(int id);
        IEnumerable<RequestBlobs> GetAll();
        Task<IEnumerable<RequestBlobs>> GetAllAsync();
        Task<RequestBlobs> GetAsync(int id);
        RequestBlobs Insert(RequestBlobs entity);
        Task<RequestBlobs> InsertAsync(RequestBlobs entity);
        RequestBlobs Update(RequestBlobs entity);
        void UpdateAll(IEnumerable<RequestBlobs> entity);
        IQueryable<RequestBlobs> GetAllQueryable();
    }
}