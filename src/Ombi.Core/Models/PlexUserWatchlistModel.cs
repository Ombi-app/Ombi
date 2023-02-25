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
        Successful,
        Failed,
        NotEnabled
    }
}
