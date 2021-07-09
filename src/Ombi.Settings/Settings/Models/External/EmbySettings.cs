using System.Collections.Generic;
using Ombi.Settings.Settings.Models.External;

namespace Ombi.Core.Settings.Models.External
{
    public sealed class EmbySettings : Ombi.Settings.Settings.Models.Settings
    {
        public bool Enable { get; set; }
        public List<EmbyServers> Servers { get; set; } = new List<EmbyServers>();
    }

    public class EmbyServers : ExternalSettings
    {
        public string ServerId { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string AdministratorId { get; set; }
        public string ServerHostname { get; set; }
        public bool EnableEpisodeSearching { get; set; }
        public List<EmbySelectedLibraries> EmbySelectedLibraries { get; set; } = new List<EmbySelectedLibraries>();
    }

    public class EmbySelectedLibraries
    {
        public int Key { get; set; }
        public string Title { get; set; } // Name is for display purposes
        public bool Enabled { get; set; }
    }
}
