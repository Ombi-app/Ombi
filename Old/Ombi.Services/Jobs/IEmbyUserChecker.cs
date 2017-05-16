using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IEmbyUserChecker
    {
        void Execute(IJobExecutionContext context);
        void Start();
    }
}