using System;
using System.Net.Http;
using System.Threading.Tasks;
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
            request.AddContentHeader("Content-Type", "application/json" );
            request.AddHeader("Accept","application/json");
        }
    }
}
