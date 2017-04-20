using System;
using System.Diagnostics;
using Hangfire;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;

namespace Ombi.Schedule
{
    public class TestJob : ITestJob
    {
        [RecurringJob("*/1 * * * *")]
        public void Test(PerformContext context)
        {
            Debug.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} TestJob1 Running ...");
        }
    }
}
