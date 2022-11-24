using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ombi.Api.Sonarr.Models;
using Ombi.Api.Sonarr.Models.V3;

namespace Ombi.Api.Sonarr
{
    public class SonarrV3Api : SonarrApi, ISonarrV3Api
    {
        public SonarrV3Api(IApi api) : base(api)
        {
        }

        protected override string ApiBaseUrl => "/api/v3/";

        public async Task<IEnumerable<LanguageProfiles>> LanguageProfiles(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}languageprofile", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);

            return await Api.Request<List<LanguageProfiles>>(request);
        }

        public override async Task<IEnumerable<SonarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}qualityprofile", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);
            return await Api.Request<List<SonarrProfile>>(request);
        }

        public Task<Tag> CreateTag(string apiKey, string baseUrl, string tagName)
        {
            var request = new Request($"{ApiBaseUrl}tag", baseUrl, HttpMethod.Post);
            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(new { Label = tagName });

            return Api.Request<Tag>(request);
        }

        public Task<Tag> GetTag(int tagId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}tag/{tagId}", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);

            return Api.Request<Tag>(request);
        }

        public async Task<List<MonitoredEpisodeResult>> MonitorEpisode(int[] episodeIds, bool monitor, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}Episode/monitor", baseUrl, HttpMethod.Put);
            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(new { episodeIds = episodeIds, monitored = monitor });
            return await Api.Request<List<MonitoredEpisodeResult>>(request);
        }
    }
}
