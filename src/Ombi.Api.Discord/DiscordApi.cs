using System.Net.Http;
using System.Threading.Tasks;
using Ombi.Api.Discord.Models;

namespace Ombi.Api.Discord
{
    public class DiscordApi : IDiscordApi
    {
        public DiscordApi()
        {
            Api = new Api();
        }
        
        private string Endpoint => "https://discordapp.com/api/"; //webhooks/270828242636636161/lLysOMhJ96AFO1kvev0bSqP-WCZxKUh1UwfubhIcLkpS0DtM3cg4Pgeraw3waoTXbZii
        private Api Api { get; }
        
        public async Task SendMessage(string message, string webhookId, string webhookToken, string username = null)
        {
            var request = new Request(Endpoint, $"webhooks/{webhookId}/{webhookToken}", HttpMethod.Post);
          
            var body = new DiscordWebhookBody
            {
                content = message,
                username = username
            };
            request.AddJsonBody(body);

            request.AddHeader("Content-Type", "application/json");

            await Api.Request(request);
        }
    }
}
