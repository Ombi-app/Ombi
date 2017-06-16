using System.Threading.Tasks;
using Ombi.Api.Discord.Models;

namespace Ombi.Api.Discord
{
    public interface IDiscordApi
    {
        Task SendMessage(DiscordWebhookBody body, string webhookId, string webhookToken);
    }
}