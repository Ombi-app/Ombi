using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Models;
using Ombi.Core.Services;
using Ombi.Store.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserType = Ombi.Store.Entities.UserType;

namespace Ombi.Core.Tests.Services
{
    [TestFixture]
    public class PlexServiceTests
    {
        private AutoMocker _mocker;
        private PlexService _subject;
        private Mock<OmbiUserManager> _userManagerMock;
        private Mock<IPlexWatchlistStatusStore> _statusStoreMock;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _userManagerMock = _mocker.GetMock<OmbiUserManager>();
            _statusStoreMock = _mocker.GetMock<IPlexWatchlistStatusStore>();
            _subject = _mocker.CreateInstance<PlexService>();
        }

        [Test]
        public async Task GetWatchlistUsers_NoPlexUsers_ReturnsEmptyList()
        {
            SetupUsers(CreateUsers(0, UserType.LocalUser));

            var result = await _subject.GetWatchlistUsers(CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetWatchlistUsers_WithStoredSuccess_ReturnsSuccessfulStatus()
        {
            var users = CreateUsers(2, UserType.PlexUser);
            SetupUsers(users);
            _statusStoreMock.Setup(x => x.Get(It.IsAny<string>())).Returns(WatchlistSyncStatus.Successful);

            var result = await _subject.GetWatchlistUsers(CancellationToken.None);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(x => x.SyncStatus == WatchlistSyncStatus.Successful), Is.True);
        }

        [Test]
        public async Task GetWatchlistUsers_WithStoredFailure_ReturnsFailedStatus()
        {
            var users = CreateUsers(1, UserType.PlexUser);
            SetupUsers(users);
            _statusStoreMock.Setup(x => x.Get(users[0].Id)).Returns(WatchlistSyncStatus.Failed);

            var result = await _subject.GetWatchlistUsers(CancellationToken.None);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].SyncStatus, Is.EqualTo(WatchlistSyncStatus.Failed));
        }

        [Test]
        public async Task GetWatchlistUsers_NoStatusStored_ReturnsNotAFriend()
        {
            var users = CreateUsers(1, UserType.PlexUser);
            SetupUsers(users);
            _statusStoreMock.Setup(x => x.Get(It.IsAny<string>())).Returns((WatchlistSyncStatus?)null);

            var result = await _subject.GetWatchlistUsers(CancellationToken.None);

            Assert.That(result[0].SyncStatus, Is.EqualTo(WatchlistSyncStatus.NotAFriend));
        }

        [Test]
        public async Task GetWatchlistUsers_MixedUserTypes_ReturnsOnlyPlexUsers()
        {
            var plex = CreateUsers(2, UserType.PlexUser);
            var local = CreateUsers(3, UserType.LocalUser);
            SetupUsers(plex.Concat(local).ToList());
            _statusStoreMock.Setup(x => x.Get(It.IsAny<string>())).Returns(WatchlistSyncStatus.Successful);

            var result = await _subject.GetWatchlistUsers(CancellationToken.None);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(x => plex.Any(u => u.Id == x.UserId)), Is.True);
        }

        [Test]
        public async Task ForceRevalidateWatchlistUsers_ClearsStatusStore()
        {
            await _subject.ForceRevalidateWatchlistUsers(CancellationToken.None);

            _statusStoreMock.Verify(x => x.Clear(), Times.Once);
        }

        private void SetupUsers(List<OmbiUser> users)
        {
            var queryable = users.AsQueryable().BuildMock();
            _userManagerMock.Setup(x => x.Users).Returns(queryable);
        }

        private static List<OmbiUser> CreateUsers(int count, UserType userType)
        {
            var users = new List<OmbiUser>();
            for (var i = 0; i < count; i++)
            {
                users.Add(new OmbiUser
                {
                    Id = $"user-{i}-{userType}",
                    UserName = $"{userType}{i}",
                    UserType = userType,
                });
            }
            return users;
        }
    }
}
