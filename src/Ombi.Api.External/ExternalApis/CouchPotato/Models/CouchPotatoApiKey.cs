using Newtonsoft.Json;

namespace Ombi.Api.External.ExternalApis.CouchPotato.Models
{
    public class CouchPotatoApiKey
    {
        [JsonProperty("success")]
        public bool success { get; set; }
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
    }
}