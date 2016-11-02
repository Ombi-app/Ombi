namespace PlexRequests.Services.Interfaces
{
    public interface ICouchPotatoCacher
    {
        void Queued();
        int[] QueuedIds();
    }
}
