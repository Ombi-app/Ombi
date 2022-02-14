using System.Threading.Tasks;
using Ombi.Core.Senders;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core
{
    public interface IMovieSender
    {
        Task<SenderResult> Send(MovieRequests model, bool is4K);
    }
}