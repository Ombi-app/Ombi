namespace Ombi.Core.Settings.Models.External
{
    public sealed class PlexSettings : ExternalSettings
    {

        public bool Enable { get; set; }
        public bool EnableEpisodeSearching { get; set; }

        public string PlexAuthToken { get; set; }
        public string MachineIdentifier { get; set; }
    }
}