using System.Collections.Generic;

namespace Ombi.Settings.Settings.Models
{
    public class AuthenticationSettings
    {
        public bool AllowNoPassword { get; set; }
        
        // Password Options
        public bool RequireDigit { get; set; }
        public int RequiredLength { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public bool RequireUppercase { get; set; }
    }
}