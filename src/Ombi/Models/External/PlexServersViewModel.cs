using Ombi.Api.Plex.Models.Server;

namespace Ombi.Models.External
{
    public class PlexServersViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public PlexServer Servers { get; set; }
    }
}
