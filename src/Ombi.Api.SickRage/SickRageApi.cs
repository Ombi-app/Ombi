using System;
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

        public async Task<SickRageTvAdd> AddSeries(int tvdbId, string quality, string status, string apiKey, string baseUrl)
        {
            var request = new Request($"/api/{apiKey}/?cmd=show.addnew", baseUrl, HttpMethod.Get);

            request.AddQueryString("tvdbid", tvdbId.ToString());
            request.AddQueryString("status", status);

            if (string.IsNullOrEmpty(quality))
            {
                quality = "default";
            }
            if (!quality.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                request.AddQueryString("initial", quality);
            }

            var obj = await _api.Request<SickRageTvAdd>(request);

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

        public async Task<SickRageShowInformation> GetShow(int tvdbid, string apikey, string baseUrl)
        {
            var request = new Request($"/api/{apikey}/?cmd=show", baseUrl, HttpMethod.Get);
            request.AddQueryString("tvdbid", tvdbid.ToString());

            return await _api.Request<SickRageShowInformation>(request);
        }

        public async Task<SickRageEpisodes> GetEpisodesForSeason(int tvdbid, int season, string apikey, string baseUrl)
        {
            var request = new Request($"/api/{apikey}/?cmd=show.seasons", baseUrl, HttpMethod.Get);
            request.AddQueryString("tvdbid", tvdbid.ToString());
            request.AddQueryString("season", season.ToString());

            return await _api.Request<SickRageEpisodes>(request);
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
        public async Task<SickRageEpisodeSetStatus> SetEpisodeStatus(string apiKey, string baseUrl, int tvdbid, string status, int season, int episode = -1)
        {
            var request = new Request($"/api/{apiKey}/?cmd=episode.setstatus", baseUrl, HttpMethod.Get);
            request.AddQueryString("tvdbid", tvdbid.ToString());
            request.AddQueryString("status", status);
            request.AddQueryString("season", season.ToString());

            if (episode != -1)
            {
                request.AddQueryString("episode", episode.ToString());
            }

            return await _api.Request<SickRageEpisodeSetStatus>(request);
        }

        public async Task<SeasonList> GetSeasonList(int tvdbId, string apikey, string baseurl)
        {
            var request = new Request($"/api/{apikey}/?cmd=show.seasonlist", baseurl, HttpMethod.Get);
            request.AddQueryString("tvdbid", tvdbId.ToString());

            return await _api.Request<SeasonList>(request);
        }
    }
}
