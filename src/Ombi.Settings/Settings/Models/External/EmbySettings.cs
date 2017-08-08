using System.Collections.Generic;

namespace Ombi.Core.Settings.Models.External
{
    public sealed class EmbySettings : Ombi.Settings.Settings.Models.Settings
    {
        public bool Enable { get; set; }
        public List<EmbyServers> Servers { get; set; }
    }

    public class EmbyServers : ExternalSettings
    {
        public string ApiKey { get; set; }
        public string AdministratorId { get; set; }
        public bool EnableEpisodeSearching { get; set; }
    }
}