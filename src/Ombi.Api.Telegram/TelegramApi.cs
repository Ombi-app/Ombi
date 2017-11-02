using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ombi.Api.Telegram
{
    public class TelegramApi : ITelegramApi
    {
        public TelegramApi(IApi api)
        {
            _api = api;
        }

        private readonly IApi _api;
        private const string BaseUrl = "https://api.telegram.org/bot";


        public async Task Send(string message, string botApi, string chatId, string parseMode)
        {
            var request = new Request($"{botApi}/sendMessage", BaseUrl, HttpMethod.Post);
            request.AddQueryString("chat_id", chatId);

            var body = new
            {
                text = message,
                parse_mode = parseMode
            };

            request.AddJsonBody(body);
            await _api.Request(request);
        }
    }
}
