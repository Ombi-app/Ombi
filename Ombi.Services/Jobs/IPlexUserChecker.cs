using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IPlexUserChecker
    {
        void Execute(IJobExecutionContext context);
        void Start();
    }
}