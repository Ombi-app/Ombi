namespace Ombi.Api.GroupMe.Models
{

    public class Groups
    {
        public string id { get; set; }
        public string group_id { get; set; }
        public string name { get; set; }
        public string phone_number { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string image_url { get; set; }
        public string creator_user_id { get; set; }
        public int created_at { get; set; }
        public int updated_at { get; set; }
        public bool office_mode { get; set; }
        public string share_url { get; set; }
        public string share_qr_code_url { get; set; }
        public Messages messages { get; set; }
        public int max_members { get; set; }
    }

    public class Messages
    {
        public int count { get; set; }
        public string last_message_id { get; set; }
        public int last_message_created_at { get; set; }
        public Preview preview { get; set; }
    }

    public class Preview
    {
        public string nickname { get; set; }
        public string text { get; set; }
        public string image_url { get; set; }
        public object[] attachments { get; set; }
    }
}