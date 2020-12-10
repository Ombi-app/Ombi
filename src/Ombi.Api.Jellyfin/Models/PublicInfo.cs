namespace Ombi.Api.Jellyfin.Models
{
    public class PublicInfo
    {
        public string LocalAddress { get; set; }
        public string ServerName { get; set; }
        public string Version { get; set; }
        /// <summary>
        /// Only populated for Jellyfin
        /// </summary>
        public string ProductName { get; set; }

        public bool IsJellyfin => !string.IsNullOrEmpty(ProductName) && ProductName.Contains("Jellyfin");

        public string OperatingSystem { get; set; }
        public string Id { get; set; }
    }

}