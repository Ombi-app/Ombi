using System.Collections.Generic;

namespace Ombi.Core.Settings.Models.External
{
    public sealed class PlexSettings : ExternalSettings
    {

        public bool Enable { get; set; }
        public bool EnableEpisodeSearching { get; set; }

        public string PlexAuthToken { get; set; }
        public string MachineIdentifier { get; set; }

        public  List<PlexSelectedLibraries> PlexSelectedLibraries { get; set; }
    }

    public class PlexSelectedLibraries
    {
        public int Key { get; set; }
        public string Title { get; set; } // Name is for display purposes
        public bool Enabled { get; set; }
    }
}