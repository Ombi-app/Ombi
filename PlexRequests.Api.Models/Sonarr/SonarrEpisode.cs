#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SonarrEpisode.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

namespace PlexRequests.Api.Models.Sonarr
{

    public class Revision
    {
        public int version { get; set; }
        public int real { get; set; }
    }

    public class EpisodeFile
    {
        public int seriesId { get; set; }
        public int seasonNumber { get; set; }
        public string relativePath { get; set; }
        public string path { get; set; }
        public long size { get; set; }
        public string dateAdded { get; set; }
        public Quality quality { get; set; }
        public bool qualityCutoffNotMet { get; set; }
        public int id { get; set; }
    }



    public class SonarrEpisode
    {
        public int seriesId { get; set; }
        public int episodeFileId { get; set; }
        public int seasonNumber { get; set; }
        public int episodeNumber { get; set; }
        public string title { get; set; }
        public string airDate { get; set; }
        public string airDateUtc { get; set; }
        public string overview { get; set; }
        public EpisodeFile episodeFile { get; set; }
        public bool hasFile { get; set; }
        public bool monitored { get; set; }
        public int absoluteEpisodeNumber { get; set; }
        public bool unverifiedSceneNumbering { get; set; }
        public Series series { get; set; }
        public int id { get; set; }
    }



}