using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Mattermost.Models;

namespace Ombi.Api.Mattermost
{
    public class MattermostApi : IMattermostApi
    {
        public MattermostApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;

        public async Task<string> PushAsync(string webhook, MattermostBody message)
        {
            var request = new Request(string.Empty, webhook, HttpMethod.Post);

            request.AddJsonBody(message);

            var result = await _api.RequestContent(request);
            return result;
        }
    }
}
