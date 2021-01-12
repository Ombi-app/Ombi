using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ombi.Api.TheMovieDb.Models
{
    public class WatchProviders
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("results")]
        public Results Results { get; set; }
    }

    public class Results
    {
        public WatchProviderData AR { get; set; }
        public WatchProviderData AT { get; set; }
        public WatchProviderData AU { get; set; }
        public WatchProviderData BE { get; set; }
        public WatchProviderData BR { get; set; }
        public WatchProviderData CA { get; set; }
        public WatchProviderData CH { get; set; }
        public WatchProviderData CL { get; set; }
        public WatchProviderData CO { get; set; }
        public WatchProviderData CZ { get; set; }
        public WatchProviderData DE { get; set; }
        public WatchProviderData DK { get; set; }
        public WatchProviderData EC { get; set; }
        public WatchProviderData EE { get; set; }
        public WatchProviderData ES { get; set; }
        public WatchProviderData FI { get; set; }
        public WatchProviderData FR { get; set; }
        public WatchProviderData GB { get; set; }
        public WatchProviderData GR { get; set; }
        public WatchProviderData HU { get; set; }
        public WatchProviderData ID { get; set; }
        public WatchProviderData IE { get; set; }
        public WatchProviderData IN { get; set; }
        public WatchProviderData IT { get; set; }
        public WatchProviderData JP { get; set; }
        public WatchProviderData KR { get; set; }
        public WatchProviderData LT { get; set; }
        public WatchProviderData LV { get; set; }
        public WatchProviderData MX { get; set; }
        public WatchProviderData MY { get; set; }
        public WatchProviderData NL { get; set; }
        public WatchProviderData NO { get; set; }
        public WatchProviderData NZ { get; set; }
        public WatchProviderData PE { get; set; }
        public WatchProviderData PH { get; set; }
        public WatchProviderData PL { get; set; }
        public WatchProviderData PT { get; set; }
        public WatchProviderData RU { get; set; }
        public WatchProviderData SE { get; set; }
        public WatchProviderData SG { get; set; }
        public WatchProviderData TH { get; set; }
        public WatchProviderData TR { get; set; }
        public WatchProviderData US { get; set; }
        public WatchProviderData VE { get; set; }
        public WatchProviderData ZA { get; set; }
    }

    public class WatchProviderData
    {
        public string link { get; set; }
        [JsonProperty("flatrate")]
        public List<StreamData> StreamInformation { get; set; }
    }

    public class StreamData
    {
        public int display_priority { get; set; }
        public string logo_path { get; set; }
        public int provider_id { get; set; }
        public string provider_name { get; set; }
    }
}
