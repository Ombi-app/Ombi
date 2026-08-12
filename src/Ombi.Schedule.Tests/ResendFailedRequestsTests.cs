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
        public async Task Execute_DeletesRequest_WhenErrorContainsTvdbIdMissing_ForTvShow()
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
        public async Task Execute_DoesNotDelete_WhenErrorContainsTvdbIdMissing_ForMovie()
        {
            var requestQueueItem = new RequestQueue
            {
                Id = 20,
                RequestId = 201,
                Type = RequestType.Movie,
                RetryCount = 1,
                Error = "TVDBID is missing\n",
                Completed = null
            };

            var movieRequest = new MovieRequests
            {
                Id = 201,
                Title = "Test Movie"
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(new List<RequestQueue> { requestQueueItem }.AsQueryable().BuildMock());
            MovieRepo.Setup(x => x.GetAll()).Returns(new List<MovieRequests> { movieRequest }.AsQueryable().BuildMock());
            MovieSender.Setup(x => x.Send(movieRequest, false)).ReturnsAsync(new SenderResult { Success = true });

            await Job.Execute(null);

            QueueRepo.Verify(x => x.Delete(It.IsAny<RequestQueue>()), Times.Never);
            Assert.That(requestQueueItem.Completed, Is.Not.Null);
        }

        [Test]
        public async Task Execute_IncrementsRetryCount_WhenTvRetryFails()
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
        public async Task Execute_IncrementsRetryCount_WhenMovieRetryFails()
        {
            var requestQueueItem = new RequestQueue
            {
                Id = 5,
                RequestId = 104,
                Type = RequestType.Movie,
                RetryCount = 2,
                Completed = null
            };

            var movieRequest = new MovieRequests
            {
                Id = 104,
                Title = "Test Movie"
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(new List<RequestQueue> { requestQueueItem }.AsQueryable().BuildMock());
            MovieRepo.Setup(x => x.GetAll()).Returns(new List<MovieRequests> { movieRequest }.AsQueryable().BuildMock());

            MovieSender.Setup(x => x.Send(movieRequest, false)).ReturnsAsync(new SenderResult
            {
                Success = false,
                Message = "Radarr offline"
            });

            await Job.Execute(null);

            Assert.That(requestQueueItem.RetryCount, Is.EqualTo(3));
            Assert.That(requestQueueItem.Error, Is.EqualTo("Radarr offline"));
            Assert.That(requestQueueItem.Completed, Is.Null);
            QueueRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Execute_IncrementsRetryCount_WhenAlbumRetryFails()
        {
            var requestQueueItem = new RequestQueue
            {
                Id = 6,
                RequestId = 105,
                Type = RequestType.Album,
                RetryCount = 2,
                Completed = null
            };

            var albumRequest = new AlbumRequest
            {
                Id = 105,
                Title = "Test Album"
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(new List<RequestQueue> { requestQueueItem }.AsQueryable().BuildMock());
            MusicRepo.Setup(x => x.GetAll()).Returns(new List<AlbumRequest> { albumRequest }.AsQueryable().BuildMock());

            MusicSender.Setup(x => x.Send(albumRequest)).ReturnsAsync(new SenderResult
            {
                Success = false,
                Message = "Lidarr offline"
            });

            await Job.Execute(null);

            Assert.That(requestQueueItem.RetryCount, Is.EqualTo(3));
            Assert.That(requestQueueItem.Error, Is.EqualTo("Lidarr offline"));
            Assert.That(requestQueueItem.Completed, Is.Null);
            QueueRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Execute_MarksCompleted_WhenMovieRetrySucceeds()
        {
            var requestQueueItem = new RequestQueue
            {
                Id = 7,
                RequestId = 106,
                Type = RequestType.Movie,
                RetryCount = 1,
                Completed = null
            };

            var movieRequest = new MovieRequests
            {
                Id = 106,
                Title = "Success Movie"
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(new List<RequestQueue> { requestQueueItem }.AsQueryable().BuildMock());
            MovieRepo.Setup(x => x.GetAll()).Returns(new List<MovieRequests> { movieRequest }.AsQueryable().BuildMock());

            MovieSender.Setup(x => x.Send(movieRequest, false)).ReturnsAsync(new SenderResult
            {
                Success = true
            });

            await Job.Execute(null);

            Assert.That(requestQueueItem.Completed, Is.Not.Null);
            QueueRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task Execute_MarksCompleted_WhenAlbumRetrySucceeds()
        {
            var requestQueueItem = new RequestQueue
            {
                Id = 8,
                RequestId = 107,
                Type = RequestType.Album,
                RetryCount = 1,
                Completed = null
            };

            var albumRequest = new AlbumRequest
            {
                Id = 107,
                Title = "Success Album"
            };

            QueueRepo.Setup(x => x.GetAll()).Returns(new List<RequestQueue> { requestQueueItem }.AsQueryable().BuildMock());
            MusicRepo.Setup(x => x.GetAll()).Returns(new List<AlbumRequest> { albumRequest }.AsQueryable().BuildMock());

            MusicSender.Setup(x => x.Send(albumRequest)).ReturnsAsync(new SenderResult
            {
                Success = true
            });

            await Job.Execute(null);

            Assert.That(requestQueueItem.Completed, Is.Not.Null);
            QueueRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
