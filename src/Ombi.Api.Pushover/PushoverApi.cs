using System;
using System.Net;
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

        public async Task<PushoverResponse> PushAsync(string accessToken, string message, string userToken,  sbyte priority, string sound)
        {
            if (message.Contains("'"))
            {
                message = message.Replace("'", "&#39;");
            }
            var request = new Request($"messages.json?token={accessToken}&user={userToken}&priority={priority}&sound={sound}&message={WebUtility.HtmlEncode(message)}", PushoverEndpoint, HttpMethod.Post);
            
            var result = await _api.Request<PushoverResponse>(request);
            return result;
        }
    }
}
