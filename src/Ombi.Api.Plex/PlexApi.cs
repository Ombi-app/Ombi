using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Plex.Models;
using Ombi.Api.Plex.Models.Friends;
using Ombi.Api.Plex.Models.Server;
using Ombi.Api.Plex.Models.Status;

namespace Ombi.Api.Plex
{
    public class PlexApi : IPlexApi
    {
        public PlexApi(IApi api)
        {
            Api = api;
        }

        private IApi Api { get; }

        private const string SignInUri = "https://plex.tv/users/sign_in.json";
        private const string FriendsUri = "https://plex.tv/pms/friends/all";
        private const string GetAccountUri = "https://plex.tv/users/account.json";
        private const string ServerUri = "https://plex.tv/pms/servers.xml";

        /// <summary>
        /// Sign into the Plex API
        /// This is for authenticating users credentials with Plex
        /// <para>NOTE: Plex "Managed" users do not work</para>
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<PlexAuthentication> SignIn(UserRequest user)
        {
            var userModel = new PlexUserRequest
            {
                user = user
            };
            var request = new Request(SignInUri, string.Empty, HttpMethod.Post);

            AddHeaders(request);
            request.AddJsonBody(userModel);

            var obj = await Api.Request<PlexAuthentication>(request);

            return obj;
        }

        public async Task<PlexStatus> GetStatus(string authToken, string uri)
        {
            var request = new Request(uri, string.Empty, HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexStatus>(request);
        }

        public async Task<PlexAccount> GetAccount(string authToken)
        {
            var request = new Request(GetAccountUri, string.Empty, HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexAccount>(request);
        }

        public async Task<PlexServer> GetServer(string authToken)
        {
            var request = new Request(ServerUri, string.Empty, HttpMethod.Get, ContentType.Xml);

            AddHeaders(request, authToken);

            return await Api.Request<PlexServer>(request);
        }

        public async Task<PlexContainer> GetLibrarySections(string authToken, string plexFullHost)
        {
            var request = new Request("library/sections", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexContainer>(request);
        }

        public async Task<PlexContainer> GetLibrary(string authToken, string plexFullHost, string libraryId)
        {
            var request = new Request($"library/sections/{libraryId}/all", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexContainer>(request);
        }

        /// <summary>
        // 192.168.1.69:32400/library/metadata/3662/allLeaves
        // The metadata ratingkey should be in the Cache
        // Search for it and then call the above with the Directory.RatingKey
        // THEN! We need the episode metadata using result.Vide.Key ("/library/metadata/3664")
        // We then have the GUID which contains the TVDB ID plus the season and episode number: guid="com.plexapp.agents.thetvdb://269586/2/8?lang=en"
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="plexFullHost"></param>
        /// <param name="ratingKey"></param>
        /// <returns></returns>
        public async Task<PlexMetadata> GetEpisodeMetaData(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new Request($"/library/metadata/{ratingKey}", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexMetadata>(request);
        }

        public async Task<PlexMetadata> GetMetadata(string authToken, string plexFullHost, int itemId)
        {
            var request = new Request($"library/metadata/{itemId}", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexMetadata>(request);
        }

        public async Task<PlexMetadata> GetSeasons(string authToken, string plexFullHost, int ratingKey)
        {
            var request = new Request($"library/metadata/{ratingKey}/children", plexFullHost, HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexMetadata>(request);
        }

        /// <summary>
        /// Gets all episodes.
        /// </summary>
        /// <param name="authToken">The authentication token.</param>
        /// <param name="host">The host.</param>
        /// <param name="section">The section.</param>
        /// <param name="start">The start count.</param>
        /// <param name="retCount">The return count, how many items you want returned.</param>
        /// <returns></returns>
        public async Task<PlexContainer> GetAllEpisodes(string authToken, string host, string section, int start, int retCount)
        {
            var request = new Request($"/library/sections/{section}/all", host, HttpMethod.Get);

            request.AddQueryString("type", "4");
            AddLimitHeaders(request, start, retCount);
            AddHeaders(request, authToken);

            return await Api.Request<PlexContainer>(request);  
        }

        /// <summary>
        /// Retuns all the Plex users for this account
        /// NOTE: For HOME USERS. There is no username or email, the user's home name is under the title property
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public async Task<PlexFriends> GetUsers(string authToken)
        {
            var request = new Request(string.Empty,FriendsUri, HttpMethod.Get, ContentType.Xml);
            AddHeaders(request, authToken);

            return await Api.Request<PlexFriends>(request);
        }

        /// <summary>
        /// Adds the required headers and also the authorization header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="authToken"></param>
        private void AddHeaders(Request request, string authToken)
        {
            request.AddHeader("X-Plex-Token", authToken);
            AddHeaders(request);
        }

        /// <summary>
        /// Adds the main required headers to the Plex Request
        /// </summary>
        /// <param name="request"></param>
        private void AddHeaders(Request request)
        {
            request.AddHeader("X-Plex-Client-Identifier", $"OmbiV3");
            request.AddHeader("X-Plex-Product", "Ombi");
            request.AddHeader("X-Plex-Version", "3");
            request.AddContentHeader("Content-Type", request.ContentType == ContentType.Json ? "application/json" : "application/xml");
            request.AddHeader("Accept", "application/json");
        }

        private void AddLimitHeaders(Request request, int from, int to)
        {
            request.AddHeader("X-Plex-Container-Start", from.ToString());
            request.AddHeader("X-Plex-Container-Size", to.ToString());
        }
    }
}
