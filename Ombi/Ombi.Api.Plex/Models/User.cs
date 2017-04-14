using System.Collections.Generic;

namespace Ombi.Api.Plex.Models
{
    public class User
    {
        public string email { get; set; }
        public string uuid { get; set; }
        public string joined_at { get; set; }
        public string username { get; set; }
        public string title { get; set; }
        public string authentication_token { get; set; }
        public Subscription subscription { get; set; }
        public Roles roles { get; set; }
        public List<string> entitlements { get; set; }
        public object confirmed_at { get; set; }
        public int forum_id { get; set; }
    }
}