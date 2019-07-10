using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Api.MusicBrainz.Models.Browse
{
    public class CoverArtArchive
    {
        public bool back { get; set; }
        public bool artwork { get; set; }
        public bool darkened { get; set; }
        public int count { get; set; }
        public bool front { get; set; }
    }

    public class TextRepresentation
    {
        public string script { get; set; }
        public string language { get; set; }
    }

    public class Recording
    {
        public int length { get; set; }
        public string disambiguation { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public bool video { get; set; }
    }

    public class Track
    {
        public string title { get; set; }
        public Recording recording { get; set; }
        public string number { get; set; }
        public string id { get; set; }
        public int? length { get; set; }
        public int position { get; set; }
    }

    public class Medium
    {
        [JsonProperty(PropertyName = "track-count")]
        public int TrackCount { get; set; }
        public string title { get; set; }
        [JsonProperty(PropertyName = "track-offset")]
        public int TrackOffset { get; set; }
        public int position { get; set; }
        [JsonProperty(PropertyName = "format-id")]
        public string FormatId { get; set; }
        public string format { get; set; }
        public List<Track> tracks { get; set; }
    }

    public class Area
    {
        public string id { get; set; }
        [JsonProperty(PropertyName = "sort-name")]
        public string SortName { get; set; }
        public string disambiguation { get; set; }
        public string name { get; set; }
        [JsonProperty(PropertyName = "iso-3166-1-codes")]
        public List<string> Iso31661Codes { get; set; }
        [JsonProperty(PropertyName = "iso-3166-2-codes")]
        public List<string> Iso31662Codes { get; set; }
    }

    public class ReleaseEvent
    {
        public string date { get; set; }
        public Area area { get; set; }
    }

    public class Release
    {
        public string quality { get; set; }
        public string asin { get; set; }
        public string date { get; set; }
        public string status { get; set; }
        public string barcode { get; set; }
        [JsonProperty(PropertyName = "cover-art-archive")]
        public CoverArtArchive CoverArtArchive { get; set; }
        public string packaging { get; set; }
        [JsonProperty(PropertyName = "packaging-id")]
        public string PackagingId { get; set; }
        [JsonProperty(PropertyName = "status-id")]
        public string StatusId { get; set; }
        public string disambiguation { get; set; }
        public string country { get; set; }
        [JsonProperty(PropertyName = "text-representation")]
        public TextRepresentation TextRepresentation { get; set; }
        public string title { get; set; }
        public List<Medium> media { get; set; }
        public string id { get; set; }
        [JsonProperty(PropertyName = "release-events")]
        public List<ReleaseEvent> ReleaseEvents { get; set; }
    }

    public class ReleaseResult
    {
        [JsonProperty(PropertyName = "release-count")]
        public int ReleaseCount { get; set; }
        [JsonProperty(PropertyName = "release-offset")]
        public int ReleaseOffset { get; set; }
        public List<Release> releases { get; set; }
    }
}