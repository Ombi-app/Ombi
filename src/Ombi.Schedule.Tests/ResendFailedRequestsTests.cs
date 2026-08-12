using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Core;
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
        [SetUp]
        public void Setup()
        {
            QueueRepo = new Mock<IRepository<RequestQueue>>();
            MovieSender = new Mock<IMovieSender>();
            TvSender = new Mock<ITvSender>();
            MusicSender = new Mock<IMusicSender>();
            MovieRepo = new Mock<IMovieRequestRepository>();
            TvRepo = new Mock<ITvRequestRepository>();
            MusicRepo = new Mock<IMusicRequestRepository>();

            Job = new ResendFailedRequests(
                QueueRepo.Object,
                MovieSender.Object,
                TvSender.Object,
                MusicSender.Object,
                MovieRepo.Object,
                TvRepo.Object,
                MusicRepo.Object);
        }

        public Mock<IRepository<RequestQueue>> QueueRepo { get; set; }
        public Mock<IMovieSender> MovieSender { get; set; }
        public Mock<ITvSender> TvSender { get; set; }
        public Mock<IMusicSender> MusicSender { get; set; }
        public Mock<IMovieRequestRepository> MovieRepo { get; set; }
        public Mock<ITvRequestRepository> TvRepo { get; set; }
        public Mock<IMusicRequestRepository> MusicRepo { get; set; }
        public ResendFailedRequests Job { get; set; }

        [Test]
        public async Task Execute_DeletesRequest_WhenRetryCountExceedsMaxLimit()
        {
            var failedRequests = new List<RequestQueue>
            {
                new RequestQueue
                {
                    Id = 1,
                    RequestId = 100,
                    Type = RequestType.TvShow,
                    RetryCount = 10,
                    Completed = null
                }
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(failedRequests.AsQueryable().BuildMock());

            await Job.Execute(null);

            QueueRepo.Verify(x => x.Delete(It.Is<RequestQueue>(r => r.Id == 1)), Times.Once);
            QueueRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
            TvSender.Verify(x => x.Send(It.IsAny<ChildRequests>()), Times.Never);
        }

        [Test]
        public async Task Execute_DeletesRequest_WhenErrorContainsTvdbIdMissing()
        {
            var failedRequests = new List<RequestQueue>
            {
                new RequestQueue
                {
                    Id = 2,
                    RequestId = 101,
                    Type = RequestType.TvShow,
                    RetryCount = 1,
                    Error = "TVDBID is missing\n",
                    Completed = null
                }
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(failedRequests.AsQueryable().BuildMock());

            await Job.Execute(null);

            QueueRepo.Verify(x => x.Delete(It.Is<RequestQueue>(r => r.Id == 2)), Times.Once);
            QueueRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
            TvSender.Verify(x => x.Send(It.IsAny<ChildRequests>()), Times.Never);
        }

        [Test]
        public async Task Execute_IncrementsRetryCount_WhenRetryFails()
        {
            var requestQueueItem = new RequestQueue
            {
                Id = 3,
                RequestId = 102,
                Type = RequestType.TvShow,
                RetryCount = 2,
                Completed = null
            };

            var tvRequest = new ChildRequests
            {
                Id = 102,
                Title = "Test Show"
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(new List<RequestQueue> { requestQueueItem }.AsQueryable().BuildMock());
            TvRepo.Setup(x => x.GetChild()).Returns(new List<ChildRequests> { tvRequest }.AsQueryable().BuildMock());

            TvSender.Setup(x => x.Send(tvRequest)).ReturnsAsync(new SenderResult
            {
                Success = false,
                Message = "Connection refused"
            });

            await Job.Execute(null);

            Assert.That(requestQueueItem.RetryCount, Is.EqualTo(3));
            Assert.That(requestQueueItem.Error, Is.EqualTo("Connection refused"));
            Assert.That(requestQueueItem.Completed, Is.Null);
            QueueRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Execute_MarksCompleted_WhenRetrySucceeds()
        {
            var requestQueueItem = new RequestQueue
            {
                Id = 4,
                RequestId = 103,
                Type = RequestType.TvShow,
                RetryCount = 1,
                Completed = null
            };

            var tvRequest = new ChildRequests
            {
                Id = 103,
                Title = "Success Show"
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(new List<RequestQueue> { requestQueueItem }.AsQueryable().BuildMock());
            TvRepo.Setup(x => x.GetChild()).Returns(new List<ChildRequests> { tvRequest }.AsQueryable().BuildMock());

            TvSender.Setup(x => x.Send(tvRequest)).ReturnsAsync(new SenderResult
            {
                Success = true
            });

            await Job.Execute(null);

            Assert.That(requestQueueItem.Completed, Is.Not.Null);
            QueueRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
