using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Lidarr.Models;

namespace Ombi.Api.Lidarr
{
    public class LidarrApi : ILidarrApi
    {
        public LidarrApi(ILogger<LidarrApi> logger, IApi api)
        {
            Api = api;
            Logger = logger;
        }

        private IApi Api { get; }
        private ILogger Logger { get; }

        private const string ApiVersion = "/api/v1";

        public async Task<List<LidarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/profile", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return await Api.Request<List<LidarrProfile>>(request);
        }

        public async Task<List<LidarrRootFolder>> GetRootFolders(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/rootfolder", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return await Api.Request<List<LidarrRootFolder>>(request);
        }

        public async Task<List<AlbumLookup>> ArtistLookup(string searchTerm, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/Artist/lookup", baseUrl, HttpMethod.Get);
            request.AddQueryString("term", searchTerm);

            AddHeaders(request, apiKey);
            return await Api.Request<List<AlbumLookup>>(request);
        }

        public async Task<List<AlbumLookup>> AlbumLookup(string searchTerm, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiVersion}/Album/lookup", baseUrl, HttpMethod.Get);
            request.AddQueryString("term", searchTerm);

            AddHeaders(request, apiKey);
            return await Api.Request<List<AlbumLookup>>(request);
        }

        private void AddHeaders(Request request, string key)
        {
            request.AddHeader("X-Api-Key", key);
        }
    }
}
