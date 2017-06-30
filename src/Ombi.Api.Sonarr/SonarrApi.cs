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

        private IApi Api { get; }

        public async Task<IEnumerable<SonarrProfile>> GetProfiles(string apiKey, string baseUrl)
        {
            var request = new Request("/api/profile", baseUrl, HttpMethod.Get);

            request.AddHeader("X-Api-Key", apiKey);

            return await Api.Request<List<SonarrProfile>>(request);
        }

        public async Task<IEnumerable<SonarrRootFolder>> GetRootFolders(string apiKey, string baseUrl)
        {
            var request = new Request("/api/rootfolder", baseUrl, HttpMethod.Get);

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
            var request = new Request("/api/series", baseUrl, HttpMethod.Get);

            request.AddHeader("X-Api-Key", apiKey);

            return await Api.Request<List<SonarrSeries>>(request);
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
            var request = new Request($"/api/series/{id}", baseUrl, HttpMethod.Get);

            request.AddHeader("X-Api-Key", apiKey);

            return await Api.Request<SonarrSeries>(request);
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
            var request = new Request("/api/series/", baseUrl, HttpMethod.Put);

            request.AddHeader("X-Api-Key", apiKey);
            request.AddJsonBody(updated);

            return await Api.Request<SonarrSeries>(request);
        }

        public async Task<NewSeries> AddSeries(NewSeries seriesToAdd, string apiKey, string baseUrl)
        {
            if(!string.IsNullOrEmpty(seriesToAdd.Validate()))
            {
                return new NewSeries { ErrorMessages = new List<string> { seriesToAdd.Validate() } };
            }
            var request = new Request("/api/series/", baseUrl, HttpMethod.Post);

            request.AddHeader("X-Api-Key", apiKey);
            try
            {

                return await Api.Request<NewSeries>(request);
            }
            catch (JsonSerializationException e)
            {
                var error = await Api.Request<List<SonarrError>>(request);
                var messages = error?.Select(x => x.errorMessage).ToList();
                messages?.ForEach(x => Log.Error(x));
                return new NewSeries { ErrorMessages = messages };
            }
        }

    }
}
