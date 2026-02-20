namespace Ombi.Api.External.MediaServers.Emby.Models
{
    /// <summary>
    /// Represents the authentication result from Emby API
    /// Emby and Jellyfin share the same API structure
    /// </summary>
    public class EmbyAuthenticationResult
    {
        public EmbyUser User { get; set; }
        public object SessionInfo { get; set; }
        public string AccessToken { get; set; }
        public string ServerId { get; set; }
    }
}
