using System.Collections.Generic;

namespace Ombi.Settings.Settings.Models
{
    public class AuthenticationSettings
    {
        /// <summary>
        /// This determins if Plex and/or Emby users can log into Ombi
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow external users to authenticate]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowExternalUsersToAuthenticate { get; set; }
        
        // Password Options
        public bool RequireDigit { get; set; }
        public int RequiredLength { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireNonAlphanumeric { get; set; }
        public bool RequireUppercase { get; set; }
    }
}