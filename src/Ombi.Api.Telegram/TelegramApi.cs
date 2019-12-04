using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Ombi.Api.Telegram
{
    public class TelegramApi : ITelegramApi
    {
        public TelegramApi(ITelegramRequestAnswer answer)
        {
            _answer = answer;
        }


        private readonly ITelegramRequestAnswer _answer;

        private static string _botApi;
        private static TelegramBotClient _client;


        public async Task Send(string message, string botApi, string chatId, string parseMode, IDictionary<string, string> data)
        {
            if (_client == null || _botApi != botApi)
            {
                UpdateClient(botApi);
            }

            string notificationType = data["NotificationType"];
            if (notificationType != "NewRequest")
            {
                await SendSimpleMessage(message, chatId, parseMode);
            }
            else
            {
                await SendQueryMessage(message, chatId, parseMode, data);
            }
        }


        private void UpdateClient(string botApi)
        {
            if (_client != null)
            {
                _client.StopReceiving();
            }

            _botApi = botApi;
            _client = new TelegramBotClient(_botApi);
            _client.OnCallbackQuery += async (sender, args) =>
            {
                await _client.EditMessageCaptionAsync(
                    chatId: args.CallbackQuery.Message.Chat.Id,
                    messageId: args.CallbackQuery.Message.MessageId,
                    caption: args.CallbackQuery.Message.Caption
                );

                var parameters = args.CallbackQuery.Data.Split(' ');
                var answer = parameters[0];
                var id = int.Parse(parameters[1]);

                var result =
                    answer == "approve_Movie" ? await _answer.ApproveMovie(id) :
                    answer == "deny_Movie" ? await _answer.DenyMovie(id) :
                    answer == "approve_TvShow" ? await _answer.ApproveTv(id) :
                    answer == "deny_TvShow" ? await _answer.DenyTv(id) :
                    "Incorrect answer";

                if (result != null)
                {
                    await SendSimpleMessage(result, args.CallbackQuery.Message.Chat.Id.ToString());
                }
            };

            _client.StartReceiving();
        }

        private async Task SendSimpleMessage(string message, string chatId, string parseMode = "default")
        {
            await _client.SendTextMessageAsync(
                chatId: chatId,
                text: message,
                parseMode: (ParseMode)Enum.Parse(typeof(ParseMode), parseMode, true)
            );
        }

        private async Task SendQueryMessage(string message, string chatId, string parseMode, IDictionary<string, string> data)
        {
            string image = data["PosterImage"];
            string type = data["Type"];
            string id = data["ItemId"];

            await _client.SendPhotoAsync(
                chatId: chatId,
                photo: new InputOnlineFile(image),
                caption: message,
                parseMode: (ParseMode)Enum.Parse(typeof(ParseMode), parseMode, true),
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                      InlineKeyboardButton.WithCallbackData("Approve", $"approve_{type} {id}"),
                      InlineKeyboardButton.WithCallbackData("Deny", $"deny_{type} {id}"),
                })
            );
        }
    }
}
