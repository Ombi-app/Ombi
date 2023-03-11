using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Engine;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Test.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class MovieRequestEngineTests
    {
        private MovieRequestEngine _subject;
        private Mock<IMovieRequestRepository> _repoMock;
        private AutoMocker _mocker;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            var userManager = MockHelper.MockUserManager(new List<OmbiUser> { new OmbiUser { NormalizedUserName = "TEST", Id = "a" } });
            userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(true);
            var principle = new Mock<IPrincipal>();
            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.Name).Returns("Test");
            principle.Setup(x => x.Identity).Returns(identity.Object);
            var currentUser = new Mock<ICurrentUser>();
            currentUser.Setup(x => x.Identity).Returns(identity.Object);
            currentUser.Setup(x => x.Username).Returns("Test");
            currentUser.Setup(x => x.GetUser()).ReturnsAsync(new OmbiUser { NormalizedUserName = "TEST", Id = "a" });

            _repoMock = new Mock<IMovieRequestRepository>();
            var requestServiceMock = new Mock<IRequestServiceMain>();
            requestServiceMock.Setup(x => x.MovieRequestService).Returns(_repoMock.Object);

            _mocker.Use(principle.Object);
            _mocker.Use(currentUser.Object);
            _mocker.Use(userManager.Object);
            _mocker.Use(requestServiceMock);

            _subject = _mocker.CreateInstance<MovieRequestEngine>();
            var list = DbHelper.GetQueryableMockDbSet(new RequestSubscription());
            _mocker.Setup<IRepository<RequestSubscription>, IQueryable<RequestSubscription>>(x => x.GetAll()).Returns(new List<RequestSubscription>().AsQueryable().BuildMock());
            _mocker.Setup<IUserPlayedMovieRepository, IQueryable<UserPlayedMovie>>(x => x.GetAll()).Returns(new List<UserPlayedMovie>().AsQueryable().BuildMock());
        }

        [Test]
        public async Task GetRequestByStatus_PendingRequests_Non4K()
        {
            var movies = RegularRequestData();
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", Models.Requests.RequestStatus.PendingApproval);

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.First().Id, Is.EqualTo(4));
        }

        [Test]
        public async Task GetRequestByStatus_PendingRequests_4K()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Approved4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved4K = false,
                    Available4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 4,
                    Has4KRequest = true,
                    Approved4K = false,
                    Available4K = false,
                    Denied4K = false,
                    RequestedDate = DateTime.MinValue
                }
            };
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", RequestStatus.PendingApproval);

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.First().Id, Is.EqualTo(4));
        }


        [Test]
        public async Task GetRequestByStatus_PendingRequests_Both4K_And_Regular()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Approved = false,
                    Approved4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.Now
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved4K = false,
                    Available4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 4,
                    Has4KRequest = true,
                    Approved4K = false,
                    Available4K = false,
                    Denied4K = false,
                    RequestedDate = DateTime.MinValue
                }
            };
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", RequestStatus.PendingApproval);

            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Collection.First().Id, Is.EqualTo(1));
            Assert.That(result.Collection.ToArray()[1].Id, Is.EqualTo(4));
        }


        [Test]
        public async Task GetRequestByStatus_ProcessingRequests_Non4K()
        {
            var movies = RegularRequestData();
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", Models.Requests.RequestStatus.ProcessingRequest);

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.First().Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetRequestByStatus_ProcessingRequests_4K()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Approved4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved4K = false,
                    Available4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 4,
                    Has4KRequest = true,
                    Approved4K = false,
                    Available4K = false,
                    Denied4K = false,
                    RequestedDate = DateTime.MinValue
                }
            };
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", RequestStatus.ProcessingRequest);

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.First().Id, Is.EqualTo(1));
        }


        [Test]
        public async Task GetRequestByStatus_ProcessingRequests_Both4K_And_Regular()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Approved = false,
                    Approved4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.Now
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved4K = false,
                    Available4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 4,
                    Has4KRequest = true,
                    Approved4K = false,
                    Approved = true,
                    Available4K = false,
                    Denied4K = false,
                    RequestedDate = DateTime.Now
                }
            };
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", RequestStatus.ProcessingRequest);

            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Collection.First().Id, Is.EqualTo(1));
            Assert.That(result.Collection.ToArray()[1].Id, Is.EqualTo(4));
        }


        [Test]
        public async Task GetRequestByStatus_AvailableRequests_Non4K()
        {
            List<MovieRequests> movies = RegularRequestData();
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", Models.Requests.RequestStatus.Available);

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.First().Id, Is.EqualTo(2));
        }



        [Test]
        public async Task GetRequestByStatus_AvailableRequests_4K()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Approved4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved4K = false,
                    Available4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 4,
                    Has4KRequest = true,
                    Approved4K = false,
                    Available4K = false,
                    Denied4K = false,
                    RequestedDate = DateTime.MinValue
                }
            };
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", RequestStatus.Available);

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.First().Id, Is.EqualTo(2));
        }


        [Test]
        public async Task GetRequestByStatus_AvailableRequests_Both4K_And_Regular()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Available = true,
                    Approved = false,
                    Approved4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.Now
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved4K = false,
                    Available4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 4,
                    Has4KRequest = true,
                    Approved4K = false,
                    Approved = true,
                    Available4K = false,
                    Denied4K = false,
                    RequestedDate = DateTime.Now
                }
            };
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", RequestStatus.Available);

            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Collection.First().Id, Is.EqualTo(1));
            Assert.That(result.Collection.ToArray()[1].Id, Is.EqualTo(2));
        }

        [Test]
        public async Task GetRequestByStatus_DeniedRequests_Non4K()
        {
            List<MovieRequests> movies = RegularRequestData();
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", Models.Requests.RequestStatus.Denied);

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.First().Id, Is.EqualTo(3));
        }

        [Test]
        public async Task GetRequestByStatus_DeniedRequests_4K()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Approved4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved4K = false,
                    Available4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 4,
                    Has4KRequest = true,
                    Approved4K = false,
                    Available4K = false,
                    Denied4K = false,
                    RequestedDate = DateTime.MinValue
                }
            };
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", RequestStatus.Denied);

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.First().Id, Is.EqualTo(3));
        }


        [Test]
        public async Task GetRequestByStatus_DeniedRequests_Both4K_And_Regular()
        {
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Available = true,
                    Approved = false,
                    Approved4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.Now
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved4K = false,
                    Available4K = true,
                    Denied = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied4K = true,
                    Has4KRequest = true,
                    RequestedDate = DateTime.MinValue
                },
                new MovieRequests
                {
                    Id = 4,
                    Has4KRequest = true,
                    Approved4K = false,
                    Approved = true,
                    Available4K = false,
                    Denied4K = false,
                    RequestedDate = DateTime.Now
                }
            };
            _repoMock.Setup(x => x.GetWithUser()).Returns(movies.AsQueryable());

            var result = await _subject.GetRequestsByStatus(10, 0, "id", "asc", RequestStatus.Denied);

            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Collection.First().Id, Is.EqualTo(2));
            Assert.That(result.Collection.ToArray()[1].Id, Is.EqualTo(3));
        }

        private static List<MovieRequests> RegularRequestData()
        {
            return new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id= 1,
                    Approved = true,
                    RequestedDate = DateTime.Now
                },
                new MovieRequests
                {
                    Id = 2,
                    Approved = false,
                    Available = true,
                    RequestedDate = DateTime.Now
                },
                new MovieRequests
                {
                    Id = 3,
                    Denied = true,
                    RequestedDate = DateTime.Now
                },
                new MovieRequests
                {
                    Id = 4,
                    Approved = false,
                    RequestedDate = DateTime.Now
                }
            };
        }
    }
}