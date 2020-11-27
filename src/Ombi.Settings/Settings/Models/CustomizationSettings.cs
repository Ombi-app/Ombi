namespace Ombi.Settings.Settings.Models
{
    public class CustomizationSettings : Settings
    {
        public string ApplicationName { get; set; }
        public string ApplicationUrl { get; set; }
        public string CustomCss { get; set; }
        public bool EnableCustomDonations { get; set; }
        public string CustomDonationUrl { get; set; }
        public string CustomDonationMessage { get; set; }
        public string Logo { get; set; }
        public bool RecentlyAddedPage { get; set; }
        public bool UseCustomPage { get; set; }
        public bool HideAvailableFromDiscover { get; set; }

        public string AddToUrl(string part)
        {
            var appUrl = ApplicationUrl;
            if (string.IsNullOrEmpty(appUrl))
            {
                return null;
            }

            if (appUrl.EndsWith("/"))
            {
                appUrl = appUrl.Remove(ApplicationUrl.Length - 1);
            }
            if (!part.StartsWith("/"))
            {
                part = "/" + part;
            }
            appUrl = appUrl + part;
            return appUrl;
        }
    }
}