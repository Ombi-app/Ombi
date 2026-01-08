namespace Ombi.Api.External.ExternalApis.Sonarr.Models
{
    public class SonarrRootFolder
    {
        public int id { get; set; }
        public string path { get; set; }
        public long freespace { get; set; }
    }
}