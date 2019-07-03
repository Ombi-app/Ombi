using System;
using System.Collections.Generic;

namespace Ombi.Api.MusicBrainz.Models
{
    public class MusicBrainzResult<T>
    {
        public DateTime created { get; set; }
        public int count { get; set; }
        public int offset { get; set; }
        [JsonPropertyNameBasedOnItemClass]
        public List<T> Data { get; set; }
    }

}