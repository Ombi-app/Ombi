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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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


        public async Task<SickRageTvAdd> AddSeries(int tvdbId, int seasonCount, int[] seasons, string quality, string apiKey,
            Uri baseUrl)
        {
            var futureStatus = seasons.Length > 0 && !seasons.Any(x => x == seasonCount) ? SickRageStatus.Skipped : SickRageStatus.Wanted;
            var status = seasons.Length > 0 ? SickRageStatus.Skipped : SickRageStatus.Wanted;

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


            if (obj.result != "failure")
            {
                var sw = new Stopwatch();
                sw.Start();

                // Check to see if it's been added yet.
                var showInfo = new SickRageShowInformation { message = "Show not found" };
                while (showInfo.message.Equals("Show not found", StringComparison.CurrentCultureIgnoreCase))
                {
                    showInfo = CheckShowHasBeenAdded(tvdbId, apiKey, baseUrl);
                    if (sw.ElapsedMilliseconds > 30000) // Break out after 30 seconds, it's not going to get added
                    {
                        Log.Warn("Couldn't find out if the show had been added after 10 seconds. I doubt we can change the status to wanted.");
                        break;
                    }
                }
                sw.Stop();
            }


            if (seasons.Length > 0)
            {
                //handle the seasons requested
                foreach (var s in seasons)
                {
                    var result = await AddSeason(tvdbId, s, apiKey, baseUrl);
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

        public async Task<SickRageTvAdd> AddSeason(int tvdbId, int season, string apiKey, Uri baseUrl)
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

            await Task.Run(() => Thread.Sleep(2000));
            return await Task.Run(() => Api.Execute<SickRageTvAdd>(request, baseUrl)).ConfigureAwait(false);
        }


        public SickRageShowInformation CheckShowHasBeenAdded(int tvdbId, string apiKey, Uri baseUrl)
        {
            var request = new RestRequest
            {
                Resource = "/api/{apiKey}/?cmd=show",
                Method = Method.GET
            };
            request.AddUrlSegment("apiKey", apiKey);
            request.AddQueryParameter("tvdbid", tvdbId.ToString());

            var obj = Api.Execute<SickRageShowInformation>(request, baseUrl);

            return obj;
        }
    }
}