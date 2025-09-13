using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.External.MediaServers.Emby.Models;
using Ombi.Api.External.MediaServers.Emby.Models.Media;

namespace Ombi.Api.External.MediaServers.Emby
{
    public interface IEmbyApi : IBaseEmbyApi
    {      
        Task<EmbyConnectUser> LoginConnectUser(string username, string password);
        Task<List<LibraryVirtualFolders>> GetLibraries(string apiKey, string baseUrl);
    }
}