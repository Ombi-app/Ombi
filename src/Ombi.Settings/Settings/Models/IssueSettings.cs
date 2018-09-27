namespace Ombi.Settings.Settings.Models
{
    public class IssueSettings : Settings
    {
        public bool Enabled { get; set; }
        public bool EnableInProgress { get; set; }

        public bool DeleteIssues { get; set; }
        public int DaysAfterResolvedToDelete { get; set; }
    }
}