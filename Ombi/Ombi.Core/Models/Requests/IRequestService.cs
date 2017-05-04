using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Store.Entities;

namespace Ombi.Core.Requests.Models
{
    public interface IRequestService<T> where T : BaseRequestModel
    {
        int AddRequest(T model);
        Task<int> AddRequestAsync(T model);
        void BatchDelete(IEnumerable<T> model);
        void BatchUpdate(IEnumerable<T> model);
        T CheckRequest(int providerId);
        Task<T> CheckRequestAsync(int providerId);
        void DeleteRequest(T request);
        Task DeleteRequestAsync(int request);
        Task DeleteRequestAsync(T request);
        T Get(int id);
        IEnumerable<T> GetAll();
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(int count, int position);
        Task<T> GetAsync(int id);
        T UpdateRequest(T model);
    }
}