namespace Ombi.Services.Interfaces
{
    public interface ISickRageCacher
    {
        void Queued();
        int[] QueuedIds();
    }
}
