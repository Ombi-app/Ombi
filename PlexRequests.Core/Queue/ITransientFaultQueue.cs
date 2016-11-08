using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store;
using PlexRequests.Store.Models;

namespace PlexRequests.Core.Queue
{
    public interface ITransientFaultQueue
    {
        void Dequeue();
        Task DequeueAsync();
        IEnumerable<RequestQueue> GetQueue();
        Task<IEnumerable<RequestQueue>> GetQueueAsync();
        void QueueItem(RequestedModel request, RequestType type, FaultType faultType);
        Task QueueItemAsync(RequestedModel request, RequestType type, FaultType faultType);
    }
}