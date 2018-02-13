namespace Ombi.Settings.Settings.Models.Notifications
{
    public class TelegramSettings : Settings
    {
        public bool Enabled { get; set; }
        public string BotApi { get; set; }
        public string ChatId { get; set; }
        public string ParseMode { get; set; }
    }
}