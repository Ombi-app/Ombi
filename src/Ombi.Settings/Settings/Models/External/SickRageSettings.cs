using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Settings.Settings.Models.External
{
    public class SickRageSettings : ExternalSettings
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public string QualityProfile { get; set; }

        [JsonIgnore]
        public Dictionary<string, string> Qualities => new Dictionary<string, string>
        {
            { "default", "Use Default" },
            { "sdtv", "SD TV" },
            { "sddvd", "SD DVD" },
            { "hdtv", "HD TV" },
            { "rawhdtv", "Raw HD TV" },
            { "hdwebdl", "HD Web DL" },
            { "fullhdwebdl", "Full HD Web DL" },
            { "hdbluray", "HD Bluray" },
            { "fullhdbluray", "Full HD Bluray" }
        };
    }
}