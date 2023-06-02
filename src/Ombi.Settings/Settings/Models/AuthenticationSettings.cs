using System.Collections.Generic;

namespace Ombi.Settings.Settings.Models
{
    public class AuthenticationSettings : Settings
    {
        public bool AllowNoPassword { get; set; }
        
        // Password Options
        public bool RequireDigit { get; set; }
        public int RequiredLength { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public bool RequireUppercase { get; set; }
        public bool EnableOAuth { get; set; } // Plex OAuth
        public bool EnableHeaderAuth { get; set; } // Header SSO
        public string HeaderAuthVariable { get; set; } // Header SSO
        public bool HeaderAuthCreateUser { get; set; } // Header SSO
    }
}