﻿using System;
using System.Net.Mime;

namespace Ombi.Api.Lidarr.Models
{
    public class ArtistAdd
    {
        public string status { get; set; }
        public bool ended { get; set; }
        public string artistName { get; set; }
        public string foreignArtistId { get; set; }
        public int tadbId { get; set; }
        public int discogsId { get; set; }
        public string overview { get; set; }
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
        public Addoptions addOptions { get; set; }
        public string rootFolderPath { get; set; }
    }

    public class Addoptions
    {
        /// <summary>
        /// Future = 1
        /// Missing = 2
        /// Existing = 3
        /// First = 5
        /// Latest = 4
        /// None = 6
        /// </summary>
        public int selectedOption { get; set; }
        public bool monitored { get; set; }
        public bool searchForMissingAlbums { get; set; }
        public string[] AlbumsToMonitor { get; set; } // Uses the MusicBrainzAlbumId!
    }
}