namespace Ombi.Core.Settings.Models.External
{
    public sealed class EmbySettings : ExternalSettings
    {
        public bool Enable { get; set; }
        public string ApiKey { get; set; }
        public string AdministratorId { get; set; }
        public bool EnableEpisodeSearching { get; set; }
    }
}