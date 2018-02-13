using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Ombi.Api.CouchPotato.Models;
using Ombi.Helpers;

namespace Ombi.Api.CouchPotato
{
    public class CouchPotatoApi : ICouchPotatoApi
    {
        public CouchPotatoApi(IApi api, ILogger<CouchPotatoApi> log)
        {
            _api = api;
            _log = log;
        }

        private readonly IApi _api;
        private readonly ILogger<CouchPotatoApi> _log;

        public async Task<bool> AddMovie(string imdbid, string apiKey, string title, string baseUrl, string profileId = default(string))
        {
            var request = new Request($"/api/{apiKey}/movie.add", baseUrl, HttpMethod.Get);

            request.AddQueryString("title", title);
            request.AddQueryString("identifier", imdbid);
            if (!string.IsNullOrEmpty(profileId))
            {
                request.AddQueryString("profile_id", profileId);
            }

            var obj = await _api.Request<JObject>(request);

            if (obj.Count > 0)
            {
                try
                {
                    var result = (bool)obj["success"];
                    return result;
                }
                catch (Exception e)
                {
                    _log.LogError(LoggingEvents.CouchPotatoApi, e, "Error calling AddMovie");
                    return false;
                }
            }
            return false;
        }

        public async Task<CouchPotatoStatus> Status(string url, string apiKey)
        {
            var request = new Request($"api/{apiKey}/app.available/", url, HttpMethod.Get);
            return await _api.Request<CouchPotatoStatus>(request);
        }

        public async Task<CouchPotatoProfiles> GetProfiles(string url, string apiKey)
        {
            var request = new Request($"api/{apiKey}/profile.list/", url, HttpMethod.Get);
            return await _api.Request<CouchPotatoProfiles>(request);
        }

        public async Task<CouchPotatoMovies> GetMovies(string baseUrl, string apiKey, string[] status)
        {
            var request = new Request($"/api/{apiKey}/movie.list", baseUrl, HttpMethod.Get);

            request.AddQueryString("status",string.Join(",", status));
            request.OnBeforeDeserialization = json =>
            {
                json.Replace("[]", "{}");
            };
            return await _api.Request<CouchPotatoMovies>(request);
        }

        public async Task<CouchPotatoApiKey> GetApiKey(string baseUrl, string username, string password)
        {
            var request = new Request("getkey",baseUrl, HttpMethod.Get);
            request.AddQueryString("u",username.CalcuateMd5Hash());
            request.AddQueryString("p",password.CalcuateMd5Hash());

            return await _api.Request<CouchPotatoApiKey>(request);
        }
    }
}
