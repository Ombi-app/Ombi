namespace Ombi.Api.Notifications.Models
{
    public class OneSignalNotificationBody
    {
        public string app_id { get; set; }
        public string[] include_player_ids { get; set; }
        public Data data { get; set; }
        public Contents contents { get; set; }
    }

    public class Data
    {
        public string foo { get; set; }
    }

    public class Contents
    {
        public string en { get; set; }
    }

}