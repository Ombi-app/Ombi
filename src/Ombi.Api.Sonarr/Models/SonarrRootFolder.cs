namespace Ombi.Api.Sonarr.Models
{
    public class SonarrRootFolder
    {
        public int id { get; set; }
        public string path { get; set; }
        public long freespace { get; set; }
    }
}