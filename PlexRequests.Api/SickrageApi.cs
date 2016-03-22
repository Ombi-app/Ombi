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
using NLog;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.SickRage;
using PlexRequests.Helpers;
using RestSharp;

namespace PlexRequests.Api
{
    public class SickrageApi : ISickRageApi
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public SickrageApi()
        {
            Api = new ApiRequest();
        }

        private ApiRequest Api { get; }


        public SickRageTvAdd AddSeries(int tvdbId, bool latest, int[] seasons, string quality, string apiKey,
            Uri baseUrl)
        {
            string status;
            var futureStatus = SickRageStatus.Wanted;

            status = latest || seasons.Length > 0 ? SickRageStatus.Skipped : SickRageStatus.Wanted;

            var request = new RestRequest
            {
                Resource = "/api/{apiKey}/?cmd=show.addnew",
                Method = Method.GET
            };
            request.AddUrlSegment("apiKey", apiKey);
            request.AddQueryParameter("tvdbid", tvdbId.ToString());
            request.AddQueryParameter("status", status);
            request.AddQueryParameter("future_status", futureStatus);
            if (!quality.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                request.AddQueryParameter("initial", quality);
            }

            var obj = Api.Execute<SickRageTvAdd>(request, baseUrl);

            if (!latest && seasons.Length > 0 && obj.result != "failure")
            {
                //handle the seasons requested
                foreach (int s in seasons)
                {
                    var result = AddSeason(tvdbId, s, apiKey, baseUrl);
                    Log.Trace("SickRage adding season results: ");
                    Log.Trace(result.DumpJson());
                }
            }

            return obj;
        }

        public SickRagePing Ping(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest
            {
                Resource = "/api/{apiKey}/?cmd=sb.ping",
                Method = Method.GET
            };

            request.AddUrlSegment("apiKey", apiKey);
            var obj = Api.ExecuteJson<SickRagePing>(request, baseUrl);

            return obj;
        }

        public SickRageTvAdd AddSeason(int tvdbId, int season, string apiKey, Uri baseUrl)
        {
            var request = new RestRequest
            {
                Resource = "/api/{apiKey}/?cmd=episode.setstatus",
                Method = Method.GET
            };
            request.AddUrlSegment("apiKey", apiKey);
            request.AddQueryParameter("tvdbid", tvdbId.ToString());
            request.AddQueryParameter("season", season.ToString());
            request.AddQueryParameter("status", SickRageStatus.Wanted);

            var obj = Api.Execute<SickRageTvAdd>(request, baseUrl);

            return obj;
        }
    }
}