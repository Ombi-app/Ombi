using Quartz;

namespace PlexRequests.Services.Jobs
{
    public interface IPlexContentCacher
    {
        void CacheContent();
        void Execute(IJobExecutionContext context);
    }
}