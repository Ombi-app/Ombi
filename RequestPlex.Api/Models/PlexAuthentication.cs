using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RequestPlex.Api.Models
{
    public class PlexAuthentication
    {
        public User user { get; set; }
    }
    public class Subscription
    {
        public bool active { get; set; }
        public string status { get; set; }
        public object plan { get; set; }
        public object features { get; set; }
    }

    public class Roles
    {
        public List<object> roles { get; set; }
    }

    public class User
    {
        public string email { get; set; }
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
