namespace PlexRequests.Services.Interfaces
{
    public interface ISonarrCacher
    {
        void Queued(long check);
        int[] QueuedIds();
    }
}
