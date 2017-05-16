namespace Ombi.Services.Interfaces
{
    public interface IWatcherCacher
    {
        void Queued();
        string[] QueuedIds();
    }
}
