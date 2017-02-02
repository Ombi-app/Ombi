using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IRecentlyAdded
    {
        void Execute(IJobExecutionContext context);
        void RecentlyAddedAdminTest();
        void Start();
    }
}