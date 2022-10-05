using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Moq;
using MockQueryable.Moq;
using NUnit.Framework;
using Ombi.Core;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Helpers;
using Ombi.Core.Services;
using Ombi.Tests;
using Moq.AutoMock;
using Ombi.Settings.Settings.Models;
using Ombi.Notifications.Models;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class PlexAvailabilityCheckerTests
    {
        private AutoMocker _mocker;
        private PlexAvailabilityChecker _subject;
        
        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            
            var hub = SignalRHelper.MockHub<NotificationHub>();
            _mocker.Use(hub);

            _subject = _mocker.CreateInstance<PlexAvailabilityChecker>();
        }

        [Test]
        public async Task ProcessMovies_ShouldMarkAvailable_WhenInPlex_WithImdbId()
        {
            var request = new MovieRequests
            {
                ImdbId = "test"
            };
            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(new List<MovieRequests> { request }.AsQueryable());
            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x => x.Get("test", ProviderType.ImdbId)).ReturnsAsync(new PlexServerContent());

            await _subject.Execute(null);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available, Is.True);
                Assert.That(request.MarkedAsAvailable, Is.Not.Null);
                Assert.That(request.Available4K, Is.False);
                Assert.That(request.MarkedAsAvailable4K, Is.Null);
            });

            _mocker.Verify<IMovieRequestRepository>(x => x.SaveChangesAsync(), Times.Once);
            _mocker.Verify<IPlexContentRepository>(x => x.Get("test", ProviderType.ImdbId), Times.Once);
            _mocker.Verify<IPlexContentRepository>(x => x.Get(It.IsAny<string>(), ProviderType.TheMovieDbId), Times.Never);
            _mocker.Verify<INotificationHelper>(x => x.Notify(It.Is<NotificationOptions>(x => x.NotificationType == NotificationType.RequestAvailable)), Times.Once);
        }

        [Test]
        public async Task ProcessMovies_ShouldMarkAvailable_WhenInPlex_WithTheMovieDbId()
        {
            var request = new MovieRequests
            {
                ImdbId = null,
                TheMovieDbId = 33
            };
            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(new List<MovieRequests> { request }.AsQueryable());
            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x => x.Get(It.IsAny<string>(), ProviderType.ImdbId)).ReturnsAsync((PlexServerContent)null);
            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x => x.Get("33", ProviderType.TheMovieDbId)).ReturnsAsync(new PlexServerContent());

            await _subject.Execute(null);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available, Is.True);
                Assert.That(request.MarkedAsAvailable, Is.Not.Null);
                Assert.That(request.Available4K, Is.False);
                Assert.That(request.MarkedAsAvailable4K, Is.Null);
            });

            _mocker.Verify<IMovieRequestRepository>(x => x.SaveChangesAsync(), Times.Once);
            _mocker.Verify<IPlexContentRepository>(x => x.Get(It.IsAny<string>(), ProviderType.ImdbId), Times.Never);
            _mocker.Verify<IPlexContentRepository>(x => x.Get(It.IsAny<string>(), ProviderType.TheMovieDbId), Times.Once);
            _mocker.Verify<INotificationHelper>(x => x.Notify(It.Is<NotificationOptions>(x => x.NotificationType == NotificationType.RequestAvailable)), Times.Once);
        }

        [Test]
        public async Task ProcessMovies_ShouldMarkAvailable_WhenInPlex_WithTheMovieDbId_4K_Enabled ()
        {
            _mocker.Setup<IFeatureService, Task<bool>>(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(true);
            var request = new MovieRequests
            {
                ImdbId = "test"
            };
            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(new List<MovieRequests> { request }.AsQueryable());
            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x => x.Get("test", ProviderType.ImdbId)).ReturnsAsync(new PlexServerContent {  Quality = "1080p" });

            await _subject.Execute(null);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available, Is.True);
                Assert.That(request.MarkedAsAvailable, Is.Not.Null);
                Assert.That(request.Available4K, Is.False);
                Assert.That(request.MarkedAsAvailable4K, Is.Null);
            });

            _mocker.Verify<IMovieRequestRepository>(x => x.SaveChangesAsync(), Times.Once);
            _mocker.Verify<IPlexContentRepository>(x => x.Get("test", ProviderType.ImdbId), Times.Once);
            _mocker.Verify<IPlexContentRepository>(x => x.Get(It.IsAny<string>(), ProviderType.TheMovieDbId), Times.Never);
            _mocker.Verify<INotificationHelper>(x => x.Notify(It.Is<NotificationOptions>(x => x.NotificationType == NotificationType.RequestAvailable)), Times.Once);
        }

        [Test]
        public async Task ProcessMovies_4K_ShouldMarkAvailable_WhenInPlex_WithImdbId_And_4K_FeatureEnabled()
        {
            _mocker.Setup<IFeatureService, Task<bool>>(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(true);
            var request = new MovieRequests
            {
                ImdbId = "test",
                Is4kRequest = true,
                Has4KRequest = true,
            };
            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(new List<MovieRequests> { request }.AsQueryable());
            _mocker.Setup<IPlexContentRepository, Task<PlexServerContent>>(x => x.Get("test", ProviderType.ImdbId)).ReturnsAsync(new PlexServerContent { Has4K = true });

            await _subject.Execute(null);

            _mocker.Verify<IMovieRequestRepository>(x => x.SaveChangesAsync(), Times.Once);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available, Is.False);
                Assert.That(request.MarkedAsAvailable, Is.Null);
                Assert.That(request.Available4K, Is.True);
                Assert.That(request.MarkedAsAvailable4K, Is.Not.Null);
            });

            _mocker.Verify<IMovieRequestRepository>(x => x.SaveChangesAsync(), Times.Once);
            _mocker.Verify<IPlexContentRepository>(x => x.Get("test", ProviderType.ImdbId), Times.Once);
            _mocker.Verify<IPlexContentRepository>(x => x.Get(It.IsAny<string>(), ProviderType.TheMovieDbId), Times.Never);
            _mocker.Verify<INotificationHelper>(x => x.Notify(It.Is<NotificationOptions>(x => x.NotificationType == NotificationType.RequestAvailable)), Times.Once);
        }

        [Test]
        public async Task ProcessMovies_ShouldNotBeAvailable_WhenInNotPlex()
        {
            var request = new MovieRequests
            {
                ImdbId = "test"
            };
            _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll()).Returns(new List<MovieRequests> { request }.AsQueryable());

            await _subject.Execute(null);

            Assert.False(request.Available);
        }

        [Test]
        public async Task ProcessTv_ShouldMark_Episode_Available_WhenInPlex_MovieDbId()
        {
            var request = CreateChildRequest(null, 33, 99);
            _mocker.Setup<ITvRequestRepository, IQueryable<ChildRequests>>(x => x.GetChild()).Returns(new List<ChildRequests> { request }.AsQueryable().BuildMock());
            _mocker.Setup<IPlexContentRepository, IQueryable<IMediaServerEpisode>>(x => x.GetAllEpisodes()).Returns(new List<PlexEpisode>
            {
                new PlexEpisode
                {
                    Series = new PlexServerContent
                    {
                        TheMovieDbId = 33.ToString(),
                        Title = "Test"
                    },
                    EpisodeNumber = 1,
                    SeasonNumber = 2,
                }
            }.AsQueryable().BuildMock());

            await _subject.Execute(null);

            _mocker.Verify<ITvRequestRepository>(x => x.Save(), Times.AtLeastOnce);

            Assert.True(request.SeasonRequests[0].Episodes[0].Available);
        }

        [Test]
        public async Task ProcessTv_ShouldMark_Episode_Available_WhenInPlex_ImdbId()
        {
            var request = CreateChildRequest("abc", -1, 99);
            _mocker.Setup<ITvRequestRepository, IQueryable<ChildRequests>>(x => x.GetChild()).Returns(new List<ChildRequests> { request }.AsQueryable().BuildMock());
            _mocker.Setup<IPlexContentRepository, IQueryable<IMediaServerEpisode>>(x => x.GetAllEpisodes()).Returns(new List<PlexEpisode>
            {
                new PlexEpisode
                {
                    Series = new  PlexServerContent
                    {
                        ImdbId = "abc",
                    },
                    EpisodeNumber = 1,
                    SeasonNumber = 2,
                }
            }.AsQueryable().BuildMock());

            await _subject.Execute(null);

            _mocker.Verify<ITvRequestRepository>(x => x.Save(), Times.AtLeastOnce);

            Assert.True(request.SeasonRequests[0].Episodes[0].Available);
        }

        [Test]
        public async Task ProcessTv_ShouldMark_Episode_Available_By_TitleMatch()
        {
            var request = CreateChildRequest("abc", -1, 99);
            _mocker.Setup<ITvRequestRepository, IQueryable<ChildRequests>>(x => x.GetChild()).Returns(new List<ChildRequests> { request }.AsQueryable().BuildMock());
            _mocker.Setup<IPlexContentRepository, IQueryable<IMediaServerEpisode>>(x => x.GetAllEpisodes()).Returns(new List<PlexEpisode>
            {
                new PlexEpisode
                {
                    Series = new  PlexServerContent
                    {
                        Title = "UNITTEST",
                        ImdbId = "invlaid",
                    },
                    EpisodeNumber = 1,
                    SeasonNumber = 2,
                }
            }.AsQueryable().BuildMock());

            await _subject.Execute(null);

            _mocker.Verify<ITvRequestRepository>(x => x.Save(), Times.AtLeastOnce);

            Assert.True(request.SeasonRequests[0].Episodes[0].Available);
        }

        private ChildRequests CreateChildRequest(string imdbId, int theMovieDbId, int tvdbId)
        {
            return new ChildRequests
            {
                Title = "UnitTest",
                ParentRequest = new TvRequests { ImdbId = imdbId, ExternalProviderId = theMovieDbId, TvDbId = tvdbId },
                SeasonRequests = new EditableList<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new EditableList<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                EpisodeNumber = 1,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 2
                                }
                            }
                        }
                    }
                },
                RequestedUser = new OmbiUser
                {
                    Email = "abc"
                }
            };
        }
    }
}