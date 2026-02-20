namespace Ombi.Api.External.MediaServers.Jellyfin.Models
{
    /// <summary>
    /// Represents the authentication result from Jellyfin API
    /// Matches the AuthenticationResult schema from Jellyfin OpenAPI spec
    /// </summary>
    public class JellyfinAuthenticationResult
    {
        public JellyfinUser User { get; set; }
        public string AccessToken { get; set; }
        public string ServerId { get; set; }
    }
}
