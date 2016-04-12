namespace PlexRequests.Services.Interfaces
{
    public interface ISickRageCacher
    {
        void Queued(long check);
        int[] QueuedIds();
    }
}
