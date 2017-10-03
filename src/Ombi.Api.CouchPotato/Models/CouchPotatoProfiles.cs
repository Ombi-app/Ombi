using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Api.CouchPotato.Models
{
    public class ProfileList
    {
        public bool core { get; set; }
        public bool hide { get; set; }
        public string _rev { get; set; }
        public List<bool> finish { get; set; }
        public List<string> qualities { get; set; }
        public string _id { get; set; }
        public string _t { get; set; }
        public string label { get; set; }
        public int minimum_score { get; set; }
        public List<int> stop_after { get; set; }
        public List<object> wait_for { get; set; }
        public int order { get; set; }
        [JsonProperty(PropertyName = "3d")]
        public List<object> threeD { get; set; }
    }

    public class CouchPotatoProfiles
    {
        public List<ProfileList> list { get; set; }
        public bool success { get; set; }
    }
}