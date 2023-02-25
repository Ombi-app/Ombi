using AutoFixture;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Services
{
    [TestFixture]
    public class RecentlyRequestedServiceTests
    {
        private AutoMocker _mocker;
        private RecentlyRequestedService _subject;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _mocker = new AutoMocker();

            _mocker.Setup<ICurrentUser, Task<OmbiUser>>(x => x.GetUser()).ReturnsAsync(new OmbiUser { UserName = "test", Alias = "alias", Language = "en" });
            _mocker.Setup<ICurrentUser, string>(x => x.Username).Returns("test");
            _subject = _mocker.CreateInstance<RecentlyRequestedService>();
        }

        [Test]
        public async Task GetRecentlyRequested_Movies()
        {
            _mocker.Setup<ISettingsService<CustomizationSettings>, Task<CustomizationSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new CustomizationSettings());
            var releaseDate = new DateTime(2019, 01, 01);
            var requestDate = DateTime.Now;
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id = 1,
                    Approved = true,
                    Available = true,
                    ReleaseDate = releaseDate,
                    Title = "title",
                    Overview = "overview",
                    RequestedDate = requestDate,
                    RequestedUser = new Store.Entities.OmbiUser
                    {
                        UserName = "a"
                    },
                    RequestedUserId = "b",
                }
            };
            var albums = new List<AlbumRequest>();
            var chilRequests = new List<ChildRequests>();
            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(movies.AsQueryable().BuildMock());
            _mocker.Setup<IMusicRequestRepository, IQueryable<AlbumRequest>>(x => x.GetAll()).Returns(albums.AsQueryable().BuildMock());
            _mocker.Setup<ITvRequestRepository, IQueryable<ChildRequests>>(x => x.GetChild()).Returns(chilRequests.AsQueryable().BuildMock());

            var result = await _subject.GetRecentlyRequested(CancellationToken.None);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First(), Is.InstanceOf<RecentlyRequestedModel>()
                .With.Property(nameof(RecentlyRequestedModel.RequestId)).EqualTo(1)
                .With.Property(nameof(RecentlyRequestedModel.Approved)).EqualTo(true)
                .With.Property(nameof(RecentlyRequestedModel.Available)).EqualTo(true)
                .With.Property(nameof(RecentlyRequestedModel.Title)).EqualTo("title")
                .With.Property(nameof(RecentlyRequestedModel.Overview)).EqualTo("overview")
                .With.Property(nameof(RecentlyRequestedModel.RequestDate)).EqualTo(requestDate)
                .With.Property(nameof(RecentlyRequestedModel.ReleaseDate)).EqualTo(releaseDate)
                .With.Property(nameof(RecentlyRequestedModel.Type)).EqualTo(RequestType.Movie)
                );
        }

        [Test]
        public async Task GetRecentlyRequested_Movies_HideAvailable()
        {
            _mocker.Setup<ISettingsService<CustomizationSettings>, Task<CustomizationSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new CustomizationSettings() { HideAvailableRecentlyRequested = true });
            var releaseDate = new DateTime(2019, 01, 01);
            var requestDate = DateTime.Now;
            var movies = new List<MovieRequests>
            {
                new MovieRequests
                {
                    Id = 1,
                    Approved = true,
                    Available = true,
                    ReleaseDate = releaseDate,
                    Title = "title",
                    Overview = "overview",
                    RequestedDate = requestDate,
                    RequestedUser = new Store.Entities.OmbiUser
                    {
                        UserName = "a"
                    },
                    RequestedUserId = "b",
                },

                new MovieRequests
                {
                    Id = 1,
                    Approved = true,
                    Available = false,
                    ReleaseDate = releaseDate,
                    Title = "title2",
                    Overview = "overview2",
                    RequestedDate = requestDate,
                    RequestedUser = new Store.Entities.OmbiUser
                    {
                        UserName = "a"
                    },
                    RequestedUserId = "b",
                }
            };
            var albums = new List<AlbumRequest>();
            var chilRequests = new List<ChildRequests>();
            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(movies.AsQueryable().BuildMock());
            _mocker.Setup<IMusicRequestRepository, IQueryable<AlbumRequest>>(x => x.GetAll()).Returns(albums.AsQueryable().BuildMock());
            _mocker.Setup<ITvRequestRepository, IQueryable<ChildRequests>>(x => x.GetChild()).Returns(chilRequests.AsQueryable().BuildMock());

            var result = await _subject.GetRecentlyRequested(CancellationToken.None);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First(), Is.InstanceOf<RecentlyRequestedModel>()
                .With.Property(nameof(RecentlyRequestedModel.RequestId)).EqualTo(1)
                .With.Property(nameof(RecentlyRequestedModel.Approved)).EqualTo(true)
                .With.Property(nameof(RecentlyRequestedModel.Available)).EqualTo(false)
                .With.Property(nameof(RecentlyRequestedModel.Title)).EqualTo("title2")
                .With.Property(nameof(RecentlyRequestedModel.Overview)).EqualTo("overview2")
                .With.Property(nameof(RecentlyRequestedModel.RequestDate)).EqualTo(requestDate)
                .With.Property(nameof(RecentlyRequestedModel.ReleaseDate)).EqualTo(releaseDate)
                .With.Property(nameof(RecentlyRequestedModel.Type)).EqualTo(RequestType.Movie)
                );
        }

        [Test]
        public async Task GetRecentlyRequested()
        {
            _mocker.Setup<ISettingsService<CustomizationSettings>, Task<CustomizationSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new CustomizationSettings());
            var releaseDate = new DateTime(2019, 01, 01);
            var requestDate = DateTime.Now;

            var movies = _fixture.CreateMany<MovieRequests>(10);
            var albums = _fixture.CreateMany<AlbumRequest>(10);
            var chilRequests = _fixture.CreateMany<ChildRequests>(10);

            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(movies.AsQueryable().BuildMock());
            _mocker.Setup<IMusicRequestRepository, IQueryable<AlbumRequest>>(x => x.GetAll()).Returns(albums.AsQueryable().BuildMock());
            _mocker.Setup<ITvRequestRepository, IQueryable<ChildRequests>>(x => x.GetChild()).Returns(chilRequests.AsQueryable().BuildMock());

            var result = await _subject.GetRecentlyRequested(CancellationToken.None);

            Assert.That(result.Count, Is.EqualTo(21));
        }


        [Test]
        public async Task GetRecentlyRequested_HideUsernames()
        {
            _mocker.Setup<ISettingsService<CustomizationSettings>, Task<CustomizationSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new CustomizationSettings());
            _mocker.Setup<ISettingsService<OmbiSettings>, Task<OmbiSettings>>(x => x.GetSettingsAsync())
    .ReturnsAsync(new OmbiSettings { HideRequestsUsers = true });
            var releaseDate = new DateTime(2019, 01, 01);
            var requestDate = DateTime.Now;

            var movies = _fixture.CreateMany<MovieRequests>(10).ToList();
            var albums = _fixture.CreateMany<AlbumRequest>(10);
            var chilRequests = _fixture.CreateMany<ChildRequests>(10);
            movies.Add(_fixture.Build<MovieRequests>().With(x => x.RequestedUserId, "a").With(x => x.Title, "unit").Create());

            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(movies.AsQueryable().BuildMock());
            _mocker.Setup<IMusicRequestRepository, IQueryable<AlbumRequest>>(x => x.GetAll()).Returns(albums.AsQueryable().BuildMock());
            _mocker.Setup<ITvRequestRepository, IQueryable<ChildRequests>>(x => x.GetChild()).Returns(chilRequests.AsQueryable().BuildMock());
            _mocker.Setup<ICurrentUser, Task<OmbiUser>>(x => x.GetUser()).ReturnsAsync(new OmbiUser { UserName = "test", Id = "a", Alias = "alias", UserType = UserType.LocalUser  });
            _mocker.Setup<ICurrentUser, string>(x => x.Username).Returns("test");
            _mocker.Setup<OmbiUserManager, Task<bool>>(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(false);

            var result = await _subject.GetRecentlyRequested(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result.First().Title, Is.EqualTo("unit"));
            });
        }
    }
}
