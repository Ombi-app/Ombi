namespace Ombi.Schedule.Jobs
{
    public class AvailabilityModel
    {
        public int Id { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string RequestedUser { get; set; }
    }
}
