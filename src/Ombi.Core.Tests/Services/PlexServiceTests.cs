using MockQueryable.Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Models;
using Ombi.Core.Services;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserType = Ombi.Store.Entities.UserType;

namespace Ombi.Core.Tests.Services
{
    public class PlexServiceTests
    {

        private PlexService _subject;
        private AutoMocker _mocker;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<PlexService>();
        }

        [Test]
        public async Task GetWatchListUsers_AllUsersSynced()
        {
            var userMock = MockHelper.MockUserManager(new List<OmbiUser>
            {
                new OmbiUser
                {
                    MediaServerToken = "token",
                    Id = "1",
                    UserName = "user1",
                    UserType = UserType.PlexUser,
                },
                new OmbiUser
                {
                    MediaServerToken = "token",
                    Id = "2",
                    UserName = "user2",
                    UserType = UserType.PlexUser,
                },
                new OmbiUser
                {
                    MediaServerToken = "token",
                    Id = "2",
                    UserName = "user2",
                    UserType = UserType.LocalUser,
                }
            });

            _mocker.Use(userMock.Object);
            _subject = _mocker.CreateInstance<PlexService>();

            _mocker.Setup<IRepository<PlexWatchlistUserError>, IQueryable<PlexWatchlistUserError>>(x => x.GetAll())
                .Returns(new List<PlexWatchlistUserError>().AsQueryable().BuildMock());

            var result = await _subject.GetWatchlistUsers(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.All(x => x.SyncStatus == WatchlistSyncStatus.Successful));
                Assert.That(result.Count, Is.EqualTo(2));
            });
        }

        [Test]
        public async Task GetWatchListUsers_NotEnabled()
        {
            var userMock = MockHelper.MockUserManager(new List<OmbiUser>
            {
                new OmbiUser
                {
                    MediaServerToken = "",
                    Id = "1",
                    UserName = "user1",
                    UserType = UserType.PlexUser,
                },
                new OmbiUser
                {
                    MediaServerToken = null,
                    Id = "2",
                    UserName = "user2",
                    UserType = UserType.PlexUser,
                },
            });

            _mocker.Use(userMock.Object);
            _subject = _mocker.CreateInstance<PlexService>();

            _mocker.Setup<IRepository<PlexWatchlistUserError>, IQueryable<PlexWatchlistUserError>>(x => x.GetAll())
                .Returns(new List<PlexWatchlistUserError>().AsQueryable().BuildMock());

            var result = await _subject.GetWatchlistUsers(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.All(x => x.SyncStatus == WatchlistSyncStatus.NotEnabled));
                Assert.That(result.Count, Is.EqualTo(2));
            });
        }


        [Test]
        public async Task GetWatchListUsers_Failed()
        {
            var userMock = MockHelper.MockUserManager(new List<OmbiUser>
            {
                new OmbiUser
                {
                    MediaServerToken = "test",
                    Id = "1",
                    UserName = "user1",
                    UserType = UserType.PlexUser,
                },
            });

            _mocker.Use(userMock.Object);
            _subject = _mocker.CreateInstance<PlexService>();

            _mocker.Setup<IRepository<PlexWatchlistUserError>, IQueryable<PlexWatchlistUserError>>(x => x.GetAll())
                .Returns(new List<PlexWatchlistUserError>
                {
                    new PlexWatchlistUserError
                    {
                        UserId = "1",
                        MediaServerToken = "test",
                    }
                }.AsQueryable().BuildMock());

            var result = await _subject.GetWatchlistUsers(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.All(x => x.SyncStatus == WatchlistSyncStatus.Failed));
                Assert.That(result.Count, Is.EqualTo(1));
            });
        }
    }
}
