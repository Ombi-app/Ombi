﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Api.Emby.Models;
using Ombi.Api.Emby.Models.Media.Tv;
using Ombi.Api.Emby.Models.Movie;
using Ombi.Helpers;

namespace Ombi.Api.Emby
{
    public class EmbyApi : IEmbyApi
    {
        public EmbyApi(IApi api)
        {
            Api = api;
        }

        private IApi Api { get; }
        private const string EmbyConnectService = "https://connect.emby.media/service/";

        /// <summary>
        /// Returns all users from the Emby Instance
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="apiKey"></param>
        public async Task<List<EmbyUser>> GetUsers(string baseUri, string apiKey)
        {
            var request = new Request("emby/users", baseUri, HttpMethod.Get);

            AddHeaders(request, apiKey);
            var obj = await Api.Request<List<EmbyUser>>(request);

            return obj;
        }

        public async Task<EmbySystemInfo> GetSystemInformation(string apiKey, string baseUrl)
        {
            var request = new Request("emby/System/Info", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);

            var obj = await Api.Request<EmbySystemInfo>(request);

            return obj;
        }

        public async Task<EmbyUser> LogIn(string username, string password, string apiKey, string baseUri)
        {
            var request = new Request("emby/users/authenticatebyname", baseUri, HttpMethod.Post);
            var body = new
            {
                username,
                pw = password,
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

        public async Task<EmbyConnectUser> LoginConnectUser(string username, string password)
        {
            var request = new Request("user/authenticate", EmbyConnectService, HttpMethod.Post);
            var body = new
            {
                nameOrEmail = username,
                rawpw = password,
            };

            request.AddJsonBody(body);

            request.AddHeader("Accept", "application/json");
            request.AddContentHeader("Content-Type", "application/json");

            var obj = await Api.Request<EmbyConnectUser>(request);
            return obj;
        }

        public async Task<EmbyItemContainer<MovieInformation>> GetCollection(string mediaId, string apiKey, string userId, string baseUrl)
        {
            var request = new Request($"emby/users/{userId}/items?parentId={mediaId}", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);

            return await Api.Request<EmbyItemContainer<MovieInformation>>(request);
        }

        public async Task<EmbyItemContainer<EmbyMovie>> GetAllMovies(string apiKey, string userId, string baseUri)
        {
            return await GetAll<EmbyMovie>("Movie", apiKey, userId, baseUri);
        }

        public async Task<EmbyItemContainer<EmbyEpisodes>> GetAllEpisodes(string apiKey, string userId, string baseUri)
        {
            return await GetAll<EmbyEpisodes>("Episode", apiKey, userId, baseUri);
        }

        public async Task<EmbyItemContainer<EmbySeries>> GetAllShows(string apiKey, string userId, string baseUri)
        {
            return await GetAll<EmbySeries>("Series", apiKey, userId, baseUri);
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
            var request = new Request($"emby/users/{userId}/items/{mediaId}", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);
            var response = await Api.RequestContent(request);

            return JsonConvert.DeserializeObject<T>(response);
        }



        private async Task<EmbyItemContainer<T>> GetAll<T>(string type, string apiKey, string userId, string baseUri)
        {
            var request = new Request($"emby/users/{userId}/items", baseUri, HttpMethod.Get);

            request.AddQueryString("Recursive", true.ToString());
            request.AddQueryString("IncludeItemTypes", type);

            AddHeaders(request, apiKey);


            var obj = await Api.Request<EmbyItemContainer<T>>(request);
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
