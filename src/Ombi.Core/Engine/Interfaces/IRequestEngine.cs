using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Core.Models;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities;

namespace Ombi.Core.Engine.Interfaces
{
    public interface IRequestEngine<T>
    {

        //Task<IEnumerable<T>> GetApprovedRequests();
        //Task<IEnumerable<T>> GetNewRequests();
        //Task<IEnumerable<T>> GetAvailableRequests();
        RequestCountModel RequestCount();
        Task<RequestsViewModel<T>> GetRequests(int count, int position, OrderFilterModel model);
        Task<IEnumerable<T>> GetRequests();
        Task<bool> UserHasRequest(string userId);

        Task<RequestEngineResult> MarkUnavailable(int modelId);
        Task<RequestEngineResult> MarkAvailable(int modelId);
        Task<int> GetTotal();
        Task UnSubscribeRequest(int requestId, RequestType type);
        Task SubscribeToRequest(int requestId, RequestType type);
        Task<RequestQuotaCountModel> GetRemainingRequests(OmbiUser user = null);
    }
}