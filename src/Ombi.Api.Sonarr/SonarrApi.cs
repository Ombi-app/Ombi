using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ombi.Api.Sonarr.Models;
using Newtonsoft.Json;
using System.Linq;

namespace Ombi.Api.Sonarr
{
    public class SonarrApi : ISonarrApi
    {
        public SonarrApi(IApi api)
        {
            Api = api;
        }

        protected IApi Api { get; }
        protected virtual string ApiBaseUrl => "/api/";

        public async Task<IEnumerable<SonarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}profile", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);
            return await Api.Request<List<SonarrProfile>>(request);
        }

        public async Task<IEnumerable<SonarrRootFolder>> GetRootFolders(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}rootfolder", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);
            return await Api.Request<List<SonarrRootFolder>>(request);
        }

        /// <summary>
        /// Returns all the series in Sonarr
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SonarrSeries>> GetSeries(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}series", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);
            var results = await Api.Request<List<SonarrSeries>>(request);

            foreach (var s in results)
            {
                if (s.seasons.Length > 0)
                {
                    s.seasons.ToList().RemoveAt(0);
                }
            }
            return results;
        }

        /// <summary>
        /// Returns the series by the Sonarr ID
        /// </summary>
        /// <param name="id">Sonarr ID for the series</param>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<SonarrSeries> GetSeriesById(int id, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}series/{id}", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);
            var result = await Api.Request<SonarrSeries>(request);
            if (result?.seasons?.Length > 0)
            {
                result?.seasons?.ToList().RemoveAt(0);
            }
            return result;
        }

        /// <summary>
        /// Update the following series
        /// </summary>
        /// <param name="updated">The series to update</param>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<SonarrSeries> UpdateSeries(SonarrSeries updated, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}series/", baseUrl, HttpMethod.Put);
            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(updated);
            return await Api.Request<SonarrSeries>(request);
        }

        public async Task<NewSeries> AddSeries(NewSeries seriesToAdd, string apiKey, string baseUrl)
        {
            if (!string.IsNullOrEmpty(seriesToAdd.Validate()))
            {
                return new NewSeries { ErrorMessages = new List<string> { seriesToAdd.Validate() } };
            }
            var request = new Request($"{ApiBaseUrl}series/", baseUrl, HttpMethod.Post);

            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(seriesToAdd);
            try
            {

                return await Api.Request<NewSeries>(request);
            }
            catch (JsonSerializationException)
            {
                var error = await Api.Request<List<SonarrError>>(request);
                var messages = error?.Select(x => x.errorMessage).ToList();
                return new NewSeries { ErrorMessages = messages };
            }
        }

        /// <summary>
        /// Returns the episodes for the series
        /// </summary>
        /// <param name="seriesId">The Sonarr SeriesId value</param>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Episode>> GetEpisodes(int seriesId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}Episode?seriesId={seriesId}", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);
            return await Api.Request<List<Episode>>(request);
        }

        /// <summary>
        /// Returns the episode for the series
        /// </summary>
        /// <param name="episodeId">The Sonarr Episode ID</param>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<Episode> GetEpisodeById(int episodeId, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}Episode/{episodeId}", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);
            return await Api.Request<Episode>(request);
        }

        public async Task<EpisodeUpdateResult> UpdateEpisode(Episode episodeToUpdate, string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}Episode/", baseUrl, HttpMethod.Put);
            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(episodeToUpdate);
            return await Api.Request<EpisodeUpdateResult>(request);
        }

        /// <summary>
        /// Search for a list of episodes
        /// </summary>
        /// <param name="episodeIds">The episodes to search for</param>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<bool> EpisodeSearch(int[] episodeIds, string apiKey, string baseUrl)
        {
            var result = await Command(apiKey, baseUrl, new { name = "EpisodeSearch", episodeIds });
            return result != null;
        }

        /// <summary>
        /// Search for all episodes of a particular season
        /// </summary>
        /// <param name="seriesId">Series to search for</param>
        /// <param name="seasonNumber">Season to get all episodes</param>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<bool> SeasonSearch(int seriesId, int seasonNumber, string apiKey, string baseUrl)
        {
            var result = await Command(apiKey, baseUrl, new { name = "SeasonSearch", seriesId, seasonNumber });
            return result != null;
        }

        /// <summary>
        /// Search for all episodes in a series
        /// </summary>
        /// <param name="seriesId">Series to search for</param>
        /// <param name="apiKey"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        public async Task<bool> SeriesSearch(int seriesId, string apiKey, string baseUrl)
        {
            var result = await Command(apiKey, baseUrl, new { name = "SeriesSearch", seriesId });
            return result != null;
        }

        private async Task<CommandResult> Command(string apiKey, string baseUrl, object body)
        {
            var request = new Request($"{ApiBaseUrl}Command/", baseUrl, HttpMethod.Post);
            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(body);
            return await Api.Request<CommandResult>(request);
        }

        public async Task<SystemStatus> SystemStatus(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}system/status", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);

            return await Api.Request<SystemStatus>(request);
        }

        public async Task<bool> SeasonPass(string apiKey, string baseUrl, SonarrSeries series)
        {
            var seasonPass = new SeasonPass
            {
                series = new []
                {
                    series
                },
                monitoringOptions = new Monitoringoptions
                {
                    ignoreEpisodesWithFiles = false,
                    ignoreEpisodesWithoutFiles = false,
                }
            };
            var request = new Request($"{ApiBaseUrl}seasonpass", baseUrl, HttpMethod.Post);
            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(seasonPass);

            var content = await Api.RequestContent(request);
            return content.Equals("ok", StringComparison.CurrentCultureIgnoreCase);
        }

        public async Task<List<Tag>> GetTags(string apiKey, string baseUrl)
        {
            var request = new Request($"{ApiBaseUrl}tag", baseUrl, HttpMethod.Get);
            request.AddHeader("X-Api-Key", apiKey);

            return await Api.Request<List<Tag>>(request);
        }
    }
}
