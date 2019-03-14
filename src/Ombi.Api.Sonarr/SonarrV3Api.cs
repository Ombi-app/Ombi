using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}
