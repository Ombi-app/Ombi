﻿using System;
using System.Net.Mime;

namespace Ombi.Api.Lidarr.Models
{
    public class ArtistLookup
    {
        public string status { get; set; }
        public bool ended { get; set; }
        public string artistName { get; set; }
        public string foreignArtistId { get; set; }
        public int tadbId { get; set; }
        public int discogsId { get; set; }
        public string overview { get; set; }
        public string artistType { get; set; }
        public string disambiguation { get; set; }
        public Link[] links { get; set; }
        public Image[] images { get; set; }
        public string remotePoster { get; set; }
        public int qualityProfileId { get; set; }
        public int languageProfileId { get; set; }
        public int metadataProfileId { get; set; }
        public bool albumFolder { get; set; }
        public bool monitored { get; set; }
        public string cleanName { get; set; }
        public string sortName { get; set; }
        public object[] tags { get; set; }
        public DateTime added { get; set; }
        public Ratings ratings { get; set; }
        public Statistics statistics { get; set; }
    }
}