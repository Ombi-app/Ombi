using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ombi.Api.Discord.Models;

namespace Ombi.Api.Discord
{
    public class DiscordApi : IDiscordApi
    {
        public DiscordApi(IApi api)
        {
            Api = api;
        }
        
        private const string _baseUrl = "https://discordapp.com/api/";
        private IApi Api { get; }
        
        public async Task SendMessage(DiscordWebhookBody body, string webhookId, string webhookToken)
        {
            var request = new Request($"webhooks/{webhookId}/{webhookToken}", _baseUrl, HttpMethod.Post);

            request.AddJsonBody(body);

            request.ApplicationJsonContentType();

            var response = await Api.Request(request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new DiscordException(content, response.StatusCode);
            }
        }

        public class DiscordException : Exception
        {
            public DiscordException(string content, HttpStatusCode code) : base($"Exception when calling Discord with status code {code} and message: {content}")
            {
            }
        }
    }
}
