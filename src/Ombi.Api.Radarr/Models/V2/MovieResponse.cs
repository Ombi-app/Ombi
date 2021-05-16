using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace Ombi.Api.Radarr.Models
{ 
    public class MovieResponse
    {
        public string title { get; set; }
        public string originalTitle { get; set; }
        public Alternatetitle[] alternateTitles { get; set; }
        public int secondaryYearSourceId { get; set; }
        public string sortTitle { get; set; }
        public long sizeOnDisk { get; set; }
        public string status { get; set; }
        public string overview { get; set; }
        public DateTime inCinemas { get; set; }
        public DateTime physicalRelease { get; set; }
        public DateTime digitalRelease { get; set; }
        public Image[] images { get; set; }
        public string website { get; set; }
        public int year { get; set; }
        public bool hasFile { get; set; }
        public string youTubeTrailerId { get; set; }
        public string studio { get; set; }
        public string path { get; set; }
        public int qualityProfileId { get; set; }
        public bool monitored { get; set; }
        public string minimumAvailability { get; set; }
        public bool isAvailable { get; set; }
        public string folderName { get; set; }
        public int runtime { get; set; }
        public string cleanTitle { get; set; }
        public string imdbId { get; set; }
        public int tmdbId { get; set; }
        public string titleSlug { get; set; }
        public string certification { get; set; }
        public string[] genres { get; set; }
        public object[] tags { get; set; }
        public DateTime added { get; set; }
        public Ratings ratings { get; set; }
        public Moviefile movieFile { get; set; }
        public Collection collection { get; set; }
        public int id { get; set; }
    }


    public class Moviefile
    {
        public int movieId { get; set; }
        public string relativePath { get; set; }
        public string path { get; set; }
        public long size { get; set; }
        public DateTime dateAdded { get; set; }
        public string sceneName { get; set; }
        public int indexerFlags { get; set; }
        public V3.Quality quality { get; set; }
        public Mediainfo mediaInfo { get; set; }
        public string originalFilePath { get; set; }
        public bool qualityCutoffNotMet { get; set; }
        public Language[] languages { get; set; }
        public string releaseGroup { get; set; }
        public string edition { get; set; }
        public int id { get; set; }
    }

    public class Revision
    {
        public int version { get; set; }
        public int real { get; set; }
        public bool isRepack { get; set; }
    }

    public class Mediainfo
    {
        public string audioAdditionalFeatures { get; set; }
        public int audioBitrate { get; set; }
        public float audioChannels { get; set; }
        public string audioCodec { get; set; }
        public string audioLanguages { get; set; }
        public int audioStreamCount { get; set; }
        public int videoBitDepth { get; set; }
        public int videoBitrate { get; set; }
        public string videoCodec { get; set; }
        public float videoFps { get; set; }
        public string resolution { get; set; }
        public string runTime { get; set; }
        public string scanType { get; set; }
        public string subtitles { get; set; }
    }

    public class Language
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Collection
    {
        public string name { get; set; }
        public int tmdbId { get; set; }
        public object[] images { get; set; }
    }

    public class Alternatetitle
    {
        public string sourceType { get; set; }
        public int movieId { get; set; }
        public string title { get; set; }
        public int sourceId { get; set; }
        public int votes { get; set; }
        public int voteCount { get; set; }
        public Language1 language { get; set; }
        public int id { get; set; }
    }

    public class Language1
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}