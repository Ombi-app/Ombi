using System.Collections.Generic;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Core.Settings.Models.External
{
    public sealed class JellyfinSettings : Ombi.Settings.Settings.Models.Settings
    {
        public bool Enable { get; set; }
        public List<JellyfinServers> Servers { get; set; } = new List<JellyfinServers>();
    }

    public class JellyfinServers : ExternalSettings
    {
        public string ServerId { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string AdministratorId { get; set; }
        public string ServerHostname { get; set; }
        public bool EnableEpisodeSearching { get; set; }
        public List<JellyfinSelectedLibraries> JellyfinSelectedLibraries { get; set; } = new List<JellyfinSelectedLibraries>();
    }

    public class JellyfinSelectedLibraries
    {
        public string Key { get; set; }
        public string Title { get; set; } // Name is for display purposes
        public string CollectionType { get; set; }
        public bool Enabled { get; set; }
    }
}
