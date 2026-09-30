using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.Sonarr;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Core;
using Ombi.Core.Settings;
using Ombi.Core.Senders;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Senders
{
    [TestFixture]
    public class TvSenderTests
    {
        private AutoMocker _mocker;
        private TvSender _subject;
        private Mock<ISonarrV3Api> _sonarr;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<TvSender>();
            _sonarr = _mocker.GetMock<ISonarrV3Api>();

            _mocker.GetMock<IRepository<UserQualityProfiles>>()
                .Setup(x => x.GetAll())
                .Returns(new List<UserQualityProfiles>().AsQueryable().BuildMock());
        }

        [TestCase(2, false)]
        [TestCase(1, true)]
        public async Task ExistingSeries_ProfileOverride_SearchesRequestedSeasonOnly(
            int existingProfileId,
            bool expectProfileUpdate)
        {
            var settings = new SonarrSettings
            {
                ApiKey = "key",
                Ip = "localhost",
                Port = 8989,
                RootPath = "1",
                QualityProfile = "1",
                AddOnly = false
            };

            var request = new ChildRequests
            {
                RequestedUserId = "user",
                SeriesType = SeriesType.Standard,
                QualityOverride = 2,
                ParentRequest = new TvRequests
                {
                    Title = "Test Show",
                    TvDbId = 123,
                    TotalSeasons = 1
                },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                EpisodeNumber = 1,
                                Title = "Pilot"
                            }
                        }
                    }
                }
            };

            var series = new SonarrSeries
            {
                id = 10,
                tvdbId = 123,
                qualityProfileId = existingProfileId,
                monitored = true,
                seasons = new[]
                {
                    new Season
                    {
                        seasonNumber = 1,
                        monitored = true
                    }
                }
            };

            _sonarr.Setup(x => x.GetRootFolders(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new SonarrRootFolder
                    {
                        id = 1,
                        path = "/tv"
                    }
                });
            _sonarr.Setup(x => x.GetSeries(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[] { series });
            _sonarr.Setup(x => x.UpdateSeries(It.IsAny<SonarrSeries>(), settings.ApiKey, settings.FullUri))
                .ReturnsAsync((SonarrSeries updated, string _, string __) => updated);
            _sonarr.Setup(x => x.GetEpisodes(series.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new Episode
                    {
                        id = 100,
                        seriesId = series.id,
                        seasonNumber = 1,
                        episodeNumber = 1,
                        title = "Pilot",
                        monitored = true
                    }
                });
            _sonarr.Setup(x => x.SeasonSearch(series.id, 1, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(true);

            await _subject.SendToSonarr(request, settings);

            _sonarr.Verify(
                x => x.UpdateSeries(
                    It.Is<SonarrSeries>(updated => updated.qualityProfileId == 2),
                    settings.ApiKey,
                    settings.FullUri),
                expectProfileUpdate ? Times.Once() : Times.Never());
            _sonarr.Verify(
                x => x.SeasonSearch(series.id, 1, settings.ApiKey, settings.FullUri),
                Times.Once);
            _sonarr.Verify(
                x => x.SeriesSearch(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }



        [Test]
        public async Task Send_UnsafeSeasonMapping_QueuesManualInterventionWithoutRetryNotification()
        {
            var settings = CreateSettings();
            settings.Enabled = true;
            var request = CreateSingleEpisodeRequest();
            request.Id = 77;
            var existingSeries = CreateSeries(id: 42, tvdbId: request.ParentRequest.TvDbId);

            _mocker.GetMock<ISettingsService<SonarrSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(settings);
            SetupRootFolder(settings);
            _sonarr.Setup(x => x.GetSeries(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[] { existingSeries });
            _sonarr.Setup(x => x.GetEpisodes(existingSeries.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new Episode
                    {
                        id = 100,
                        seriesId = existingSeries.id,
                        seasonNumber = 1,
                        episodeNumber = 1,
                        title = "A Different Episode Title",
                        monitored = false
                    }
                });

            RequestQueue queued = null;
            var queueRepository = _mocker.GetMock<IRepository<RequestQueue>>();
            queueRepository
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RequestQueue, bool>>>()))
                .ReturnsAsync((RequestQueue)null);
            queueRepository
                .Setup(x => x.Add(It.IsAny<RequestQueue>()))
                .Callback<RequestQueue>(x => queued = x)
                .ReturnsAsync((RequestQueue x) => x);

            var result = await _subject.Send(request);

            Assert.That(result.Success, Is.False);
            Assert.That(queued, Is.Not.Null);
            Assert.That(queued.Error, Does.StartWith(TvSender.ManualInterventionQueuePrefix));
            Assert.That(queued.Error, Does.Contain("Unable to safely map requested season 1"));
            _mocker.GetMock<INotificationHelper>()
                .Verify(x => x.Notify(request, NotificationType.ItemAddedToFaultQueue), Times.Never);
        }

        [Test]
        public void NewlyAddedSeries_ConfigurationFailure_RollsBackWithoutDeletingFiles()
        {
            var settings = CreateSettings();
            var request = CreateSingleEpisodeRequest();
            var createdSeries = CreateSeries(id: 42, tvdbId: request.ParentRequest.TvDbId);

            SetupNewSeriesPath(settings, createdSeries);
            _sonarr.Setup(x => x.GetEpisodes(createdSeries.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new Episode
                    {
                        id = 100,
                        seriesId = createdSeries.id,
                        seasonNumber = 1,
                        episodeNumber = 1,
                        title = "A Different Episode Title",
                        monitored = false
                    }
                });
            _sonarr.Setup(x => x.DeleteSeries(
                    createdSeries.id,
                    settings.ApiKey,
                    settings.FullUri,
                    false,
                    false))
                .ReturnsAsync(true);

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _subject.SendToSonarr(request, settings));

            StringAssert.Contains("Unable to safely map requested season 1", exception.Message);
            _sonarr.Verify(x => x.DeleteSeries(
                    createdSeries.id,
                    settings.ApiKey,
                    settings.FullUri,
                    false,
                    false),
                Times.Once);
        }

        [Test]
        public void ExistingSeries_ConfigurationFailure_DoesNotRollback()
        {
            var settings = CreateSettings();
            var request = CreateSingleEpisodeRequest();
            var existingSeries = CreateSeries(id: 42, tvdbId: request.ParentRequest.TvDbId);

            SetupRootFolder(settings);
            _sonarr.Setup(x => x.GetSeries(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[] { existingSeries });
            _sonarr.Setup(x => x.GetEpisodes(existingSeries.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new Episode
                    {
                        id = 100,
                        seriesId = existingSeries.id,
                        seasonNumber = 1,
                        episodeNumber = 1,
                        title = "A Different Episode Title",
                        monitored = false
                    }
                });

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _subject.SendToSonarr(request, settings));

            StringAssert.Contains("Unable to safely map requested season 1", exception.Message);
            _sonarr.Verify(x => x.DeleteSeries(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                Times.Never);
        }

        [Test]
        public void NewlyAddedSeries_RollbackFailure_PreservesOriginalConfigurationException()
        {
            var settings = CreateSettings();
            var request = CreateSingleEpisodeRequest();
            var createdSeries = CreateSeries(id: 42, tvdbId: request.ParentRequest.TvDbId);

            SetupNewSeriesPath(settings, createdSeries);
            _sonarr.Setup(x => x.GetEpisodes(createdSeries.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new Episode
                    {
                        id = 100,
                        seriesId = createdSeries.id,
                        seasonNumber = 1,
                        episodeNumber = 1,
                        title = "A Different Episode Title",
                        monitored = false
                    }
                });
            _sonarr.Setup(x => x.DeleteSeries(
                    createdSeries.id,
                    settings.ApiKey,
                    settings.FullUri,
                    false,
                    false))
                .ThrowsAsync(new System.Net.Http.HttpRequestException("rollback failed"));

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _subject.SendToSonarr(request, settings));

            StringAssert.Contains("Unable to safely map requested season 1", exception.Message);
            StringAssert.DoesNotContain("rollback failed", exception.Message);
        }

        private void SetupNewSeriesPath(SonarrSettings settings, SonarrSeries createdSeries)
        {
            SetupRootFolder(settings);
            _sonarr.Setup(x => x.GetSeries(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(Enumerable.Empty<SonarrSeries>());
            _sonarr.Setup(x => x.AddSeries(It.IsAny<NewSeries>(), settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new NewSeries { id = createdSeries.id });
            _sonarr.Setup(x => x.GetSeriesById(createdSeries.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(createdSeries);
        }

        private void SetupRootFolder(SonarrSettings settings)
        {
            _sonarr.Setup(x => x.GetRootFolders(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new SonarrRootFolder
                    {
                        id = 1,
                        path = "/tv"
                    }
                });
        }

        private static SonarrSettings CreateSettings()
        {
            return new SonarrSettings
            {
                ApiKey = "key",
                Ip = "localhost",
                Port = 8989,
                RootPath = "1",
                QualityProfile = "1",
                AddOnly = false
            };
        }

        private static ChildRequests CreateSingleEpisodeRequest()
        {
            return new ChildRequests
            {
                RequestedUserId = "user",
                SeriesType = SeriesType.Standard,
                ParentRequest = new TvRequests
                {
                    Title = "Test Show",
                    TvDbId = 123,
                    TotalSeasons = 1
                },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                EpisodeNumber = 1,
                                Title = "Pilot"
                            }
                        }
                    }
                }
            };
        }

        private static SonarrSeries CreateSeries(int id, int tvdbId)
        {
            return new SonarrSeries
            {
                id = id,
                tvdbId = tvdbId,
                monitored = true,
                seasons = new[]
                {
                    new Season
                    {
                        seasonNumber = 1,
                        monitored = false
                    }
                }
            };
        }
    }
}
