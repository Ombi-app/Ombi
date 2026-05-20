using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class RequestDeletionNotificationTests
    {
        [Test]
        public async Task RemoveTvChild_DoesNotNotify_WhenUserCannotManageRequest()
        {
            var (subject, repo, mocker, userManager) = CreateTvEngine();
            userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(false);

            var child = new ChildRequests
            {
                Id = 1,
                RequestedUserId = "different-user",
                RequestedUser = new OmbiUser { Id = "different-user", Email = "user@test.local" },
                ParentRequest = new TvRequests { Title = "TV Title" }
            };

            repo.Setup(x => x.GetChild()).Returns(new List<ChildRequests> { child }.AsQueryable().BuildMock());
            mocker.GetMock<INotificationHelper>().Setup(x => x.Notify(It.IsAny<NotificationOptions>())).Returns(Task.CompletedTask);

            var result = await subject.RemoveTvChild(1);

            Assert.That(result.Result, Is.False);
            mocker.GetMock<INotificationHelper>().Verify(x => x.Notify(It.IsAny<NotificationOptions>()), Times.Never);
        }

        [Test]
        public async Task RemoveTvRequest_DoesNotNotify_WhenNoChildRequesters()
        {
            var (subject, repo, mocker, _) = CreateTvEngine();

            var request = new TvRequests
            {
                Id = 7,
                Title = "TV Title",
                ChildRequests = new List<ChildRequests>
                {
                    new ChildRequests { Id = 1, RequestedUserId = null },
                    new ChildRequests { Id = 2, RequestedUserId = string.Empty }
                }
            };

            repo.Setup(x => x.Get()).Returns(new List<TvRequests> { request }.AsQueryable().BuildMock());
            repo.Setup(x => x.Delete(request)).Returns(Task.CompletedTask);
            mocker.GetMock<IMediaCacheService>().Setup(x => x.Purge()).Returns(Task.CompletedTask);
            mocker.GetMock<INotificationHelper>().Setup(x => x.Notify(It.IsAny<NotificationOptions>())).Returns(Task.CompletedTask);

            await subject.RemoveTvRequest(7);

            mocker.GetMock<INotificationHelper>().Verify(x => x.Notify(It.IsAny<NotificationOptions>()), Times.Never);
            repo.Verify(x => x.Delete(request), Times.Once);
        }

        [Test]
        public async Task RemoveTvRequest_NotifiesEachRequesterOnce_WhenMultipleChildrenSameUser()
        {
            var (subject, repo, mocker, _) = CreateTvEngine();

            var request = new TvRequests
            {
                Id = 8,
                Title = "TV Title",
                ChildRequests = new List<ChildRequests>
                {
                    new ChildRequests { Id = 1, RequestedUserId = "u1", RequestedUser = new OmbiUser { Id = "u1", Email = "u1@test.local" } },
                    new ChildRequests { Id = 2, RequestedUserId = "u1", RequestedUser = new OmbiUser { Id = "u1", Email = "u1@test.local" } },
                    new ChildRequests { Id = 3, RequestedUserId = "u2", RequestedUser = new OmbiUser { Id = "u2", Email = "u2@test.local" } },
                }
            };

            repo.Setup(x => x.Get()).Returns(new List<TvRequests> { request }.AsQueryable().BuildMock());
            repo.Setup(x => x.Delete(request)).Returns(Task.CompletedTask);
            mocker.GetMock<IMediaCacheService>().Setup(x => x.Purge()).Returns(Task.CompletedTask);
            mocker.GetMock<INotificationHelper>().Setup(x => x.Notify(It.IsAny<NotificationOptions>())).Returns(Task.CompletedTask);

            await subject.RemoveTvRequest(8);

            mocker.GetMock<INotificationHelper>().Verify(x => x.Notify(It.Is<NotificationOptions>(n =>
                n.NotificationType == NotificationType.RequestDeleted &&
                n.UserId == "u1" &&
                n.RequestId == 0
            )), Times.Once);
            mocker.GetMock<INotificationHelper>().Verify(x => x.Notify(It.Is<NotificationOptions>(n =>
                n.NotificationType == NotificationType.RequestDeleted &&
                n.UserId == "u2" &&
                n.RequestId == 0
            )), Times.Once);
        }

        [Test]
        public async Task RemoveAlbumRequest_DoesNotNotify_WhenUserCannotManageRequest()
        {
            var (subject, repo, mocker, userManager) = CreateMusicEngine();
            userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(false);

            var request = new AlbumRequest
            {
                Id = 99,
                RequestType = RequestType.Album,
                RequestedUserId = "different-user",
                RequestedUser = new OmbiUser { Id = "different-user", Email = "album@test.local" },
                Title = "Album Title"
            };

            repo.Setup(x => x.GetAll()).Returns(new List<AlbumRequest> { request }.AsQueryable().BuildMock());
            mocker.GetMock<INotificationHelper>().Setup(x => x.Notify(It.IsAny<NotificationOptions>())).Returns(Task.CompletedTask);

            var result = await subject.RemoveAlbumRequest(99);

            Assert.That(result.Result, Is.False);
            repo.Verify(x => x.Delete(It.IsAny<AlbumRequest>()), Times.Never);
            mocker.GetMock<INotificationHelper>().Verify(x => x.Notify(It.IsAny<NotificationOptions>()), Times.Never);
        }

        private static (TvRequestEngine subject, Mock<ITvRequestRepository> repo, AutoMocker mocker, Mock<Ombi.Core.Authentication.OmbiUserManager> userManager) CreateTvEngine()
        {
            var mocker = new AutoMocker();
            var repo = new Mock<ITvRequestRepository>();
            var requestService = new Mock<IRequestServiceMain>();
            requestService.Setup(x => x.TvRequestService).Returns(repo.Object);
            requestService.Setup(x => x.MovieRequestService).Returns(new Mock<IMovieRequestRepository>().Object);
            requestService.Setup(x => x.MusicRequestRepository).Returns(new Mock<IMusicRequestRepository>().Object);

            var userManager = MockHelper.MockUserManager(new List<OmbiUser> { new OmbiUser { NormalizedUserName = "TEST", Id = "a" } });
            userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(true);

            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.Name).Returns("Test");
            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.Identity).Returns(identity.Object);

            var currentUser = new Mock<ICurrentUser>();
            currentUser.Setup(x => x.Identity).Returns(identity.Object);
            currentUser.Setup(x => x.Username).Returns("Test");
            currentUser.Setup(x => x.GetUser()).ReturnsAsync(new OmbiUser { NormalizedUserName = "TEST", Id = "a" });

            mocker.Use(principal.Object);
            mocker.Use(currentUser.Object);
            mocker.Use(userManager.Object);
            mocker.Use(requestService.Object);

            var subject = mocker.CreateInstance<TvRequestEngine>();
            return (subject, repo, mocker, userManager);
        }

        private static (MusicRequestEngine subject, Mock<IMusicRequestRepository> repo, AutoMocker mocker, Mock<Ombi.Core.Authentication.OmbiUserManager> userManager) CreateMusicEngine()
        {
            var mocker = new AutoMocker();
            var repo = new Mock<IMusicRequestRepository>();
            var requestService = new Mock<IRequestServiceMain>();
            requestService.Setup(x => x.MusicRequestRepository).Returns(repo.Object);
            requestService.Setup(x => x.MovieRequestService).Returns(new Mock<IMovieRequestRepository>().Object);
            requestService.Setup(x => x.TvRequestService).Returns(new Mock<ITvRequestRepository>().Object);

            var userManager = MockHelper.MockUserManager(new List<OmbiUser> { new OmbiUser { NormalizedUserName = "TEST", Id = "a" } });
            userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(true);

            var identity = new Mock<IIdentity>();
            identity.Setup(x => x.Name).Returns("Test");
            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.Identity).Returns(identity.Object);

            var currentUser = new Mock<ICurrentUser>();
            currentUser.Setup(x => x.Identity).Returns(identity.Object);
            currentUser.Setup(x => x.Username).Returns("Test");
            currentUser.Setup(x => x.GetUser()).ReturnsAsync(new OmbiUser { NormalizedUserName = "TEST", Id = "a" });

            mocker.Use(principal.Object);
            mocker.Use(currentUser.Object);
            mocker.Use(userManager.Object);
            mocker.Use(requestService.Object);
            mocker.Setup<IRepository<RequestSubscription>, IQueryable<RequestSubscription>>(x => x.GetAll()).Returns(new List<RequestSubscription>().AsQueryable().BuildMock());

            var subject = mocker.CreateInstance<MusicRequestEngine>();
            return (subject, repo, mocker, userManager);
        }
    }
}
