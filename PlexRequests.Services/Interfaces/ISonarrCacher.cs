namespace PlexRequests.Services.Interfaces
{
    public interface ISonarrCacher
    {
        void Queued();
        int[] QueuedIds();
    }
}
