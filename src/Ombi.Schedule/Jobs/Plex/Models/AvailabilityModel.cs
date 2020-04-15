using Ombi.Store.Entities;

namespace Ombi.Schedule.Jobs.Plex.Models
{
    public class AvailabilityModel
    {
        public int Id { get; set; }
        public string RequestedUser { get; set; }
    }
}
