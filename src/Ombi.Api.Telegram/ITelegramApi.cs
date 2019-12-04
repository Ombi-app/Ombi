using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Api.Telegram
{
    public interface ITelegramApi
    {
        Task Send(string message, string botApi, string chatId, string parseMode, IDictionary<string, string> data);
    }
}