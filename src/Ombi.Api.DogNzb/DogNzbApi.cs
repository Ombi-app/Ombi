using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Ombi.Api.DogNzb.Models;

namespace Ombi.Api.DogNzb
{
    public class DogNzbApi : IDogNzbApi
    {
        public DogNzbApi(IApi api, ILogger<DogNzbApi> log)
        {
            _api = api;
            _log = log;
        }

        private readonly IApi _api;
        private readonly ILogger<DogNzbApi> _log;
        private const string BaseUrl = "https://api.dognzb.cr/";

        public async Task<DogNzbMovies> ListMovies(string apiKey)
        {
            var request = new Request("watchlist", BaseUrl, HttpMethod.Get, ContentType.Xml);

            request.AddQueryString("t", "list");
            request.AddQueryString("imdbid", string.Empty);
            request.AddQueryString("apikey", apiKey);

            return await _api.Request<DogNzbMovies>(request);
        }

        public async Task<DogNzbTvShows> ListTvShows(string apiKey)
        {
            var request = new Request("watchlist", BaseUrl, HttpMethod.Get, ContentType.Xml);

            request.AddQueryString("t", "list");
            request.AddQueryString("tvdbid", string.Empty);
            request.AddQueryString("apikey", apiKey);

            return await _api.Request<DogNzbTvShows>(request);
        }

        public async Task<DogNzbAddResult> AddTvShow(string apiKey, string tvdbId)
        {
            var request = new Request("watchlist", BaseUrl, HttpMethod.Get, ContentType.Xml);

            request.AddQueryString("t", "add");
            request.AddQueryString("tvdbid", tvdbId);
            request.AddQueryString("apikey", apiKey);
            var result = await _api.RequestContent(request);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DogNzbAddResult));
                StringReader reader = new StringReader(result);
                return (DogNzbAddResult)serializer.Deserialize(reader);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error when adding TV Shows to DogNzb");
                XmlSerializer serializer = new XmlSerializer(typeof(DogNzbError));
                StringReader reader = new StringReader(result);
                var error = (DogNzbError)serializer.Deserialize(reader);

                return new DogNzbAddResult
                {
                    Failure = true,
                    ErrorMessage = error.Description
                };
            }
        }

        public async Task<DogNzbMovieAddResult> AddMovie(string apiKey, string imdbid)
        {
            var request = new Request("watchlist", BaseUrl, HttpMethod.Get, ContentType.Xml);

            request.AddQueryString("t", "add");
            request.AddQueryString("imdbid", imdbid);
            request.AddQueryString("apikey", apiKey);
            return await _api.Request<DogNzbMovieAddResult>(request);
        }
    }
}
