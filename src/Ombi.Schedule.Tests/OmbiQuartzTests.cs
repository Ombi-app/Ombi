using Moq;
using NUnit.Framework;
using Quartz;
using System.Threading;
using System.Threading.Tasks;
using Ombi.Helpers;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class OmbiQuartzTests
    {

        [Test]
        [Ignore("Cannot get this to work")]
        public void Test()
        {
            var scheduleMock = new Mock<IScheduler>();
            scheduleMock.Setup(x => x.TriggerJob(It.IsAny<JobKey>(),
                It.IsAny<CancellationToken>()));
            var sut = new QuartzMock(scheduleMock);

            //await QuartzMock.TriggerJob("ABC");

            scheduleMock.Verify(x => x.TriggerJob(It.Is<JobKey>(j => j.Name == "ABC"), 
                default(CancellationToken)), Times.Once);

        }
    }
    public class QuartzMock : OmbiQuartz
    {
        public QuartzMock(Mock<IScheduler> mock)
        {
            _instance = this;
            _scheduler = mock.Object;
        }
    }
}
