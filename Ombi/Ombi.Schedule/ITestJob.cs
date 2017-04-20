using Hangfire.RecurringJobExtensions;
using Hangfire.Server;

namespace Ombi.Schedule
{
    public interface ITestJob
    {
        [RecurringJob("*/1 * * * *")]
        void Test(PerformContext context);
    }
}