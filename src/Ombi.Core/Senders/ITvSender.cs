using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Senders
{
    public interface ITvSender
    {
        Task<SenderResult> Send(ChildRequests model);
    }
}