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

        public async Task PushAsync(string webhook, MattermostMessage message)
        {
            var client = new MatterhookClient(webhook);
            await client.PostAsync(_api, message);
        }
    }
}
