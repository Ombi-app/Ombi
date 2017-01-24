using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IPlexContentCacher
    {
        void CacheContent();
        void Execute(IJobExecutionContext context);
    }
}