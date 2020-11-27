namespace Ombi.Settings.Settings.Models
{
    public class OmbiSettings : Models.Settings
    {
        public string BaseUrl { get; set; }
        public bool CollectAnalyticData { get; set; }
        public bool Set { get; set; }
        public bool Wizard { get; set; }
        public string ApiKey { get; set; }
        public bool IgnoreCertificateErrors { get; set; }
        public bool DoNotSendNotificationsForAutoApprove { get; set; }
        public bool HideRequestsUsers { get; set; }
        public bool DisableHealthChecks { get; set; }
        public string DefaultLanguageCode { get; set; } = "en";
        public bool AutoDeleteAvailableRequests { get; set; }
        public int AutoDeleteAfterDays { get; set; }
    }
}