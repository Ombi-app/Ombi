namespace PlexRequests.Services.Interfaces
{
    public interface ISickRageCacher
    {
        void Queued();
        int[] QueuedIds();
    }
}
