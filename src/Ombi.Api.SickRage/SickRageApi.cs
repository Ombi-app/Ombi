using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Api.SickRage.Models;

namespace Ombi.Api.SickRage
{
    public class SickRageApi : ISickRageApi
    {
        public SickRageApi(IApi api, ILogger<SickRageApi> log)
        {
            _api = api;
            _log = log;
        }

        private readonly IApi _api;
        private readonly ILogger<SickRageApi> _log;

        public async Task<SickRageSeasonList> VerifyShowHasLoaded(int tvdbId, string apiKey, string baseUrl)
        {
            var request = new Request($"/api/{apiKey}/?cmd=show.seasonlist", baseUrl, HttpMethod.Get);
            request.AddQueryString("tvdbid", tvdbId.ToString());

            return await _api.Request<SickRageSeasonList>(request);
        }

        public async Task<SickRageTvAdd> AddSeries(int tvdbId, int seasonCount, int[] seasons, string quality, string apiKey, string baseUrl)
        {
            var futureStatus = seasons.Length > 0 && seasons.All(x => x != seasonCount) ? SickRageStatus.Skipped : SickRageStatus.Wanted;
            var status = seasons.Length > 0 ? SickRageStatus.Skipped : SickRageStatus.Wanted;
            var request = new Request($"/api/{apiKey}/?cmd=show.addnew", baseUrl, HttpMethod.Get);

            request.AddQueryString("tvdbid", tvdbId.ToString());
            request.AddQueryString("status", status);
            request.AddQueryString("future_status", futureStatus);

            if (!quality.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                request.AddQueryString("initial", quality);
            }

            var obj = await _api.Request<SickRageTvAdd>(request);

            if (obj.result != "failure")
            {
                var sw = new Stopwatch();
                sw.Start();

                var seasonIncrement = 0;
                try
                {
                    while (seasonIncrement < seasonCount)
                    {
                        var seasonList = await VerifyShowHasLoaded(tvdbId, apiKey, baseUrl);
                        if (seasonList.result.Equals("failure"))
                        {
                            await Task.Delay(3000);
                            continue;
                        }
                        seasonIncrement = seasonList.Data?.Length ?? 0;

                        if (sw.ElapsedMilliseconds > 30000) // Break out after 30 seconds, it's not going to get added
                        {
                            _log.LogWarning("Couldn't find out if the show had been added after 10 seconds. I doubt we can change the status to wanted.");
                            break;
                        }
                    }
                    sw.Stop();
                }
                catch (Exception e)
                {
                    _log.LogCritical(e, "Exception thrown when getting the seasonList");
                }
            }

            try
            {
                if (seasons.Length > 0)
                {
                    //handle the seasons requested
                    foreach (var s in seasons)
                    {
                        var result = await AddSeason(tvdbId, s, apiKey, baseUrl);
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogCritical(e, "Exception when adding seasons:");
                throw;
            }

            return obj;
        }

        public async Task<SickRageTvAdd> AddSeason(int tvdbId, int season, string apiKey, string baseUrl)
        {
            var request = new Request($"/api/{apiKey}/?cmd=episode.setstatus", baseUrl, HttpMethod.Get);
            request.AddQueryString("tvdbid", tvdbId.ToString());
            request.AddQueryString("season", season.ToString());
            request.AddQueryString("status", SickRageStatus.Wanted);

            return await _api.Request<SickRageTvAdd>(request);
        }

        public async Task<SickRageShows> GetShows(string apiKey, string baseUrl)
        {
            var request = new Request($"/api/{apiKey}/?cmd=shows", baseUrl, HttpMethod.Get);

            return await _api.Request<SickRageShows>(request);
        }

        public async Task<SickRagePing> Ping(string apiKey, string baseUrl)
        {
            var request = new Request($"/api/{apiKey}/?cmd=sb.ping", baseUrl, HttpMethod.Get);

            return await _api.Request<SickRagePing>(request);
        }

        /// <summary>
        /// Sets the epsiode status e.g. wanted
        /// The episode number is optional, if not supplied it will set the whole season as the status passed in
        /// </summary>
        /// <returns></returns>
        public async Task<SickRageEpisodeStatus> SetEpisodeStatus(string apiKey, string baseUrl, int tvdbid, string status, int season, int episode = -1)
        {
            var request = new Request($"/api/{apiKey}/?cmd=episode.setstatus", baseUrl, HttpMethod.Get);
            request.AddQueryString("tvdbid", tvdbid.ToString());
            request.AddQueryString("status", status);
            request.AddQueryString("season", season.ToString());

            if (episode != -1)
            {
                request.AddQueryString("episode", episode.ToString());
            }

            return await _api.Request<SickRageEpisodeStatus>(request);
        }
    }
}
