using Ombi.Api.Plex.Models.OAuth;

namespace Ombi.Models
{
    public class PlexOAuthViewModel
    {
        public bool Wizard { get; set; }
        public OAuthPin Pin { get; set; }
    }
}