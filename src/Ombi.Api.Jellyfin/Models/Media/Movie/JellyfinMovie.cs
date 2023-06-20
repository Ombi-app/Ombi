using System;

namespace Ombi.Api.Jellyfin.Models.Movie
{
    public class JellyfinMovie
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public JellyfinProviderids ProviderIds { get; set; }
        public JellyfinMediastream[] MediaStreams { get; set; }
    }
}