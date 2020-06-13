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
        
        private const string BaseUrl = "https://discordapp.com/api/";
        private IApi Api { get; }
        
        public async Task SendMessage(DiscordWebhookBody body, string webhookId, string webhookToken)
        {
            var request = new Request($"webhooks/{webhookId}/{webhookToken}", BaseUrl, HttpMethod.Post);

            request.AddJsonBody(body);

            request.ApplicationJsonContentType();

            await Api.Request(request);
        }
    }
}
