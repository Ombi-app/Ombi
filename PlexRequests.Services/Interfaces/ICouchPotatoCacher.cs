namespace PlexRequests.Services.Interfaces
{
    public interface ICouchPotatoCacher
    {
        void Queued(long check);
        int[] QueuedIds();
    }
}
