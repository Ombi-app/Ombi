using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Senders;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class ResendFailedRequestsTests
    {
        private AutoMocker _mocker;
        private ResendFailedRequests _subject;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<ResendFailedRequests>();
        }

        [Test]
        public async Task ManualInterventionTvFailure_RemainsVisibleWithoutAutomaticRetry()
        {
            var request = new ChildRequests
            {
                Id = 42,
                ParentRequest = new TvRequests { TvDbId = 123, Title = "Test Show" }
            };
            var queueItem = new RequestQueue
            {
                RequestId = request.Id,
                Type = RequestType.TvShow,
                Error = TvSender.ManualInterventionQueuePrefix + "Unsafe season mapping",
                RetryCount = 1
            };

            _mocker.GetMock<IRepository<RequestQueue>>()
                .Setup(x => x.GetAll())
                .Returns(new List<RequestQueue> { queueItem }.AsQueryable().BuildMock());
            _mocker.GetMock<ITvRequestRepository>()
                .Setup(x => x.GetChild())
                .Returns(new List<ChildRequests> { request }.AsQueryable().BuildMock());

            await _subject.Execute(null);

            _mocker.GetMock<ITvSender>()
                .Verify(x => x.Send(It.IsAny<ChildRequests>()), Times.Never);
            Assert.That(queueItem.Completed, Is.Null);
            Assert.That(queueItem.RetryCount, Is.EqualTo(1));
        }

        [Test]
        public async Task LegacyUnclassifiedTvFailure_GetsAnotherAutomaticRetry()
        {
            var request = new ChildRequests
            {
                Id = 42,
                ParentRequest = new TvRequests { TvDbId = 123, Title = "Test Show" }
            };
            var queueItem = new RequestQueue
            {
                RequestId = request.Id,
                Type = RequestType.TvShow,
                Error = "Unable to safely map requested season 1",
                RetryCount = 1
            };

            _mocker.GetMock<IRepository<RequestQueue>>()
                .Setup(x => x.GetAll())
                .Returns(new List<RequestQueue> { queueItem }.AsQueryable().BuildMock());
            _mocker.GetMock<ITvRequestRepository>()
                .Setup(x => x.GetChild())
                .Returns(new List<ChildRequests> { request }.AsQueryable().BuildMock());
            _mocker.GetMock<ITvSender>()
                .Setup(x => x.Send(request))
                .ReturnsAsync(new SenderResult { Success = false });

            await _subject.Execute(null);

            _mocker.GetMock<ITvSender>()
                .Verify(x => x.Send(request), Times.Once);
            Assert.That(queueItem.Completed, Is.Null);
        }
    }
}
