using System.Threading.Tasks;
using Ombi.Api.Emby.Models;
using Ombi.Api.Emby.Models.Media;

namespace Ombi.Api.Emby
{
    public interface IEmbyApi : IBaseEmbyApi
    {      
        Task<EmbyConnectUser> LoginConnectUser(string username, string password);
        Task<EmbyItemContainer<MediaFolders>> GetLibraries(string apiKey, string baseUrl);
    }
}