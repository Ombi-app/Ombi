using System;

namespace Ombi.Api.Emby.Models.Movie
{
    public class EmbyMovie
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public EmbyProviderids ProviderIds { get; set; }
        public EmbyMediastream[] MediaStreams { get; set; }
    }
}