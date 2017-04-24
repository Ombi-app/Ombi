using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Api.Plex.Models;
using Ombi.Api.Plex.Models.Server;
using Ombi.Api.Plex.Models.Status;

namespace Ombi.Api.Plex
{
    public class PlexApi : IPlexApi
    {
        public PlexApi()
        {
            Api = new Api();
        }

        private Api Api { get; }

        private const string SignInUri = "https://plex.tv/users/sign_in.json";
        private const string FriendsUri = "https://plex.tv/pms/friends/all";
        private const string GetAccountUri = "https://plex.tv/users/account";
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

        public async Task<PlexServer> GetServer(string authToken)
        {
            var request = new Request(ServerUri, string.Empty, HttpMethod.Get, ContentType.Xml);

            AddHeaders(request, authToken);

            return await Api.Request<PlexServer>(request);
        }

        public async Task<PlexLibraries> GetLibrarySections(string authToken, string plexFullHost)
        {
            var request = new Request(plexFullHost, "library/sections", HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexLibraries>(request);
        }

        public async Task<PlexLibraries> GetLibrary(string authToken, string plexFullHost, string libraryId)
        {
            var request = new Request(plexFullHost, $"library/sections/{libraryId}/all", HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexLibraries>(request);
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
        public async Task<PlexMetadata> GetEpisodeMetaData(string authToken, string plexFullHost, string ratingKey)
        {
            var request = new Request(plexFullHost, $"/library/metadata/{ratingKey}", HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexMetadata>(request);
        }

        public async Task<PlexMetadata> GetMetadata(string authToken, string plexFullHost, string itemId)
        {
            var request = new Request(plexFullHost, $"library/metadata/{itemId}", HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexMetadata>(request);
        }

        public async Task<PlexMetadata> GetSeasons(string authToken, string plexFullHost, string ratingKey)
        {
            var request = new Request(plexFullHost, $"library/metadata/{ratingKey}/children", HttpMethod.Get);
            AddHeaders(request, authToken);
            return await Api.Request<PlexMetadata>(request);
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
    }
}
