using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Store;
using Ombi.Store.Models;

namespace Ombi.Core.Queue
{
    public interface ITransientFaultQueue
    {
        void Dequeue();
        Task DequeueAsync();
        IEnumerable<RequestQueue> GetQueue();
        Task<IEnumerable<RequestQueue>> GetQueueAsync();
        void QueueItem(RequestedModel request, string id, RequestType type, FaultType faultType);
        Task QueueItemAsync(RequestedModel request, string id, RequestType type, FaultType faultType, string message = null);
    }
}