namespace Ombi.Models
{
    public class AboutViewModel
    {
        public string Version { get; set; }
        public string Branch { get; set; }
        public string FrameworkDescription { get; set; }
        public string OsArchitecture { get; set; }
        public string OsDescription { get; set; }
        public string ProcessArchitecture { get; set; }
        public string ApplicationBasePath { get; set; }
        public string OmbiDatabaseType { get; set; }
        public string ExternalDatabaseType { get; set; }
        public string SettingsDatabaseType { get; set; }
        public string OmbiConnectionString { get; set; }
        public string ExternalConnectionString { get; set; }
        public string SettingsConnectionString { get; set; }
        public string StoragePath { get; set; }
        public bool NotSupported { get; set; }
    }
}