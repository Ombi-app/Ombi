using Quartz;

namespace Ombi.Services.Jobs
{
    public interface IMassEmail
    {
        void Execute(IJobExecutionContext context);
        void MassEmailAdminTest(string html);
    }
}