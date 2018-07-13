using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Api.FanartTv.Models;

namespace Ombi.Api.FanartTv
{
    public class FanartTvApi : IFanartTvApi
    {
        public FanartTvApi(IApi api)
        {
            Api = api;
        }

        private string Endpoint => "https://webservice.fanart.tv/v3";
        private IApi Api { get; }

        public async Task<TvResult> GetTvImages(int tvdbId, string token)
        {
            var request = new Request($"tv/{tvdbId}", Endpoint, HttpMethod.Get);
            request.AddHeader("api-key", token);
            request.IgnoreErrors = true;
            try
            {
                return await Api.Request<TvResult>(request);
            }
            catch (JsonSerializationException)
            {
                // Usually this is when it's not found
                return null;
            }
        }

        public async Task<MovieResult> GetMovieImages(string movieOrImdbId, string token)
        {
            var request = new Request($"movies/{movieOrImdbId}", Endpoint, HttpMethod.Get);
            request.AddHeader("api-key", token);
            request.IgnoreErrors = true;

            return await Api.Request<MovieResult>(request);
        }
    }
}
