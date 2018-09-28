namespace Ombi.Core.Models.Requests
{
    public class FilterViewModel
    {
        public FilterType AvailabilityFilter { get; set; }
        public FilterType StatusFilter { get; set; }
        public int Position { get; set; }
        public int Count { get; set; }
    }

    public enum FilterType
    {
        None = 0,
        Available = 1,
        NotAvailable = 2,
        Approved = 3,
        Processing = 4,
        PendingApproval = 5
    }
}