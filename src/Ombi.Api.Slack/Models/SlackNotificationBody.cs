using Newtonsoft.Json;

namespace Ombi.Api.Slack.Models
{
    public class SlackNotificationBody
    {
        [JsonConstructor]
        public SlackNotificationBody()
        {
            username = "Ombi";
        }

        [JsonIgnore]
        private string _username;
        public string username
        {
            get => _username;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _username = value;
            }
        }
        public string channel { get; set; }
        public string text { get; set; }

        public string icon_url { get; set; }
        public string icon_emoji { get; set; }
    }
}