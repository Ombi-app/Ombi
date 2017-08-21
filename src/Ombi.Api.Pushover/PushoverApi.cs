using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Pushover.Models;

namespace Ombi.Api.Pushover
{
    public class PushoverApi : IPushoverApi
    {
        public PushoverApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;
        private const string PushoverEndpoint = "https://api.pushover.net/1";

        public async Task<PushoverResponse> PushAsync(string accessToken, string message, string userToken)
        {
            var request = new Request($"messages.json?token={accessToken}&user={userToken}&message={message}", PushoverEndpoint, HttpMethod.Post);
            
            var result = await _api.Request<PushoverResponse>(request);
            return result;
        }
    }
}
