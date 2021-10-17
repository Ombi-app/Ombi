using System.Collections.Generic;

namespace Ombi.Settings.Settings.Models
{
    public class CloudflareAuthenticationSettings : Settings
    {
        public string issuer { get; set; }
        public string audience { get; set; }
        public string certlink { get; set; }
    }
}