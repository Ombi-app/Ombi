using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Emby.Models;
using Ombi.Helpers;

namespace Ombi.Api.Emby
{
    public class EmbyApi : IEmbyApi
    {
        public EmbyApi()
        {
            Api = new Api();
        }

        private Api Api { get; }

        /// <summary>
        /// Returns all users from the Emby Instance
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="apiKey"></param>
        public async Task<List<EmbyUser>> GetUsers(Uri baseUri, string apiKey)
        {
            var request = new Request("emby/users", baseUri.ToString(), HttpMethod.Get);
 
            AddHeaders(request, apiKey);
            var obj = await Api.Request<List<EmbyUser>>(request);

            return obj;
        }

        public async Task<EmbySystemInfo> GetSystemInformation(string apiKey, Uri baseUrl)
        {
            var request = new Request("emby/System/Info", baseUrl.ToString(), HttpMethod.Get);

            AddHeaders(request, apiKey);

            var obj = await Api.Request<EmbySystemInfo>(request);

            return obj;
        }

        public async Task<EmbyUser> LogIn(string username, string password, string apiKey, Uri baseUri)
        {
            var request = new Request("emby/users/authenticatebyname", baseUri.ToString(), HttpMethod.Post);


            var body = new
            {
                username,
                password = password.GetSha1Hash().ToLower(),
                passwordMd5 = password.CalcuateMd5Hash()
            };

            request.AddJsonBody(body);

            request.AddHeader("X-Emby-Authorization",
                $"MediaBrowser Client=\"Ombi\", Device=\"Ombi\", DeviceId=\"v3\", Version=\"v3\"");
            AddHeaders(request, apiKey);

            var obj = await Api.Request<EmbyUser>(request);
            return obj;
        }

        private static void AddHeaders(Request req, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                req.AddHeader("X-MediaBrowser-Token", apiKey);
            }
            req.AddHeader("Accept", "application/json");
            req.AddContentHeader("Content-Type", "application/json");
            req.AddHeader("Device", "Ombi");
        }
    }
}
