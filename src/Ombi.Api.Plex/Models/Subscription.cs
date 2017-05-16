namespace Ombi.Api.Plex.Models
{
    public class Subscription
    {
        public bool active { get; set; }
        public string status { get; set; }
        public object plan { get; set; }
        public object features { get; set; }
    }
}