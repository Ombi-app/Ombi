using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core;
using Ombi.Core.Settings;
using Ombi.Hubs;
using Ombi.Notifications.Models;
using Ombi.Schedule.Jobs;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class AvailabilityCheckerTests
    {
        private AutoMocker _mocker;
        private TestAvailabilityChecker _subject;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMocker();
            var hub = SignalRHelper.MockHub<NotificationHub>();
            _mocker.Use(hub);

            _subject = _mocker.CreateInstance<TestAvailabilityChecker>();
        }

        [Test]
        public async Task All_Episodes_Are_Available_In_Request()
        {
            var request = new ChildRequests
            {
                Title = "Test",
                Id = 1,
                RequestedUser = new OmbiUser { Email = "" },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                Available = false,
                                EpisodeNumber = 1,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            },
                            new EpisodeRequests
                            {
                                Available = false,
                                EpisodeNumber = 2,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            }
                        }
                    }
                }
            };

            var databaseEpisodes = new List<IBaseMediaServerEpisode>
            {
                new PlexEpisode
                {
                    EpisodeNumber = 1,
                    SeasonNumber = 1,
                },
                new PlexEpisode
                {
                    EpisodeNumber = 2,
                    SeasonNumber = 1,
                },
            }.AsQueryable().BuildMock();

            await _subject.ProcessTvShow(databaseEpisodes, request);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available, Is.True);
                Assert.That(request.MarkedAsAvailable, Is.Not.Null);
                Assert.That(request.SeasonRequests[0].Episodes[0].Available, Is.True);
                Assert.That(request.SeasonRequests[0].Episodes[1].Available, Is.True);
            });

            Assert.Multiple(() =>
            {
                _mocker.Verify<ITvRequestRepository>(x => x.Save(), Times.Exactly(2));
                _mocker.Verify<INotificationHelper>(x => x.Notify(It.Is<NotificationOptions>(x => x.NotificationType == Helpers.NotificationType.RequestAvailable && x.RequestId == 1)), Times.Once);
            });
        }

        [Test]
        public async Task All_One_Episode_Is_Available_In_Request()
        {
            var request = new ChildRequests
            {
                Title = "Test",
                Id = 1,
                RequestedUser = new OmbiUser { Email = "" },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                Available = false,
                                EpisodeNumber = 1,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            },
                            new EpisodeRequests
                            {
                                Available = false,
                                EpisodeNumber = 2,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            },
                            new EpisodeRequests
                            {
                                Available = true,
                                EpisodeNumber = 3,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            }
                        }
                    }
                }
            };

            var databaseEpisodes = new List<IBaseMediaServerEpisode>
            {
                new PlexEpisode
                {
                    EpisodeNumber = 1,
                    SeasonNumber = 1,
                },
                new PlexEpisode
                {
                    EpisodeNumber = 3,
                    SeasonNumber = 1,
                },
            }.AsQueryable().BuildMock();

            await _subject.ProcessTvShow(databaseEpisodes, request);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available, Is.False);
                Assert.That(request.MarkedAsAvailable, Is.Null);
                Assert.That(request.SeasonRequests[0].Episodes[0].Available, Is.True);
                Assert.That(request.SeasonRequests[0].Episodes[1].Available, Is.False);
            });

            Assert.Multiple(() =>
            {
                _mocker.Verify<ITvRequestRepository>(x => x.Save(), Times.Once);
                _mocker.Verify<INotificationHelper>(x => x.Notify(It.Is<NotificationOptions>(x => x.NotificationType == Helpers.NotificationType.PartiallyAvailable && x.RequestId == 1)), Times.Once);
            });
        }

        [Test]
        public async Task ShouldDeferToRadarr_WhenRadarrDisabled_ReturnsFalse()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<RadarrSettings>, Task<RadarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = false, ScanForAvailability = true, PrioritizeArrAvailability = true });

            var result = await subject.TestShouldDeferToRadarr(123, false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ShouldDeferToRadarr_WhenScanForAvailabilityDisabled_ReturnsFalse()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<RadarrSettings>, Task<RadarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = true, ScanForAvailability = false, PrioritizeArrAvailability = true });

            var result = await subject.TestShouldDeferToRadarr(123, false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ShouldDeferToRadarr_WhenPrioritizeArrAvailabilityDisabled_ReturnsFalse()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<RadarrSettings>, Task<RadarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = false });

            var result = await subject.TestShouldDeferToRadarr(123, false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ShouldDeferToRadarr_WhenNotInCache_ReturnsFalse()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<RadarrSettings>, Task<RadarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = true });
            _mocker.Setup<IExternalRepository<RadarrCache>, IQueryable<RadarrCache>>(x => x.GetAll())
                .Returns(new List<RadarrCache>().AsQueryable().BuildMock());

            var result = await subject.TestShouldDeferToRadarr(123, false);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ShouldDeferToRadarr_WhenInCacheWithFile_ReturnsTrue()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<RadarrSettings>, Task<RadarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = true });
            _mocker.Setup<IExternalRepository<RadarrCache>, IQueryable<RadarrCache>>(x => x.GetAll())
                .Returns(new List<RadarrCache> { new RadarrCache { TheMovieDbId = 123, HasFile = true, HasRegular = true } }.AsQueryable().BuildMock());

            var result = await subject.TestShouldDeferToRadarr(123, false);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ShouldDeferToRadarr_4K_WhenInCacheWith4K_ReturnsTrue()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<RadarrSettings>, Task<RadarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = true });
            _mocker.Setup<IExternalRepository<RadarrCache>, IQueryable<RadarrCache>>(x => x.GetAll())
                .Returns(new List<RadarrCache> { new RadarrCache { TheMovieDbId = 123, HasFile = true, Has4K = true } }.AsQueryable().BuildMock());

            var result = await subject.TestShouldDeferToRadarr(123, true);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task ShouldDeferToSonarr_WhenSonarrDisabled_ReturnsFalse()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<SonarrSettings>, Task<SonarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings { Enabled = false, ScanForAvailability = true, PrioritizeArrAvailability = true });

            var result = await subject.TestShouldDeferToSonarr(456);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ShouldDeferToSonarr_WhenScanForAvailabilityDisabled_ReturnsFalse()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<SonarrSettings>, Task<SonarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings { Enabled = true, ScanForAvailability = false, PrioritizeArrAvailability = true });

            var result = await subject.TestShouldDeferToSonarr(456);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ShouldDeferToSonarr_WhenPrioritizeArrAvailabilityDisabled_ReturnsFalse()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<SonarrSettings>, Task<SonarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = false });

            var result = await subject.TestShouldDeferToSonarr(456);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ShouldDeferToSonarr_WhenNoEpisodesInCache_ReturnsFalse()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<SonarrSettings>, Task<SonarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = true });
            _mocker.Setup<IExternalRepository<SonarrEpisodeCache>, IQueryable<SonarrEpisodeCache>>(x => x.GetAll())
                .Returns(new List<SonarrEpisodeCache>().AsQueryable().BuildMock());

            var result = await subject.TestShouldDeferToSonarr(456);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ShouldDeferToSonarr_WhenEpisodesInCacheWithFile_ReturnsTrue()
        {
            var subject = CreateSubjectWithArrDependencies();
            _mocker.Setup<ISettingsService<SonarrSettings>, Task<SonarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = true });
            _mocker.Setup<IExternalRepository<SonarrEpisodeCache>, IQueryable<SonarrEpisodeCache>>(x => x.GetAll())
                .Returns(new List<SonarrEpisodeCache> { new SonarrEpisodeCache { TvDbId = 456, HasFile = true } }.AsQueryable().BuildMock());

            var result = await subject.TestShouldDeferToSonarr(456);

            Assert.That(result, Is.True);
        }

        private TestAvailabilityCheckerWithArrSupport CreateSubjectWithArrDependencies()
        {
            _mocker.Setup<ISettingsService<RadarrSettings>, Task<RadarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings());
            _mocker.Setup<ISettingsService<SonarrSettings>, Task<SonarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings());
            _mocker.Setup<IExternalRepository<RadarrCache>, IQueryable<RadarrCache>>(x => x.GetAll())
                .Returns(new List<RadarrCache>().AsQueryable().BuildMock());
            _mocker.Setup<IExternalRepository<SonarrEpisodeCache>, IQueryable<SonarrEpisodeCache>>(x => x.GetAll())
                .Returns(new List<SonarrEpisodeCache>().AsQueryable().BuildMock());

            return _mocker.CreateInstance<TestAvailabilityCheckerWithArrSupport>();
        }
    }


    public class TestAvailabilityChecker : AvailabilityChecker
    {
        public TestAvailabilityChecker(ITvRequestRepository tvRequest, INotificationHelper notification, ILogger log, INotificationHubService notificationHubService) : base(tvRequest, notification, log, notificationHubService, null, null, null, null)
        {
        }

        public new Task ProcessTvShow(IQueryable<IBaseMediaServerEpisode> seriesEpisodes, ChildRequests child) => base.ProcessTvShow(seriesEpisodes, child);
    }

    public class TestAvailabilityCheckerWithArrSupport : AvailabilityChecker
    {
        public TestAvailabilityCheckerWithArrSupport(
            ITvRequestRepository tvRequest,
            INotificationHelper notification,
            ILogger log,
            INotificationHubService notificationHubService,
            ISettingsService<RadarrSettings> radarrSettings,
            ISettingsService<SonarrSettings> sonarrSettings,
            IExternalRepository<RadarrCache> radarrCache,
            IExternalRepository<SonarrEpisodeCache> sonarrEpisodeCache)
            : base(tvRequest, notification, log, notificationHubService, radarrSettings, sonarrSettings, radarrCache, sonarrEpisodeCache)
        {
        }

        public Task<bool> TestShouldDeferToRadarr(int theMovieDbId, bool is4K) => base.ShouldDeferToRadarr(theMovieDbId, is4K);
        public Task<bool> TestShouldDeferToSonarr(int tvDbId) => base.ShouldDeferToSonarr(tvDbId);
    }
}
