using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Emby;
using Ombi.Api.External.MediaServers.Emby.Models;
using Ombi.Api.External.MediaServers.Emby.Models.Media.Tv;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class EmbyEpisodeSyncTests
    {
        private AutoMocker _mocker;
        private EmbyEpisodeSync _subject;
        private Mock<IEmbyApi> _api;
        private Mock<IJobExecutionContext> _context;
        private List<EmbyEpisode> _addedEpisodes;
        private List<EmbyEpisode> _deletedEpisodes;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();

            _api = new Mock<IEmbyApi>();
            _mocker.Setup<IEmbyApiFactory, IEmbyApi>(x => x.CreateClient(It.IsAny<EmbySettings>()))
                .Returns(_api.Object);

            SetupServers(Server("server1"));

            // AddRange is called with a live HashSet that the sync clears afterwards,
            // so snapshot its contents at call time rather than inspecting it at verify time
            _addedEpisodes = new List<EmbyEpisode>();
            _mocker.GetMock<IEmbyContentRepository>()
                .Setup(x => x.AddRange(It.IsAny<IEnumerable<IMediaServerEpisode>>()))
                .Callback<IEnumerable<IMediaServerEpisode>>(e => _addedEpisodes.AddRange(e.Cast<EmbyEpisode>()))
                .Returns(Task.CompletedTask);

            _deletedEpisodes = new List<EmbyEpisode>();
            _mocker.GetMock<IEmbyContentRepository>()
                .Setup(x => x.DeleteEpisodes(It.IsAny<IEnumerable<EmbyEpisode>>()))
                .Callback<IEnumerable<EmbyEpisode>>(e => _deletedEpisodes.AddRange(e))
                .Returns(Task.CompletedTask);

            _mocker.Setup<IEmbyContentRepository, Task<HashSet<string>>>(x => x.GetAllSeriesEmbyIds())
                .ReturnsAsync(new HashSet<string> { "series1" });
            _mocker.Setup<IEmbyContentRepository, Task<Dictionary<string, (int EpisodeNumber, int SeasonNumber)>>>(x => x.GetAllEpisodeMetadata())
                .ReturnsAsync(new Dictionary<string, (int EpisodeNumber, int SeasonNumber)>());
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyEpisode>>>(x => x.GetAllEpisodeIdentifiers())
                .ReturnsAsync(new List<EmbyEpisode>());

            _context = new Mock<IJobExecutionContext>();
            _context.Setup(x => x.MergedJobDataMap).Returns(new JobDataMap());

            var scheduler = new Mock<IScheduler>();
            scheduler.Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IJobExecutionContext>());
            _ = new QuartzMock(scheduler);

            _subject = _mocker.CreateInstance<EmbyEpisodeSync>();
        }

        [Test]
        public async Task EpisodeWithNullProviderIds_IsStillAdded()
        {
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Page(Episode("ep1")));

            await _subject.Execute(_context.Object);

            Assert.That(_addedEpisodes, Has.Count.EqualTo(1));
            Assert.That(_addedEpisodes.Single().EmbyId, Is.EqualTo("ep1"));
            Assert.That(_addedEpisodes.Single().TvDbId, Is.Null);
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Episode Sync Failed", It.IsAny<CancellationToken>()), Times.Never);
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Episode Sync Finished", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task EmptyPageWithRemainingRecords_StopsInsteadOfRefetchingForever()
        {
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbyEpisodes> { TotalRecordCount = 100, Items = new List<EmbyEpisodes>() });

            await _subject.Execute(_context.Object);

            _api.Verify(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Episode Sync Finished", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ServerThrowing_SendsFailureNotification_AndDoesNotThrow()
        {
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Emby returned something we cannot handle"));

            await _subject.Execute(_context.Object);

            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Episode Sync Failed", It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Episode Sync Finished", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task OneServerFailing_DoesNotStopTheOtherServers()
        {
            SetupServers(Server("badServer"), Server("goodServer"));
            _api.Setup(x => x.GetAllEpisodes("badServer", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Emby returned something we cannot handle"));
            _api.Setup(x => x.GetAllEpisodes("goodServer", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Page(Episode("ep1")));

            await _subject.Execute(_context.Object);

            _api.Verify(x => x.GetAllEpisodes("goodServer", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.That(_addedEpisodes.Select(x => x.EmbyId), Does.Contain("ep1"));
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Episode Sync Failed", It.IsAny<CancellationToken>()), Times.Once);
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Episode Sync Finished", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task FullSync_RemovesEpisodesThatNoLongerExistOnTheServer()
        {
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Page(Episode("ep1")));
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyEpisode>>>(x => x.GetAllEpisodeIdentifiers())
                .ReturnsAsync(new List<EmbyEpisode>
                {
                    new EmbyEpisode { Id = 1, EmbyId = "ep1", EpisodeNumber = 1, ParentId = "series1" },
                    new EmbyEpisode { Id = 2, EmbyId = "goneEpisode", EpisodeNumber = 1, ParentId = "series1" }
                });

            await _subject.Execute(_context.Object);

            Assert.That(_deletedEpisodes.Select(x => x.EmbyId), Is.EquivalentTo(new[] { "goneEpisode" }));
        }

        [Test]
        public async Task FullSync_RemovesEpisodeRowsThatMovedToAnotherSeries()
        {
            // The API reports ep1 under series1, but the database row still points at
            // a previous series - the stale row must be cleaned up
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Page(Episode("ep1")));
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyEpisode>>>(x => x.GetAllEpisodeIdentifiers())
                .ReturnsAsync(new List<EmbyEpisode>
                {
                    new EmbyEpisode { Id = 1, EmbyId = "ep1", EpisodeNumber = 1, ParentId = "previousSeries" }
                });

            await _subject.Execute(_context.Object);

            Assert.That(_deletedEpisodes.Select(x => x.Id), Is.EquivalentTo(new[] { 1 }));
        }

        [Test]
        public async Task FullSync_KeepsRowsForMultiEpisodeFiles()
        {
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Page(Episode("ep1", episodeNumber: 1, seasonNumber: 1, indexNumberEnd: 3)));
            _mocker.Setup<IEmbyContentRepository, Task<Dictionary<string, (int EpisodeNumber, int SeasonNumber)>>>(x => x.GetAllEpisodeMetadata())
                .ReturnsAsync(new Dictionary<string, (int EpisodeNumber, int SeasonNumber)>
                {
                    ["ep1:1"] = (1, 1),
                    ["ep1:2"] = (2, 1),
                    ["ep1:3"] = (3, 1)
                });
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyEpisode>>>(x => x.GetAllEpisodeIdentifiers())
                .ReturnsAsync(new List<EmbyEpisode>
                {
                    new EmbyEpisode { Id = 1, EmbyId = "ep1", EpisodeNumber = 1, ParentId = "series1" },
                    new EmbyEpisode { Id = 2, EmbyId = "ep1", EpisodeNumber = 2, ParentId = "series1" },
                    new EmbyEpisode { Id = 3, EmbyId = "ep1", EpisodeNumber = 3, ParentId = "series1" }
                });

            await _subject.Execute(_context.Object);

            Assert.That(_deletedEpisodes, Is.Empty);
        }

        [Test]
        public async Task NegativeIndexNumberWithHugeIndexNumberEnd_DoesNotOverflowTheFillCap()
        {
            // int subtraction would wrap negative here and bypass the 50-episode cap
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Page(Episode("ep1", episodeNumber: -2100000000, seasonNumber: 1, indexNumberEnd: 2100000000)));

            await _subject.Execute(_context.Object);

            Assert.That(_addedEpisodes, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task RecentlyAddedSync_DoesNotRemoveEpisodes()
        {
            _context.Setup(x => x.MergedJobDataMap).Returns(new JobDataMap(new Dictionary<string, string>
            {
                { JobDataKeys.EmbyRecentlyAddedSearch, "true" }
            }));
            _api.Setup(x => x.RecentlyAddedEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Page(Episode("ep1")));

            await _subject.Execute(_context.Object);

            _mocker.Verify<IEmbyContentRepository>(x => x.GetAllEpisodeIdentifiers(), Times.Never);
            _mocker.Verify<IEmbyContentRepository>(x => x.DeleteEpisodes(It.IsAny<IEnumerable<EmbyEpisode>>()), Times.Never);
        }

        [Test]
        public async Task FailedServer_SkipsStaleEpisodeCleanup()
        {
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Emby died"));
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyEpisode>>>(x => x.GetAllEpisodeIdentifiers())
                .ReturnsAsync(new List<EmbyEpisode>
                {
                    new EmbyEpisode { Id = 1, EmbyId = "wouldOtherwiseBeRemoved", EpisodeNumber = 1 }
                });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IEmbyContentRepository>(x => x.DeleteEpisodes(It.IsAny<IEnumerable<EmbyEpisode>>()), Times.Never);
        }

        [Test]
        public async Task EmptyPageWithRemainingRecords_SkipsStaleEpisodeCleanup()
        {
            _api.Setup(x => x.GetAllEpisodes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbyEpisodes> { TotalRecordCount = 100, Items = new List<EmbyEpisodes>() });
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyEpisode>>>(x => x.GetAllEpisodeIdentifiers())
                .ReturnsAsync(new List<EmbyEpisode>
                {
                    new EmbyEpisode { Id = 1, EmbyId = "wouldOtherwiseBeRemoved", EpisodeNumber = 1 }
                });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IEmbyContentRepository>(x => x.DeleteEpisodes(It.IsAny<IEnumerable<EmbyEpisode>>()), Times.Never);
        }

        private void SetupServers(params EmbyServers[] servers)
        {
            _mocker.Setup<ISettingsService<EmbySettings>, Task<EmbySettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new EmbySettings
                {
                    Enable = true,
                    Servers = servers.ToList()
                });
        }

        private static EmbyServers Server(string apiKey) => new EmbyServers
        {
            Name = apiKey,
            ApiKey = apiKey,
            AdministratorId = "admin",
            Ip = "localhost",
            Port = 8096
        };

        private static EmbyItemContainer<EmbyEpisodes> Page(params EmbyEpisodes[] episodes) => new EmbyItemContainer<EmbyEpisodes>
        {
            TotalRecordCount = episodes.Length,
            Items = episodes.ToList()
        };

        private static EmbyEpisodes Episode(string id, int episodeNumber = 1, int seasonNumber = 1, int? indexNumberEnd = null) => new EmbyEpisodes
        {
            Id = id,
            Name = $"Episode {id}",
            SeriesId = "series1",
            SeriesName = "Series",
            IndexNumber = episodeNumber,
            ParentIndexNumber = seasonNumber,
            IndexNumberEnd = indexNumberEnd,
            ProviderIds = null
        };
    }
}
