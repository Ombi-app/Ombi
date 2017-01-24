using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IStoreCleanup
    {
        void Execute(IJobExecutionContext context);
        void Start();
    }
}