using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models.Requests;
using Ombi.Store.Entities;

namespace Ombi.Core.Requests.Models
{
    public interface IRequestService
    {
        int AddRequest(RequestModel model);
        Task<int> AddRequestAsync(RequestModel model);
        void BatchDelete(IEnumerable<RequestModel> model);
        void BatchUpdate(IEnumerable<RequestModel> model);
        RequestModel CheckRequest(int providerId);
        RequestModel CheckRequest(string musicId);
        Task<RequestModel> CheckRequestAsync(int providerId);
        Task<RequestModel> CheckRequestAsync(string musicId);
        void DeleteRequest(RequestModel request);
        Task DeleteRequestAsync(int request);
        Task DeleteRequestAsync(RequestModel request);
        RequestModel Get(int id);
        IEnumerable<RequestModel> GetAll();
        Task<IEnumerable<RequestModel>> GetAllAsync();
        Task<IEnumerable<RequestModel>> GetAllAsync(int count, int position);
        Task<RequestModel> GetAsync(int id);
        RequestModel UpdateRequest(RequestModel model);
    }
}