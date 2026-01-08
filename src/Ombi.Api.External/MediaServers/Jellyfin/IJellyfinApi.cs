using System.Collections.Generic;
using System.Threading.Tasks;
using Ombi.Api.External.MediaServers.Jellyfin.Models;

namespace Ombi.Api.External.MediaServers.Jellyfin
{
    public interface IJellyfinApi : IBaseJellyfinApi
    {      
        Task<JellyfinConnectUser> LoginConnectUser(string username, string password);
        Task<List<LibraryVirtualFolders>> GetLibraries(string apiKey, string baseUrl);
    }
}