namespace Ombi.Api.Lidarr.Models
{
    public class LidarrRootFolder
    {
        public string path { get; set; }
        public long freeSpace { get; set; }
        public object[] unmappedFolders { get; set; }
        public int id { get; set; }
    }

}