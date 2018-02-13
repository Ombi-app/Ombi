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
    }
}