using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.Radarr.Models;

namespace Ombi.Api.Radarr
{
    public class RadarrApi : IRadarrApi
    {
        public RadarrApi(ILogger<RadarrApi> logger)
        {
            Api = new Api();
            Logger = logger;
        }

        private Api Api { get; }
        private ILogger<RadarrApi> Logger { get; }

        public async Task<List<RadarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request(baseUrl, "/api/profile", HttpMethod.Get);

            AddHeaders(request, apiKey);
            return await Api.Request<List<RadarrProfile>>(request);
        }

        public async Task<List<RadarrRootFolder>> GetRootFolders(string apiKey, string baseUrl)
        {
            var request = new Request(baseUrl, "/api/rootfolder", HttpMethod.Get);

            AddHeaders(request, apiKey);
            return await Api.Request<List<RadarrRootFolder>>(request);
        }

        public async Task<SystemStatus> SystemStatus(string apiKey, string baseUrl)
        {
            var request = new Request(baseUrl, "/api/system/status", HttpMethod.Get);
            AddHeaders(request, apiKey);

            return await Api.Request<SystemStatus>(request);
        }

        public async Task<List<MovieResponse>> GetMovies(string apiKey, string baseUrl)
        {
            var request = new Request(baseUrl, "/api/movie", HttpMethod.Get);
            AddHeaders(request, apiKey);

            return await Api.Request<List<MovieResponse>>(request);
        }

        /// <summary>
        /// Adds the required headers and also the authorization header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        private void AddHeaders(Request request, string key)
        {
            request.AddHeader("X-Api-Key", key);
        }
    }
}
