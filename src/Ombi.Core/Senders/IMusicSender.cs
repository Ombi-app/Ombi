using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Senders
{
    public interface IMusicSender
    {
        Task<SenderResult> Send(AlbumRequest model);
    }
}