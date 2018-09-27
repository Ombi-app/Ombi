using Ombi.Core.Models.Requests;

namespace Ombi.Core.Models.UI
{
    public class OrderFilterModel
    {
        public FilterType AvailabilityFilter { get; set; }
        public FilterType StatusFilter { get; set; }
        public OrderType OrderType { get; set; }
    }
}