using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class MovieRequestEngineGetRequestTests
    {
        private const string UserA = "user-a";
        private const string UserB = "user-b";

        private MovieRequestEngine _engine;
        private Mock<IMovieRequestRepository> _repo;
        private Mock<ICurrentUser> _currentUser;
        private Mock<ICacheService> _cache;
        private Mock<OmbiUserManager> _userManager;
        private OmbiSettings _settings;
        private List<MovieRequests> _requests;

        [SetUp]
        public void Setup()
        {
            _requests = new List<MovieRequests>
            {
                new MovieRequests { Id = 1, RequestedUserId = UserA, RequestedUser = new OmbiUser { Id = UserA, UserAccessToken = "token-a" } },
                new MovieRequests { Id = 2, RequestedUserId = UserB, RequestedUser = new OmbiUser { Id = UserB, UserAccessToken = "token-b" } },
            };

            _repo = new Mock<IMovieRequestRepository>();
            _repo.Setup(x => x.GetWithUser()).Returns(() => _requests.AsQueryable().BuildMock());
            _repo.Setup(x => x.GetWithUser(It.IsAny<string>()))
                .Returns((string userId) => _requests.Where(r => r.RequestedUserId == userId).AsQueryable().BuildMock());

            var requestService = new Mock<IRequestServiceMain>();
            requestService.Setup(x => x.MovieRequestService).Returns(_repo.Object);

            _currentUser = new Mock<ICurrentUser>();
            SetCurrentUser(UserA);

            _settings = new OmbiSettings { HideRequestsUsers = true };
            _cache = new Mock<ICacheService>();
            _cache.Setup(x => x.GetOrAddAsync(It.IsAny<string>(), It.IsAny<Func<Task<OmbiSettings>>>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(() => _settings);

            var subs = new Mock<IRepository<RequestSubscription>>();
            subs.Setup(x => x.GetAll()).Returns(new List<RequestSubscription>().AsQueryable().BuildMock());
            var played = new Mock<IUserPlayedMovieRepository>();
            played.Setup(x => x.GetAll()).Returns(new List<UserPlayedMovie>().AsQueryable().BuildMock());

            _userManager = MockHelper.MockUserManager(new List<OmbiUser>());
            _userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(false);

            _engine = new MovieRequestEngine(new Mock<IMovieDbApi>().Object, requestService.Object, _currentUser.Object,
                new Mock<INotificationHelper>().Object, new Mock<IRuleEvaluator>().Object, new Mock<IMovieSender>().Object,
                new Mock<ILogger<MovieRequestEngine>>().Object, _userManager.Object, new Mock<IRepository<RequestLog>>().Object,
                _cache.Object, new Mock<ISettingsService<OmbiSettings>>().Object, subs.Object, new Mock<IMediaCacheService>().Object,
                new Mock<IFeatureService>().Object, played.Object);
        }

        private void SetCurrentUser(string id)
        {
            var user = new OmbiUser { Id = id, UserName = id, UserType = UserType.LocalUser };
            _currentUser.Setup(x => x.Username).Returns(id);
            _currentUser.Setup(x => x.GetUser()).ReturnsAsync(user);
        }

        [Test]
        public async Task GetRequest_HideEnabled_OtherUsersRequest_ReturnsNull()
        {
            var result = await _engine.GetRequest(2);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetRequest_HideEnabled_OwnRequest_ReturnsRequest()
        {
            var result = await _engine.GetRequest(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
        }

        [Test]
        public async Task GetRequest_HideDisabled_OtherUsersRequest_ReturnsRequest()
        {
            _settings.HideRequestsUsers = false;

            var result = await _engine.GetRequest(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(2));
        }

        [TestCase("Admin")]
        [TestCase("PowerUser")]
        public async Task GetRequest_HideEnabled_PrivilegedUser_CanSeeOtherUsersRequest(string role)
        {
            _userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), role)).ReturnsAsync(true);

            var result = await _engine.GetRequest(2);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(2));
        }

        [Test]
        public async Task GetRequest_RequestDoesNotExist_ReturnsNull()
        {
            _settings.HideRequestsUsers = false;

            var result = await _engine.GetRequest(999);

            Assert.That(result, Is.Null);
        }
    }
}
