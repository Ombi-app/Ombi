using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Api.Service.Models
{

    public class Updates
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("updateVersionString")]
        public string UpdateVersionString { get; set; }
        [JsonProperty("updateVersion")]
        public int UpdateVersion { get; set; }
        [JsonProperty("updateDate")]
        public DateTime UpdateDate { get; set; }
        [JsonProperty("changeLogs")]
        public List<Changelog> ChangeLogs { get; set; }
        [JsonProperty("downloads")]
        public List<Download> Downloads { get; set; }
    }

    public class Changelog
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("descripion")]
        public string Descripion { get; set; }
        [JsonProperty("updateId")]
        public int UpdateId { get; set; }
    }

    public class Download
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("updateId")]
        public int UpdateId { get; set; }
    }
}