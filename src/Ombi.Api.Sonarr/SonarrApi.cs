using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ombi.Api.Sonarr.Models;

namespace Ombi.Api.Sonarr
{
    public class SonarrApi : ISonarrApi
    {

        public SonarrApi(IApi api)
        {
            Api = api;
        }

        private IApi Api { get; }

        public async Task<IEnumerable<SonarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request("/api/profile", baseUrl, HttpMethod.Get);

            request.AddHeader("X-Api-Key", apiKey);

            return await Api.Request<List<SonarrProfile>>(request);
        }

        public async Task<IEnumerable<SonarrRootFolder>> GetRootFolders(string apiKey, string baseUrl)
        {
            var request = new Request("/api/rootfolder", baseUrl, HttpMethod.Get);

            request.AddHeader("X-Api-Key", apiKey);

            return await Api.Request<List<SonarrRootFolder>>(request);
        }
    }
}
