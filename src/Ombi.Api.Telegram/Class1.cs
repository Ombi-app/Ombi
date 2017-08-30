using System;

namespace Ombi.Api.Telegram
{
    public class TelegramApi
    {
        public TelegramApi(IApi api)
        {
            Api = api;
        }
        //https://core.telegram.org/bots/api
        //https://github.com/TelegramBots/telegram.bot
        private IApi Api { get; }
    }
}
