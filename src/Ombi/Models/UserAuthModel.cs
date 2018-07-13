using Ombi.Api.Plex.Models.OAuth;

namespace Ombi.Models
{
    public class UserAuthModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public bool UsePlexAdminAccount { get; set; }
        public bool UsePlexOAuth { get; set; }
        public OAuthPin PlexTvPin { get; set; }
    }
}