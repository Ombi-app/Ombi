using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Discord.Models;

namespace Ombi.Api.Discord
{
    public class DiscordApi : IDiscordApi
    {
        public DiscordApi(IApi api)
        {
            Api = api;
        }
        
        private string Endpoint => "https://discordapp.com/api/"; //webhooks/270828242636636161/lLysOMhJ96AFO1kvev0bSqP-WCZxKUh1UwfubhIcLkpS0DtM3cg4Pgeraw3waoTXbZii
        private IApi Api { get; }
        
        public async Task SendMessage(DiscordWebhookBody body, string webhookId, string webhookToken)
        {
            var request = new Request(Endpoint, $"webhooks/{webhookId}/{webhookToken}", HttpMethod.Post);
          
            request.AddJsonBody(body);

            request.AddHeader("Content-Type", "application/json");

            await Api.Request(request);
        }
    }
}
