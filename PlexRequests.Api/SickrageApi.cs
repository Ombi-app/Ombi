#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SickrageApi.cs
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
using Ombi.Api.Interfaces;
using Ombi.Api.Models.SickRage;
using Ombi.Helpers;
using Ombi.Helpers.Exceptions;
using RestSharp;

namespace Ombi.Api
{
    public class SickrageApi : ISickRageApi
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public SickrageApi()
        {
            Api = new ApiRequest();
        }

        private ApiRequest Api { get; }

        public SickRageSeasonList VerifyShowHasLoaded(int tvdbId, string apiKey, Uri baseUrl)
        {
            Log.Trace("Entered `VerifyShowHasLoaded({0} <- id)`", tvdbId);
            var request = new RestRequest { Resource = "/api/{apiKey}/?cmd=show.seasonlist", Method = Method.GET };
            request.AddUrlSegment("apiKey", apiKey);
            request.AddQueryParameter("tvdbid", tvdbId.ToString());

            try
            {
                var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling VerifyShowHasLoaded for SR, Retrying {0}", timespan), null);

                var obj = policy.Execute(() => Api.ExecuteJson<SickRageSeasonList>(request, baseUrl));
                return obj;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return new SickRageSeasonList();
            }
        }

        public async Task<SickRageTvAdd> AddSeries(int tvdbId, int seasonCount, int[] seasons, string quality, string apiKey, Uri baseUrl)
        {
            var futureStatus = seasons.Length > 0 && !seasons.Any(x => x == seasonCount) ? SickRageStatus.Skipped : SickRageStatus.Wanted;
            var status = seasons.Length > 0 ? SickRageStatus.Skipped : SickRageStatus.Wanted;

            Log.Trace("Future Status: {0}", futureStatus);
            Log.Trace("Current Status: {0}", status);

            var request = new RestRequest { Resource = "/api/{apiKey}/?cmd=show.addnew", Method = Method.GET };
            request.AddUrlSegment("apiKey", apiKey);
            request.AddQueryParameter("tvdbid", tvdbId.ToString());
            request.AddQueryParameter("status", status);
            request.AddQueryParameter("future_status", futureStatus);
            if (!quality.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                Log.Trace("Settings quality to {0}", quality);
                request.AddQueryParameter("initial", quality);
            }

            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling AddSeries for SR, Retrying {0}", timespan), null);

            var obj = policy.Execute(() => Api.Execute<SickRageTvAdd>(request, baseUrl));
            Log.Trace("obj Result:");
            Log.Trace(obj.DumpJson());

            if (obj.result != "failure")
            {
                var sw = new Stopwatch();
                sw.Start();

                var seasonIncrement = 0;
                var seasonList = new SickRageSeasonList();
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
            var request = new RestRequest { Resource = "/api/{apiKey}/?cmd=sb.ping", Method = Method.GET };

            var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling Ping for SR, Retrying {0}", timespan), null);

            request.AddUrlSegment("apiKey", apiKey);
            var obj = policy.Execute(() => Api.ExecuteJson<SickRagePing>(request, baseUrl));

            return obj;
        }

        public async Task<SickRageTvAdd> AddSeason(int tvdbId, int season, string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/{apiKey}/?cmd=episode.setstatus", Method = Method.GET };
            request.AddUrlSegment("apiKey", apiKey);
            request.AddQueryParameter("tvdbid", tvdbId.ToString());
            request.AddQueryParameter("season", season.ToString());
            request.AddQueryParameter("status", SickRageStatus.Wanted);

            await Task.Run(() => Thread.Sleep(2000));
            return await Task.Run(
                () =>
                {
                    var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling AddSeason for SR, Retrying {0}", timespan), null);

                    var result = policy.Execute(() => Api.Execute<SickRageTvAdd>(request, baseUrl));

                    return result;
                }).ConfigureAwait(false);
        }

        public async Task<SickrageShows> GetShows(string apiKey, Uri baseUrl)
        {
            var request = new RestRequest { Resource = "/api/{apiKey}/?cmd=shows", Method = Method.GET };
            request.AddUrlSegment("apiKey", apiKey);

            return await Task.Run(
                () =>
                {
                    try
                    {
                        var policy = RetryHandler.RetryAndWaitPolicy((exception, timespan) => Log.Error(exception, "Exception when calling GetShows for SR, Retrying {0}", timespan), new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) });

                        return policy.Execute(() => Api.Execute<SickrageShows>(request, baseUrl));
                    }
                    catch (ApiRequestException)
                    {
                        Log.Error("There has been a API exception when Getting the Sickrage shows");
                        return null;
                    }
                }).ConfigureAwait(false);
        }
    }
}