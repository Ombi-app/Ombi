using System.Threading.Tasks;
using Ombi.Api.Plex.Models;
using Ombi.Api.Plex.Models.Server;
using Ombi.Api.Plex.Models.Status;

namespace Ombi.Api.Plex
{
    public interface IPlexApi
    {
        Task<PlexStatus> GetStatus(string authToken, string uri);
        Task<PlexAuthentication> SignIn(UserRequest user);
        Task<PlexServer> GetServer(string authToken);
    }
}