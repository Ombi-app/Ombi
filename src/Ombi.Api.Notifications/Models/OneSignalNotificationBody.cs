namespace Ombi.Api.Notifications.Models
{
    public class OneSignalNotificationBody
    {
        public string app_id { get; set; }
        public string[] include_player_ids { get; set; }
        public object data { get; set; }
        public Button[] buttons { get; set; }
        public Contents contents { get; set; }
    }


    public class Contents
    {
        public string en { get; set; }
    }

    public class Button
    {
        public string id { get; set; }
        public string text { get; set; }
        //public string icon { get; set; }
    }

}