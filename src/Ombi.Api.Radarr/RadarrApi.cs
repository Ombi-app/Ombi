using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ombi.Api.Radarr.Models;
using Ombi.Helpers;

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

        public async Task<RadarrAddMovieResponse> AddMovie(int tmdbId, string title, int year, int qualityId, string rootPath, string apiKey, string baseUrl, bool searchNow = false)
        {
            var request = new Request(baseUrl, "/api/movie", HttpMethod.Post);

            var options = new RadarrAddMovieResponse
            {
                title = title,
                tmdbId = tmdbId,
                qualityProfileId = qualityId,
                rootFolderPath = rootPath,
                titleSlug = title,
                monitored = true,
                year = year
            };

            if (searchNow)
            {
                options.addOptions = new RadarrAddOptions
                {
                    searchForMovie = true
                };
            }


            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(options);

            try
            {
                var response = await Api.Request(request);
                if (response.Contains("\"message\":"))
                {
                    var error = JsonConvert.DeserializeObject<RadarrError>(response);
                    return new RadarrAddMovieResponse { Error = error };
                }
                if (response.Contains("\"errorMessage\":"))
                {
                    var error = JsonConvert.DeserializeObject<List<RadarrErrorResponse>>(response).FirstOrDefault();
                    return new RadarrAddMovieResponse { Error = new RadarrError { message = error?.errorMessage } };
                }
                return JsonConvert.DeserializeObject<RadarrAddMovieResponse>(response);
            }
            catch (JsonSerializationException jse)
            {
                Logger.LogError(LoggingEvents.RadarrApiException,jse, "Error When adding movie to Radarr");
            }
            return null;
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
