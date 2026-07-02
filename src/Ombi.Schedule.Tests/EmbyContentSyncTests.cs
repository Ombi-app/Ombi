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
using Ombi.Api.External.MediaServers.Emby.Models.Movie;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Quartz;
using MediaType = Ombi.Store.Entities.MediaType;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class EmbyContentSyncTests
    {
        private AutoMocker _mocker;
        private EmbyContentSync _subject;
        private Mock<IEmbyApi> _api;
        private Mock<IJobExecutionContext> _context;
        private List<EmbyContent> _addedContent;
        private List<EmbyContent> _deletedContent;
        private List<EmbyEpisode> _deletedEpisodes;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();

            _api = new Mock<IEmbyApi>();
            _mocker.Setup<IEmbyApiFactory, IEmbyApi>(x => x.CreateClient(It.IsAny<EmbySettings>()))
                .Returns(_api.Object);

            // The sync passes live collections that it clears afterwards, so snapshot
            // their contents at call time rather than inspecting them at verify time
            _addedContent = new List<EmbyContent>();
            _mocker.GetMock<IEmbyContentRepository>()
                .Setup(x => x.AddRange(It.IsAny<IEnumerable<EmbyContent>>(), It.IsAny<bool>()))
                .Callback<IEnumerable<EmbyContent>, bool>((c, _) => _addedContent.AddRange(c))
                .Returns(Task.CompletedTask);

            _deletedContent = new List<EmbyContent>();
            _mocker.GetMock<IEmbyContentRepository>()
                .Setup(x => x.DeleteRange(It.IsAny<IEnumerable<EmbyContent>>()))
                .Callback<IEnumerable<EmbyContent>>(c => _deletedContent.AddRange(c))
                .Returns(Task.CompletedTask);

            _deletedEpisodes = new List<EmbyEpisode>();
            _mocker.GetMock<IEmbyContentRepository>()
                .Setup(x => x.DeleteEpisodes(It.IsAny<IEnumerable<EmbyEpisode>>()))
                .Callback<IEnumerable<EmbyEpisode>>(e => _deletedEpisodes.AddRange(e))
                .Returns(Task.CompletedTask);

            _mocker.Setup<IEmbyContentRepository, Task<EmbyContent>>(x => x.GetByEmbyId(It.IsAny<string>()))
                .ReturnsAsync((EmbyContent)null);
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyContent>>>(x => x.GetAllContentIdentifiers())
                .ReturnsAsync(new List<EmbyContent>());
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyEpisode>>>(x => x.GetAllEpisodeIdentifiers())
                .ReturnsAsync(new List<EmbyEpisode>());

            _mocker.Setup<ISettingsService<EmbySettings>, Task<EmbySettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new EmbySettings
                {
                    Enable = true,
                    Servers = new List<EmbyServers>
                    {
                        new EmbyServers
                        {
                            Name = "Test",
                            ApiKey = "key",
                            AdministratorId = "admin",
                            Ip = "localhost",
                            Port = 8096
                        }
                    }
                });

            _mocker.Setup<IFeatureService, Task<bool>>(x => x.FeatureEnabled(It.IsAny<string>()))
                .ReturnsAsync(false);

            _context = new Mock<IJobExecutionContext>();
            _context.Setup(x => x.MergedJobDataMap).Returns(new JobDataMap());

            var scheduler = new Mock<IScheduler>();
            scheduler.Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IJobExecutionContext>());
            _ = new QuartzMock(scheduler);

            _subject = _mocker.CreateInstance<EmbyContentSync>();
        }

        [Test]
        public async Task TvSync_EmptyPageWithRemainingRecords_StopsInsteadOfRefetchingForever()
        {
            SetupMovies(totalRecordCount: 0);
            SetupShows(totalRecordCount: 50);

            await _subject.Execute(_context.Object);

            _api.Verify(x => x.GetAllShows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Content Sync Finished", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task MovieSync_EmptyPageWithRemainingRecords_StopsInsteadOfRefetchingForever()
        {
            SetupMovies(totalRecordCount: 50);
            SetupShows(totalRecordCount: 0);

            await _subject.Execute(_context.Object);

            _api.Verify(x => x.GetAllMovies(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mocker.Verify<INotificationHubService>(x => x.SendNotificationToAdmins("Emby Content Sync Finished", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SeriesWithoutProviderIds_IsAddedWithEmbyIdOnly()
        {
            SetupMovies(totalRecordCount: 0);
            SetupShows(Show("series1", "No Metadata Show", providerIds: null));

            await _subject.Execute(_context.Object);

            var added = _addedContent.SingleOrDefault(x => x.EmbyId == "series1");
            Assert.That(added, Is.Not.Null);
            Assert.That(added.Title, Is.EqualTo("No Metadata Show"));
            Assert.That(added.TvDbId, Is.Null);
            Assert.That(added.TheMovieDbId, Is.Null);
            Assert.That(added.ImdbId, Is.Null);
        }

        [Test]
        public async Task SeriesThatLostItsProviderIds_DoesNotWipeTheExistingIds()
        {
            SetupMovies(totalRecordCount: 0);
            SetupShows(Show("series1", "Backfilled Show", providerIds: null));
            _mocker.Setup<IEmbyContentRepository, Task<EmbyContent>>(x => x.GetByEmbyId("series1"))
                .ReturnsAsync(new EmbyContent { Id = 1, EmbyId = "series1", TvDbId = "123", Type = MediaType.Series });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IEmbyContentRepository>(x => x.UpdateWithoutSave(It.IsAny<IMediaServerContent>()), Times.Never);
            Assert.That(_addedContent, Is.Empty);
        }

        [Test]
        public async Task FullSync_RemovesContentThatNoLongerExistsOnTheServer()
        {
            SetupMovies(totalRecordCount: 0);
            SetupShows(Show("series1", "Still Here", providerIds: new EmbyProviderids { Tvdb = "1" }));
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyContent>>>(x => x.GetAllContentIdentifiers())
                .ReturnsAsync(new List<EmbyContent>
                {
                    new EmbyContent { Id = 1, EmbyId = "series1", Type = MediaType.Series },
                    new EmbyContent { Id = 2, EmbyId = "goneSeries", Type = MediaType.Series },
                    new EmbyContent { Id = 3, EmbyId = "goneMovie", Type = MediaType.Movie }
                });
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyEpisode>>>(x => x.GetAllEpisodeIdentifiers())
                .ReturnsAsync(new List<EmbyEpisode>
                {
                    new EmbyEpisode { Id = 10, EmbyId = "ep1", EpisodeNumber = 1, ParentId = "goneSeries" },
                    new EmbyEpisode { Id = 11, EmbyId = "ep2", EpisodeNumber = 1, ParentId = "series1" }
                });

            await _subject.Execute(_context.Object);

            Assert.That(_deletedContent.Select(x => x.EmbyId), Is.EquivalentTo(new[] { "goneSeries", "goneMovie" }));
            Assert.That(_deletedEpisodes.Select(x => x.Id), Is.EquivalentTo(new[] { 10 }));
        }

        [Test]
        public async Task RecentlyAddedSync_DoesNotRemoveContent()
        {
            _context.Setup(x => x.MergedJobDataMap).Returns(new JobDataMap(new Dictionary<string, string>
            {
                { JobDataKeys.EmbyRecentlyAddedSearch, "true" }
            }));
            _api.Setup(x => x.RecentlyAddedMovies(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbyMovie> { TotalRecordCount = 0, Items = new List<EmbyMovie>() });
            _api.Setup(x => x.RecentlyAddedShows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbySeries>
                {
                    TotalRecordCount = 1,
                    Items = new List<EmbySeries> { Show("series1", "Still Here", providerIds: new EmbyProviderids { Tvdb = "1" }) }
                });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IEmbyContentRepository>(x => x.GetAllContentIdentifiers(), Times.Never);
            _mocker.Verify<IEmbyContentRepository>(x => x.DeleteRange(It.IsAny<IEnumerable<EmbyContent>>()), Times.Never);
        }

        [Test]
        public async Task FailedServer_SkipsStaleContentCleanup()
        {
            SetupMovies(totalRecordCount: 0);
            _api.Setup(x => x.GetAllShows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Emby died"));
            _mocker.Setup<IEmbyContentRepository, Task<List<EmbyContent>>>(x => x.GetAllContentIdentifiers())
                .ReturnsAsync(new List<EmbyContent>
                {
                    new EmbyContent { Id = 1, EmbyId = "wouldOtherwiseBeRemoved", Type = MediaType.Series }
                });

            await _subject.Execute(_context.Object);

            _mocker.Verify<IEmbyContentRepository>(x => x.DeleteRange(It.IsAny<IEnumerable<EmbyContent>>()), Times.Never);
        }

        private void SetupShows(params EmbySeries[] shows)
        {
            _api.Setup(x => x.GetAllShows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbySeries>
                {
                    TotalRecordCount = shows.Length,
                    Items = shows.ToList()
                });
        }

        private static EmbySeries Show(string id, string name, EmbyProviderids providerIds) => new EmbySeries
        {
            Id = id,
            Name = name,
            ProviderIds = providerIds
        };

        private void SetupMovies(int totalRecordCount)
        {
            _api.Setup(x => x.GetAllMovies(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbyMovie>
                {
                    TotalRecordCount = totalRecordCount,
                    Items = new List<EmbyMovie>()
                });
        }

        private void SetupShows(int totalRecordCount)
        {
            _api.Setup(x => x.GetAllShows(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyItemContainer<EmbySeries>
                {
                    TotalRecordCount = totalRecordCount,
                    Items = new List<EmbySeries>()
                });
        }
    }
}
