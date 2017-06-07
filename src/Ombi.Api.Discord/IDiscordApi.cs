using System.Threading.Tasks;

namespace Ombi.Api.Discord
{
    public interface IDiscordApi
    {
        Task SendMessage(string message, string webhookId, string webhookToken, string username = null);
    }
}