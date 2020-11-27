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
        public RadarrApi(ILogger<RadarrApi> logger, IApi api)
        {
            Api = api;
            Logger = logger;
        }

        private IApi Api { get; }
        private ILogger<RadarrApi> Logger { get; }

        public async Task<List<RadarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request("/api/profile", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return await Api.Request<List<RadarrProfile>>(request);
        }

        public async Task<List<RadarrRootFolder>> GetRootFolders(string apiKey, string baseUrl)
        {
            var request = new Request("/api/rootfolder", baseUrl, HttpMethod.Get);

            AddHeaders(request, apiKey);
            return await Api.Request<List<RadarrRootFolder>>(request);
        }

        public async Task<SystemStatus> SystemStatus(string apiKey, string baseUrl)
        {
            var request = new Request("/api/system/status", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);

            return await Api.Request<SystemStatus>(request);
        }

        public async Task<List<MovieResponse>> GetMovies(string apiKey, string baseUrl)
        {
            var request = new Request("/api/movie", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);

            return await Api.Request<List<MovieResponse>>(request);
        }

        public async Task<MovieResponse> GetMovie(int id, string apiKey, string baseUrl)
        {
            var request = new Request($"/api/movie/{id}", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);

            return await Api.Request<MovieResponse>(request);
        }

        public async Task<MovieResponse> UpdateMovie(MovieResponse movie, string apiKey, string baseUrl)
        {
            var request = new Request($"/api/movie/", baseUrl, HttpMethod.Put);
            AddHeaders(request, apiKey);
            request.AddJsonBody(movie);

            return await Api.Request<MovieResponse>(request);
        }

        public async Task<RadarrAddMovie> AddMovie(int tmdbId, string title, int year, int qualityId, string rootPath, string apiKey, string baseUrl, bool searchNow, string minimumAvailability)
        {
            var request = new Request("/api/movie", baseUrl, HttpMethod.Post);

            var options = new RadarrAddMovieResponse
            {
                title = title,
                tmdbId = tmdbId,
                qualityProfileId = qualityId,
                rootFolderPath = rootPath,
                titleSlug = title + year,
                monitored = true,
                year = year,
                minimumAvailability = minimumAvailability
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

            var response = await Api.RequestContent(request);
            try
            {
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
                return JsonConvert.DeserializeObject<RadarrAddMovie>(response);
            }
            catch (JsonSerializationException jse)
            {
                Logger.LogError(LoggingEvents.RadarrApi, jse, "Error When adding movie to Radarr, Reponse: {0}", response);
            }
            return null;
        }

        public async Task<bool> MovieSearch(int[] movieIds, string apiKey, string baseUrl)
        {
            var result = await Command(apiKey, baseUrl, new { name = "MoviesSearch", movieIds });
            return result != null;
        }        
        
        public async Task<List<Tag>> GetTags(string apiKey, string baseUrl)
        {
            var request = new Request("/api/tag", baseUrl, HttpMethod.Get);
            AddHeaders(request, apiKey);

            return await Api.Request<List<Tag>>(request);
        }

        private async Task<CommandResult> Command(string apiKey, string baseUrl, object body)
        {
            var request = new Request($"/api/Command/", baseUrl, HttpMethod.Post);
            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(body);
            return await Api.Request<CommandResult>(request);
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
