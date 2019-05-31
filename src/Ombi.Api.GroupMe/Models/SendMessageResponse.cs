namespace Ombi.Api.GroupMe.Models
{

    public class SendResponse
    {
        public Message message { get; set; }
    }

    public class Message
    {
        public string id { get; set; }
        public string source_guid { get; set; }
        public int created_at { get; set; }
        public string user_id { get; set; }
        public string group_id { get; set; }
        public string name { get; set; }
        public string avatar_url { get; set; }
        public string text { get; set; }
        public bool system { get; set; }
        public object[] attachments { get; set; }
        public object[] favorited_by { get; set; }
        public string sender_type { get; set; }
        public string sender_id { get; set; }
    }

}