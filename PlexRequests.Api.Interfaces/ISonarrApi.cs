#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ISonarrApi.cs
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
using System;
using System.Collections.Generic;

using PlexRequests.Api.Models.Sonarr;

namespace PlexRequests.Api.Interfaces
{
    public interface ISonarrApi
    {
        List<SonarrProfile> GetProfiles(string apiKey, Uri baseUrl);

        SonarrAddSeries AddSeries(int tvdbId, string title, int qualityId, bool seasonFolders, string rootPath,
            int seasonCount, int[] seasons, string apiKey, Uri baseUrl, bool monitor = true,
            bool searchForMissingEpisodes = false);

        SystemStatus SystemStatus(string apiKey, Uri baseUrl);

        List<Series> GetSeries(string apiKey, Uri baseUrl);
        Series GetSeries(string seriesId, string apiKey, Uri baseUrl);
        IEnumerable<SonarrEpisodes> GetEpisodes(string seriesId, string apiKey, Uri baseUrl);
        SonarrEpisode GetEpisode(string episodeId, string apiKey, Uri baseUrl);
        SonarrEpisode UpdateEpisode(SonarrEpisode episodeInfo, string apiKey, Uri baseUrl);
        SonarrEpisodes UpdateEpisode(SonarrEpisodes episodeInfo, string apiKey, Uri baseUrl);
        SonarrAddEpisodeResult SearchForEpisodes(int[] episodeIds, string apiKey, Uri baseUrl);
        Series UpdateSeries(Series series, string apiKey, Uri baseUrl);
        SonarrSeasonSearchResult SearchForSeason(int seriesId, int seasonNumber, string apiKey, Uri baseUrl);
        SonarrSeriesSearchResult SearchForSeries(int seriesId, string apiKey, Uri baseUrl);
    }
}