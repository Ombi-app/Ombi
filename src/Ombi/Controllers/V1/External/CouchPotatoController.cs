using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ombi.Api.CouchPotato;
using Ombi.Api.CouchPotato.Models;
using Ombi.Attributes;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Controllers.V1.External
{
    [Admin]
    [ApiV1]
    [Produces("application/json")]
    public class CouchPotatoController
    {
        public CouchPotatoController(ICouchPotatoApi api)
        {
            _api = api;
        }

        private readonly ICouchPotatoApi _api;

        [HttpPost("profile")]
        public async Task<CouchPotatoProfiles> GetQualityProfiles([FromBody] CouchPotatoSettings settings)
        {
            var profiles = await _api.GetProfiles(settings.FullUri, settings.ApiKey);

            return profiles;
        }

        [HttpPost("apikey")]
        public async Task<CouchPotatoApiKey> GetApiKey([FromBody] CouchPotatoSettings settings)
        {
            var apiKey = await _api.GetApiKey(settings.FullUri, settings.Username, settings.Password);
            return apiKey;
        }
    }
}