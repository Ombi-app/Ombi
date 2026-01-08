using System.Threading.Tasks;
using Ombi.Api.External.NotificationServices.Discord.Models;

namespace Ombi.Api.External.NotificationServices.Discord
{
    public interface IDiscordApi
    {
        Task SendMessage(DiscordWebhookBody body, string webhookId, string webhookToken);
    }
}