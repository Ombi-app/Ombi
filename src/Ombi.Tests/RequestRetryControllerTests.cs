using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Controllers.V1;
using Ombi.Core;
using Ombi.Core.Senders;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Tests
{
    [TestFixture]
    public class RequestRetryControllerTests
    {
        [Test]
        public async Task GetFailedRequests_SkipsOrphanedMovieQueueEntry()
        {
            var queue = new List<RequestQueue>
            {
                new RequestQueue
                {
                    RequestId = 42,
                    Type = RequestType.Movie,
                    Dts = DateTime.UtcNow,
                    Error = "Original request was deleted",
                    RetryCount = 1
                }
            };

            var queueRepository = new Mock<IRepository<RequestQueue>>();
            queueRepository.Setup(x => x.GetAll()).Returns(queue.AsQueryable().BuildMock());

            var movieRepository = new Mock<IMovieRequestRepository>();
            movieRepository.Setup(x => x.Find(It.IsAny<object>())).ReturnsAsync((MovieRequests)null);

            var subject = CreateSubject(queueRepository, movieRepository: movieRepository);

            var result = (await subject.GetFailedRequests()).ToList();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task Retry_TvSuccess_CompletesQueueEntry()
        {
            var queueItem = new RequestQueue
            {
                Id = 7,
                RequestId = 101,
                Type = RequestType.TvShow,
                Error = "Old failure"
            };
            var request = new ChildRequests { Id = 101 };
            var queueRepository = QueueRepository(queueItem);
            var tvRepository = new Mock<ITvRequestRepository>();
            tvRepository.Setup(x => x.GetChild())
                .Returns(new List<ChildRequests> { request }.AsQueryable().BuildMock());
            var tvSender = new Mock<ITvSender>();
            tvSender.Setup(x => x.Send(request))
                .ReturnsAsync(new SenderResult { Success = true });

            var subject = CreateSubject(queueRepository, tvRepository: tvRepository, tvSender: tvSender);

            var result = await subject.Retry(queueItem.Id);

            Assert.That(result.Result, Is.True);
            Assert.That(result.RequestId, Is.EqualTo(request.Id));
            Assert.That(queueItem.Completed, Is.Not.Null);
            queueRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
            tvSender.Verify(x => x.Send(request), Times.Once);
        }

        [Test]
        public async Task Retry_TvFailure_LeavesQueueEntryActive()
        {
            var queueItem = new RequestQueue
            {
                Id = 8,
                RequestId = 102,
                Type = RequestType.TvShow,
                Error = "Old failure"
            };
            var request = new ChildRequests { Id = 102 };
            var queueRepository = QueueRepository(queueItem);
            var tvRepository = new Mock<ITvRequestRepository>();
            tvRepository.Setup(x => x.GetChild())
                .Returns(new List<ChildRequests> { request }.AsQueryable().BuildMock());
            var tvSender = new Mock<ITvSender>();
            tvSender.Setup(x => x.Send(request))
                .ReturnsAsync(new SenderResult { Success = false, Message = "Still failed" });

            var subject = CreateSubject(queueRepository, tvRepository: tvRepository, tvSender: tvSender);

            var result = await subject.Retry(queueItem.Id);

            Assert.That(result.Result, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Still failed"));
            Assert.That(queueItem.Completed, Is.Null);
            queueRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task Retry_Movie_UsesExistingApproved4KRetryBehavior()
        {
            var queueItem = new RequestQueue
            {
                Id = 9,
                RequestId = 103,
                Type = RequestType.Movie
            };
            var request = new MovieRequests { Id = 103, Approved4K = true };
            var queueRepository = QueueRepository(queueItem);
            var movieRepository = new Mock<IMovieRequestRepository>();
            movieRepository.Setup(x => x.GetWithUser())
                .Returns(new List<MovieRequests> { request }.AsQueryable().BuildMock());
            var movieSender = new Mock<IMovieSender>();
            movieSender.Setup(x => x.Send(request, true))
                .ReturnsAsync(new SenderResult { Success = true });

            var subject = CreateSubject(queueRepository, movieRepository: movieRepository, movieSender: movieSender);

            var result = await subject.Retry(queueItem.Id);

            Assert.That(result.Result, Is.True);
            Assert.That(queueItem.Completed, Is.Not.Null);
            movieSender.Verify(x => x.Send(request, true), Times.Once);
        }

        [Test]
        public async Task Retry_AlbumSuccess_CompletesQueueEntry()
        {
            var queueItem = new RequestQueue
            {
                Id = 10,
                RequestId = 104,
                Type = RequestType.Album
            };
            var request = new AlbumRequest { Id = 104 };
            var queueRepository = QueueRepository(queueItem);
            var musicRepository = new Mock<IMusicRequestRepository>();
            musicRepository.Setup(x => x.GetAll())
                .Returns(new List<AlbumRequest> { request }.AsQueryable().BuildMock());
            var musicSender = new Mock<IMusicSender>();
            musicSender.Setup(x => x.Send(request))
                .ReturnsAsync(new SenderResult { Success = true });

            var subject = CreateSubject(queueRepository, musicRepository: musicRepository, musicSender: musicSender);

            var result = await subject.Retry(queueItem.Id);

            Assert.That(result.Result, Is.True);
            Assert.That(queueItem.Completed, Is.Not.Null);
            musicSender.Verify(x => x.Send(request), Times.Once);
        }

        private static Mock<IRepository<RequestQueue>> QueueRepository(params RequestQueue[] queueItems)
        {
            var repository = new Mock<IRepository<RequestQueue>>();
            repository.Setup(x => x.GetAll()).Returns(queueItems.AsQueryable().BuildMock());
            repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
            return repository;
        }

        private static RequestRetryController CreateSubject(
            Mock<IRepository<RequestQueue>> queueRepository,
            Mock<IMovieRequestRepository> movieRepository = null,
            Mock<ITvRequestRepository> tvRepository = null,
            Mock<IMusicRequestRepository> musicRepository = null,
            Mock<IMovieSender> movieSender = null,
            Mock<ITvSender> tvSender = null,
            Mock<IMusicSender> musicSender = null)
        {
            return new RequestRetryController(
                queueRepository.Object,
                (movieRepository ?? new Mock<IMovieRequestRepository>()).Object,
                (tvRepository ?? new Mock<ITvRequestRepository>()).Object,
                (musicRepository ?? new Mock<IMusicRequestRepository>()).Object,
                (movieSender ?? new Mock<IMovieSender>()).Object,
                (tvSender ?? new Mock<ITvSender>()).Object,
                (musicSender ?? new Mock<IMusicSender>()).Object);
        }
    }
}
