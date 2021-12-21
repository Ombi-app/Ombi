using System.Threading.Tasks;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Senders
{
    public interface IMusicSender
    {
        Task<SenderResult> SendAlbum(MusicRequests model);
        Task<SenderResult> SendArtist(MusicRequests model);
    }
}