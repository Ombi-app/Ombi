namespace Ombi.Settings.Settings.Models
{
    public class CustomPageSettings : Settings
    {
        public bool Enabled { get; set; }
        public string Title { get; set; }
        public string Html { get; set; }
    }
}