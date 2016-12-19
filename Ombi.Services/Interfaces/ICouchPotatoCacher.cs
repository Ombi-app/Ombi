namespace Ombi.Services.Interfaces
{
    public interface ICouchPotatoCacher
    {
        void Queued();
        int[] QueuedIds();
    }
}
