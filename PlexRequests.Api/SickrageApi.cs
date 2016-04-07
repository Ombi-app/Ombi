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
using System.ComponentModel;
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

            Log.Trace("Future Status: {0}", futureStatus);
            Log.Trace("Current Status: {0}", status);

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
                Log.Trace("Settings quality to {0}", quality);
                request.AddQueryParameter("initial", quality);
            }

            Log.Trace("Entering `Execute<SickRageTvAdd>`");
            var obj = Api.Execute<SickRageTvAdd>(request, baseUrl);
            Log.Trace("Exiting `Execute<SickRageTvAdd>`");
            Log.Trace("obj Result:");
            Log.Trace(obj.DumpJson());

            if (obj.result != "failure")
            {
                var sw = new Stopwatch();
                sw.Start();

                var seasonIncrement = 0;
                var seasonList = new SickRageSeasonList();
                Log.Trace("while (seasonIncrement < seasonCount) where seasonCount = {0}", seasonCount);
                try
                {
                    while (seasonIncrement < seasonCount)
                    {
                        seasonList = VerifyShowHasLoaded(tvdbId, apiKey, baseUrl);
                        if (seasonList.result.Equals("failure"))
                        {
                            Thread.Sleep(3000);
                            continue;
                        }
                        seasonIncrement = seasonList.Data?.Length ?? 0;
                        Log.Trace("New seasonIncrement -> {0}", seasonIncrement);

                        if (sw.ElapsedMilliseconds > 30000) // Break out after 30 seconds, it's not going to get added
                        {
                            Log.Warn("Couldn't find out if the show had been added after 10 seconds. I doubt we can change the status to wanted.");
                            break;
                        }
                    }
                    sw.Stop();
                }
                catch (Exception e)
                {
                    Log.Error("Exception thrown when getting the seasonList");
                    Log.Error(e);
                }
            }
            Log.Trace("seasons.Length > 0 where seasons.Len -> {0}", seasons.Length);
            try
            {
                if (seasons.Length > 0)
                {
                    //handle the seasons requested
                    foreach (var s in seasons)
                    {
                        Log.Trace("Adding season {0}", s);
                        var result = await AddSeason(tvdbId, s, apiKey, baseUrl);
                        Log.Trace("SickRage adding season results: ");
                        Log.Trace(result.DumpJson());
                    }
                }
            }
            catch (Exception e)
            {
                Log.Trace("Exception when adding seasons:");
                Log.Error(e);
                throw;
            }

            Log.Trace("Finished with the API, returning the obj");
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

        public SickRageSeasonList VerifyShowHasLoaded(int tvdbId, string apiKey, Uri baseUrl)
        {
            Log.Trace("Entered `VerifyShowHasLoaded({0} <- id)`", tvdbId);
            var request = new RestRequest
            {
                Resource = "/api/{apiKey}/?cmd=show.seasonlist",
                Method = Method.GET
            };
            request.AddUrlSegment("apiKey", apiKey);
            request.AddQueryParameter("tvdbid", tvdbId.ToString());

            try
            {
                Log.Trace("Entering `ExecuteJson<SickRageSeasonList>`");
                var obj = Api.ExecuteJson<SickRageSeasonList>(request, baseUrl);
                Log.Trace("Exited `ExecuteJson<SickRageSeasonList>`");
                return obj;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new SickRageSeasonList();
            }
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
            return await Task.Run(() =>
            {
                Log.Trace("Entering `Execute<SickRageTvAdd>` in a new `Task<T>`");
                var result = Api.Execute<SickRageTvAdd>(request, baseUrl);

                Log.Trace("Exiting `Execute<SickRageTvAdd>` and yeilding `Task<T>` result");
                return result;
            }).ConfigureAwait(false);
        }
    }
}