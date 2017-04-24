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
        Task<PlexLibraries> GetLibrarySections(string authToken, string plexFullHost);
        Task<PlexLibraries> GetLibrary(string authToken, string plexFullHost, string libraryId);
        Task<PlexMetadata> GetEpisodeMetaData(string authToken, string host, string ratingKey);
        Task<PlexMetadata> GetMetadata(string authToken, string plexFullHost, string itemId);
        Task<PlexMetadata> GetSeasons(string authToken, string plexFullHost, string ratingKey);
    }
}