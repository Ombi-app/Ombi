using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IStoreBackup
    {
        void Start();
        void Execute(IJobExecutionContext context);
    }
}