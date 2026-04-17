namespace Ombi.Core.Models
{
    public class PlexUserWatchlistModel
    {
        public string UserId { get; set; }
        public WatchlistSyncStatus SyncStatus { get; set; }
        public string UserName { get; set; }
    }

    public enum WatchlistSyncStatus
    {
        Successful = 0,
        Failed = 1,
        NotAFriend = 2,
    }
}
