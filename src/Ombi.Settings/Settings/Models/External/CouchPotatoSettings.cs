using Ombi.Core.Settings.Models.External;

namespace Ombi.Settings.Settings.Models.External
{
    public class CouchPotatoSettings : ExternalSettings
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public string DefaultProfileId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}