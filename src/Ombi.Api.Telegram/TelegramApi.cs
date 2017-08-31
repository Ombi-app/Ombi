using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Ombi.Api.Telegram
{
    public class TelegramApi
    {

        //https://core.telegram.org/bots/api
        //https://github.com/TelegramBots/telegram.bot

        public async Task Send()
        {
            var botClient = new TelegramBotClient("422833810:AAEztVaoaSIeoXI3l9-rECKlSKJZtpFuMAU");
            var me = await botClient.GetMeAsync();
            await botClient.SendTextMessageAsync(new ChatId("@Ombi"), "Test");
        }
    }
}
