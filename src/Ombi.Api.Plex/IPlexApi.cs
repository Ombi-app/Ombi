using System;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Api.Plex.Models;
using Ombi.Api.Plex.Models.Friends;
using Ombi.Api.Plex.Models.OAuth;
using Ombi.Api.Plex.Models.Server;
using Ombi.Api.Plex.Models.Status;

namespace Ombi.Api.Plex
{
    public interface IPlexApi
    {
        Task<PlexStatus> GetStatus(string authToken, string uri);
        Task<PlexLibrariesForMachineId> GetLibrariesForMachineId(string authToken, string machineId);
        Task<PlexAuthentication> SignIn(UserRequest user);
        Task<PlexServer> GetServer(string authToken);
        Task<PlexContainer> GetLibrarySections(string authToken, string plexFullHost);
        Task<PlexContainer> GetLibrary(string authToken, string plexFullHost, string libraryId);
        Task<PlexMetadata> GetEpisodeMetaData(string authToken, string host, string ratingKey);
        Task<PlexMetadata> GetMetadata(string authToken, string plexFullHost, string itemId);
        Task<PlexMetadata> GetSeasons(string authToken, string plexFullHost, string ratingKey);
        Task<PlexContainer> GetAllEpisodes(string authToken, string host, string section, int start, int retCount);
        Task<PlexUsers> GetUsers(string authToken);
        Task<PlexAccount> GetAccount(string authToken);
        Task<PlexMetadata> GetRecentlyAdded(string authToken, string uri, string sectionId);
        Task<PlexMetadata> GetPlayed(string authToken, string uri, string sectionId, int maxNumberOfItems = 0);
        Task<OAuthContainer> GetPin(int pinId);
        Task<Uri> GetOAuthUrl(string code, string applicationUrl);
        Task<PlexAddWrapper> AddUser(string emailAddress, string serverId, string authToken, int[] libs);
        Task<PlexWatchlistContainer> GetWatchlist(string plexToken, CancellationToken cancellationToken);
        Task<PlexWatchlistMetadataContainer> GetWatchlistMetadata(string ratingKey, string plexToken, CancellationToken cancellationToken);
    }
}