using System.Collections.Generic;
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
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Emby;
using Quartz;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class EmbyContentSyncTests
    {
        private AutoMocker _mocker;
        private EmbyContentSync _subject;
        private Mock<IEmbyApi> _api;
        private Mock<IJobExecutionContext> _context;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();

            _api = new Mock<IEmbyApi>();
            _mocker.Setup<IEmbyApiFactory, IEmbyApi>(x => x.CreateClient(It.IsAny<EmbySettings>()))
                .Returns(_api.Object);

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
