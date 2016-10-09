using Quartz;

namespace PlexRequests.Services.Jobs
{
    public interface IRecentlyAdded
    {
        void Execute(IJobExecutionContext context);
        void Test();
    }
}