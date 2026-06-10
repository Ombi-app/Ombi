using Moq;
using NUnit.Framework;
using Quartz;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Helpers;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class OmbiQuartzTests
    {
        private Mock<IScheduler> _scheduler;

        [SetUp]
        public void Setup()
        {
            _scheduler = new Mock<IScheduler>();
            _ = new QuartzMock(_scheduler);
        }

        [Test]
        public async Task TriggerJobIfNotRunning_TriggersWhenJobIsNotRunning()
        {
            SetupRunningJobs(_scheduler);

            var triggered = await OmbiQuartz.TriggerJobIfNotRunning("MyJob", "Group");

            Assert.That(triggered, Is.True);
            _scheduler.Verify(x => x.TriggerJob(It.Is<JobKey>(j => j.Name == "MyJob" && j.Group == "Group"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task TriggerJobIfNotRunning_DoesNotTriggerWhenJobIsAlreadyRunning()
        {
            SetupRunningJobs(_scheduler, "MyJob");

            var triggered = await OmbiQuartz.TriggerJobIfNotRunning("MyJob", "Group");

            Assert.That(triggered, Is.False);
            _scheduler.Verify(x => x.TriggerJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()), Times.Never);
            _scheduler.Verify(x => x.TriggerJob(It.IsAny<JobKey>(), It.IsAny<JobDataMap>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task TriggerJobIfNotRunning_PassesTheDataMap()
        {
            SetupRunningJobs(_scheduler);

            var triggered = await OmbiQuartz.TriggerJobIfNotRunning("MyJob", "Group",
                new Dictionary<string, object> { { "RecentlyAddedSearch", "false" } });

            Assert.That(triggered, Is.True);
            _scheduler.Verify(x => x.TriggerJob(It.Is<JobKey>(j => j.Name == "MyJob" && j.Group == "Group"),
                It.Is<JobDataMap>(d => (string)d["RecentlyAddedSearch"] == "false"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task TriggerJob_DoesNotTriggerWhenJobIsAlreadyRunning()
        {
            SetupRunningJobs(_scheduler, "MyJob");

            await OmbiQuartz.TriggerJob("MyJob", "Group");

            _scheduler.Verify(x => x.TriggerJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private static void SetupRunningJobs(Mock<IScheduler> scheduler, params string[] runningJobNames)
        {
            var running = new List<IJobExecutionContext>();
            foreach (var name in runningJobNames)
            {
                var jobDetail = new Mock<IJobDetail>();
                jobDetail.Setup(x => x.Key).Returns(new JobKey(name, "Group"));
                var context = new Mock<IJobExecutionContext>();
                context.Setup(x => x.JobDetail).Returns(jobDetail.Object);
                running.Add(context.Object);
            }

            scheduler.Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(running);
        }
    }

    public class QuartzMock : OmbiQuartz
    {
        public QuartzMock(Mock<IScheduler> mock) : base(false)
        {
            _instance = this;
            _scheduler = mock.Object;
        }
    }
}
