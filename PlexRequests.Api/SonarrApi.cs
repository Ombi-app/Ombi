#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CouchPotatoApi.cs
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
using System.Linq;
using NLog;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Sonarr;
using RestSharp;

namespace PlexRequests.Api
{
    public class SonarrApi : ISonarrApi
    {
        public SonarrApi()
        {
            Api = new ApiRequest();
        }
        private ApiRequest Api { get; set; }
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public List<SonarrProfile> GetProfiles(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/profile", Method = Method.GET };

            request.AddHeader("X-Api-Key", apiKey);

            var obj = Api.ExecuteJson<List<SonarrProfile>>(request, baseUrl);

            return obj;
        }

        public SonarrAddSeries AddSeries(int tvdbId, string title, int qualityId, bool seasonFolders, string rootPath, int seasonCount, int[] seasons, string apiKey, Uri baseUrl)
        {

            var request = new RestRequest
            {
                Resource = "/api/Series?",
                Method = Method.POST
            };

            var options = new SonarrAddSeries
            {
                seasonFolder = seasonFolders,
                title = title,
                qualityProfileId = qualityId,
                tvdbId = tvdbId,
                titleSlug = title,
                seasons = new List<Season>(),
                rootFolderPath = rootPath
            };


            for (var i = 1; i <= seasonCount; i++)
            {
                var season = new Season
                {
                    seasonNumber = i,
                    monitored = seasons.Length == 0 || seasons.Any(x => x == i)
                };
                options.seasons.Add(season);
            }

            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(options);

            var obj = Api.ExecuteJson<SonarrAddSeries>(request, baseUrl);

            return obj;
        }

        public SystemStatus SystemStatus(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/system/status", Method = Method.GET };
            request.AddHeader("X-Api-Key", apiKey);

            var obj = Api.ExecuteJson<SystemStatus>(request, baseUrl);

            return obj;
        }
    }
}