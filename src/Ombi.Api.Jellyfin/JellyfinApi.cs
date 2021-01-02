using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Ombi.Api.Jellyfin.Models;
using Ombi.Api.Jellyfin.Models.Media.Tv;
using Ombi.Api.Jellyfin.Models.Movie;
using Ombi.Helpers;

namespace Ombi.Api.Jellyfin
{
    public class JellyfinApi : IJellyfinApi
    {
        public JellyfinApi(IApi api)
        {
            Api = api;
        }

        private IApi Api { get; }

        /// <summary>
        /// Returns all users from the Jellyfin Instance
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="apiKey"></param>
        public async Task<List<JellyfinUser>> GetUsers(string baseUri, string apiKey)
        {
            var request = new Request("users", baseUri, HttpMethod.Get);

            AddHeaders(request, apiKey);
            var obj = await Api.Request<List<JellyfinUser>>(request);

            return obj;
        }

        public async Task<JellyfinSystemInfo> GetSystemInformation(string apiKey, string baseUrl)
        {
            var request = new Request("System/Info", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);

            var obj = await Api.Request<JellyfinSystemInfo>(request);

            return obj;
        }

        public async Task<PublicInfo> GetPublicInformation(string baseUrl)
        {
            var request = new Request("System/Info/public", baseUrl, HttpMethod.Get);

            AddHeaders(request, string.Empty);

            var obj = await Api.Request<PublicInfo>(request);

            return obj;
        }

        public async Task<JellyfinUser> LogIn(string username, string password, string apiKey, string baseUri)
        {
            var request = new Request("users/authenticatebyname", baseUri, HttpMethod.Post);
            var body = new
            {
                username,
                pw = password,
            };

            request.AddJsonBody(body);

            request.AddHeader("X-Emby-Authorization",
                $"MediaBrowser Client=\"Ombi\", Device=\"Ombi\", DeviceId=\"v3\", Version=\"v3\"");
            AddHeaders(request, apiKey);

            var obj = await Api.Request<JellyfinUser>(request);
            return obj;
        }

        public async Task<JellyfinItemContainer<JellyfinMovie>> GetCollection(string mediaId, string apiKey, string userId, string baseUrl)
        {
            var request = new Request($"users/{userId}/items?parentId={mediaId}", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);

            request.AddQueryString("Fields", "ProviderIds,Overview");

            request.AddQueryString("IsVirtualItem", "False");

            return await Api.Request<JellyfinItemContainer<JellyfinMovie>>(request);
        }

        public async Task<JellyfinItemContainer<JellyfinMovie>> GetAllMovies(string apiKey, int startIndex, int count, string userId, string baseUri)
        {
            return await GetAll<JellyfinMovie>("Movie", apiKey, userId, baseUri, true, startIndex, count);
        }

        public async Task<JellyfinItemContainer<JellyfinEpisodes>> GetAllEpisodes(string apiKey, int startIndex, int count, string userId, string baseUri)
        {
            return await GetAll<JellyfinEpisodes>("Episode", apiKey, userId, baseUri, false, startIndex, count);
        }

        public async Task<JellyfinItemContainer<JellyfinSeries>> GetAllShows(string apiKey, int startIndex, int count, string userId, string baseUri)
        {
            return await GetAll<JellyfinSeries>("Series", apiKey, userId, baseUri, false, startIndex, count);
        }

        public async Task<SeriesInformation> GetSeriesInformation(string mediaId, string apiKey, string userId, string baseUrl)
        {
            return await GetInformation<SeriesInformation>(mediaId, apiKey, userId, baseUrl);
        }
        public async Task<MovieInformation> GetMovieInformation(string mediaId, string apiKey, string userId, string baseUrl)
        {
            return await GetInformation<MovieInformation>(mediaId, apiKey, userId, baseUrl);
        }

        public async Task<EpisodeInformation> GetEpisodeInformation(string mediaId, string apiKey, string userId, string baseUrl)
        {
            return await GetInformation<EpisodeInformation>(mediaId, apiKey, userId, baseUrl);
        }

        private async Task<T> GetInformation<T>(string mediaId, string apiKey, string userId, string baseUrl)
        {
            var request = new Request($"users/{userId}/items/{mediaId}", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            var response = await Api.RequestContent(request);

            return JsonConvert.DeserializeObject<T>(response);
        }

        private async Task<JellyfinItemContainer<T>> GetAll<T>(string type, string apiKey, string userId, string baseUri, bool includeOverview = false)
        {
            var request = new Request($"users/{userId}/items", baseUri, HttpMethod.Get);

            request.AddQueryString("Recursive", true.ToString());
            request.AddQueryString("IncludeItemTypes", type);
            request.AddQueryString("Fields", includeOverview ? "ProviderIds,Overview" : "ProviderIds");

            request.AddQueryString("IsVirtualItem", "False");

            AddHeaders(request, apiKey);


            var obj = await Api.Request<JellyfinItemContainer<T>>(request);
            return obj;
        }
        private async Task<JellyfinItemContainer<T>> GetAll<T>(string type, string apiKey, string userId, string baseUri, bool includeOverview, int startIndex, int count)
        {
            var request = new Request($"users/{userId}/items", baseUri, HttpMethod.Get);

            request.AddQueryString("Recursive", true.ToString());
            request.AddQueryString("IncludeItemTypes", type);
            request.AddQueryString("Fields", includeOverview ? "ProviderIds,Overview" : "ProviderIds");
            request.AddQueryString("startIndex", startIndex.ToString());
            request.AddQueryString("limit", count.ToString());

            request.AddQueryString("IsVirtualItem", "False");

            AddHeaders(request, apiKey);


            var obj = await Api.Request<JellyfinItemContainer<T>>(request);
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

        public Task<JellyfinConnectUser> LoginConnectUser(string username, string password)
        {
            throw new System.NotImplementedException();
        }
    }
}
