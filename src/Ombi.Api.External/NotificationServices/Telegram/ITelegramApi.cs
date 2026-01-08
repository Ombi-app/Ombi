using System.Threading.Tasks;

namespace Ombi.Api.External.NotificationServices.Telegram
{
    public interface ITelegramApi
    {
        Task Send(string message, string botApi, string chatId, string parseMode);
    }
}