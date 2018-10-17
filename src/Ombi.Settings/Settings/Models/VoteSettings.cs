namespace Ombi.Settings.Settings.Models
{
    public class VoteSettings : Settings
    {
        public bool Enabled { get; set; }
        public int MovieVoteMax { get; set; }
        public int MusicVoteMax { get; set; }
        public int TvShowVoteMax { get; set; }
    }
}