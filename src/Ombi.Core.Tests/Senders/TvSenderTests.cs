using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.Sonarr;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
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
        public async Task Send_UnsafeSeasonMapping_ReopensCompletedQueueEntry()
        {
            var settings = CreateSettings();
            settings.Enabled = true;
            var request = CreateSingleEpisodeRequest();
            request.Id = 78;
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

            var oldDts = System.DateTime.UtcNow.AddDays(-2);
            var queueItem = new RequestQueue
            {
                Id = 9,
                RequestId = request.Id,
                Type = RequestType.TvShow,
                Dts = oldDts,
                Error = "previous completed failure",
                Completed = System.DateTime.UtcNow.AddDays(-1),
                RetryCount = 7
            };
            var queueRepository = _mocker.GetMock<IRepository<RequestQueue>>();
            queueRepository
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RequestQueue, bool>>>()))
                .ReturnsAsync(queueItem);

            var result = await _subject.Send(request);

            Assert.That(result.Success, Is.False);
            Assert.That(queueItem.Completed, Is.Null);
            Assert.That(queueItem.RetryCount, Is.Zero);
            Assert.That(queueItem.Dts, Is.GreaterThan(oldDts));
            Assert.That(queueItem.Error, Does.StartWith(TvSender.ManualInterventionQueuePrefix));
            queueRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
            queueRepository.Verify(x => x.Add(It.IsAny<RequestQueue>()), Times.Never);
        }

        [Test]
        public async Task Send_UnsafeSeasonMapping_ActiveQueueEntryKeepsRetryHistory()
        {
            var settings = CreateSettings();
            settings.Enabled = true;
            var request = CreateSingleEpisodeRequest();
            request.Id = 79;
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

            var originalDts = System.DateTime.UtcNow.AddHours(-3);
            var queueItem = new RequestQueue
            {
                Id = 10,
                RequestId = request.Id,
                Type = RequestType.TvShow,
                Dts = originalDts,
                Error = "existing active failure",
                Completed = null,
                RetryCount = 2
            };
            var queueRepository = _mocker.GetMock<IRepository<RequestQueue>>();
            queueRepository
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RequestQueue, bool>>>()))
                .ReturnsAsync(queueItem);

            await _subject.Send(request);

            Assert.That(queueItem.Completed, Is.Null);
            Assert.That(queueItem.RetryCount, Is.EqualTo(3));
            Assert.That(queueItem.Dts, Is.EqualTo(originalDts));
            Assert.That(queueItem.Error, Does.StartWith(TvSender.ManualInterventionQueuePrefix));
        }

        [Test]
        public async Task Send_TransientFailure_ReopensCompletedQueueEntryForAutomaticRetry()
        {
            var request = CreateSingleEpisodeRequest();
            request.Id = 80;
            _mocker.GetMock<ISettingsService<SonarrSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ThrowsAsync(new System.InvalidOperationException("temporary send failure"));

            var oldDts = System.DateTime.UtcNow.AddDays(-2);
            var queueItem = new RequestQueue
            {
                Id = 11,
                RequestId = request.Id,
                Type = RequestType.TvShow,
                Dts = oldDts,
                Error = "previous completed failure",
                Completed = System.DateTime.UtcNow.AddDays(-1),
                RetryCount = 4
            };
            var queueRepository = _mocker.GetMock<IRepository<RequestQueue>>();
            queueRepository
                .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RequestQueue, bool>>>()))
                .ReturnsAsync(queueItem);

            var result = await _subject.Send(request);

            Assert.That(result.Success, Is.False);
            Assert.That(queueItem.Completed, Is.Null);
            Assert.That(queueItem.RetryCount, Is.Zero);
            Assert.That(queueItem.Dts, Is.GreaterThan(oldDts));
            Assert.That(queueItem.Error, Is.EqualTo("temporary send failure"));
            Assert.That(queueItem.Error, Does.Not.StartWith(TvSender.ManualInterventionQueuePrefix));
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

            var exception = Assert.CatchAsync<System.InvalidOperationException>(
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

            var exception = Assert.CatchAsync<System.InvalidOperationException>(
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

            var exception = Assert.CatchAsync<System.InvalidOperationException>(
                async () => await _subject.SendToSonarr(request, settings));

            StringAssert.Contains("Unable to safely map requested season 1", exception.Message);
            StringAssert.DoesNotContain("rollback failed", exception.Message);
        }

        [Test]
        public async Task MissingTvDb_StandaloneSeries_RepairsConsolidatedParentFromEpisodeFingerprint()
        {
            var settings = CreateSettings();
            var request = new ChildRequests
            {
                Id = 8616,
                RequestedUserId = "user",
                SeriesType = SeriesType.Standard,
                ParentRequest = new TvRequests
                {
                    Title = "Total Drama Action",
                    ExternalProviderId = 8616,
                    TvDbId = 0,
                    TotalSeasons = 1
                },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests { EpisodeNumber = 1, Title = "Monster Cash!" },
                            new EpisodeRequests { EpisodeNumber = 2, Title = "Alien Resurr-eggtion" },
                            new EpisodeRequests { EpisodeNumber = 3, Title = "Riot on Set" },
                            new EpisodeRequests { EpisodeNumber = 4, Title = "Beach Blanket Bogus" },
                            new EpisodeRequests { EpisodeNumber = 5, Title = "TDA Aftermath: I" }
                        }
                    }
                }
            };
            var existingSeries = new SonarrSeries
            {
                id = 620,
                title = "Total Drama",
                tvdbId = 83294,
                tmdbId = 0,
                monitored = true,
                seasons = new[]
                {
                    new Season { seasonNumber = 2, monitored = true }
                }
            };
            var episodes = new[]
            {
                new Episode { id = 201, seriesId = 620, seasonNumber = 2, episodeNumber = 1, title = "Monster Cash", monitored = true },
                new Episode { id = 202, seriesId = 620, seasonNumber = 2, episodeNumber = 2, title = "Alien Resurr-eggtion", monitored = true },
                new Episode { id = 203, seriesId = 620, seasonNumber = 2, episodeNumber = 3, title = "Riot on Set", monitored = true },
                new Episode { id = 204, seriesId = 620, seasonNumber = 2, episodeNumber = 4, title = "Beach Blanket Bogus", monitored = true },
                new Episode { id = 205, seriesId = 620, seasonNumber = 2, episodeNumber = 5, title = "TDA Aftermath I: Trent's Descent", monitored = true }
            };

            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.GetTvExternals(request.ParentRequest.ExternalProviderId))
                .ReturnsAsync(new TvExternals());
            _mocker.GetMock<ITvRequestRepository>()
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);
            SetupRootFolder(settings);
            _sonarr.Setup(x => x.GetSeries(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[] { existingSeries });
            _sonarr.Setup(x => x.GetEpisodes(existingSeries.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(episodes);
            _sonarr.Setup(x => x.SeasonSearch(existingSeries.id, 2, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(true);

            var result = await _subject.SendToSonarr(request, settings);

            Assert.That(result, Is.Not.Null);
            Assert.That(request.ParentRequest.TvDbId, Is.EqualTo(83294));
            _mocker.GetMock<ITvRequestRepository>().Verify(x => x.Save(), Times.Once);
            _sonarr.Verify(x => x.SeasonSearch(existingSeries.id, 2, settings.ApiKey, settings.FullUri), Times.Once);
            _sonarr.Verify(x => x.SeasonSearch(existingSeries.id, 1, settings.ApiKey, settings.FullUri), Times.Never);
        }

        [Test]
        public async Task MissingTvDb_WorldTour_RepairsConsolidatedParentFromAirDateFingerprint()
        {
            var settings = CreateSettings();
            var request = new ChildRequests
            {
                Id = 19123,
                RequestedUserId = "user",
                SeriesType = SeriesType.Standard,
                ParentRequest = new TvRequests
                {
                    Title = "Total Drama World Tour",
                    ExternalProviderId = 19123,
                    TvDbId = 0,
                    TotalSeasons = 1
                },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests { EpisodeNumber = 1, Title = "Walk Like An Egyptian, Part 1", AirDate = new System.DateTime(2010, 6, 10) },
                            new EpisodeRequests { EpisodeNumber = 2, Title = "Walk Like An Egyptian, Part 2", AirDate = new System.DateTime(2010, 9, 9) },
                            new EpisodeRequests { EpisodeNumber = 3, Title = "Super Crazy Happy Fun Time Japan", AirDate = new System.DateTime(2010, 9, 16) },
                            new EpisodeRequests { EpisodeNumber = 4, Title = "Anything Yukon Do, I Can Do Better", AirDate = new System.DateTime(2010, 9, 23) },
                            new EpisodeRequests { EpisodeNumber = 5, Title = "Broadway, Baby!", AirDate = new System.DateTime(2010, 9, 30) },
                            new EpisodeRequests { EpisodeNumber = 6, Title = "Aftermath: Bridgette Over Troubled Water", AirDate = new System.DateTime(2010, 10, 7) }
                        }
                    }
                }
            };
            var existingSeries = new SonarrSeries
            {
                id = 620,
                title = "Total Drama",
                tvdbId = 83294,
                tmdbId = 0,
                monitored = true,
                seasons = new[]
                {
                    new Season { seasonNumber = 3, monitored = true }
                }
            };
            var episodes = new[]
            {
                new Episode { id = 301, seriesId = 620, seasonNumber = 3, episodeNumber = 1, title = "Walk Like an Egyptian: Part 1", airDate = "2010-06-10", monitored = true },
                new Episode { id = 302, seriesId = 620, seasonNumber = 3, episodeNumber = 2, title = "Walk Like an Egyptian: Part 2", airDate = "2010-09-09", monitored = true },
                new Episode { id = 303, seriesId = 620, seasonNumber = 3, episodeNumber = 3, title = "Super Crazy Happy Fun Time in Japan", airDate = "2010-09-16", monitored = true },
                new Episode { id = 304, seriesId = 620, seasonNumber = 3, episodeNumber = 4, title = "Anything Yukon Do, I Can Do Better", airDate = "2010-09-23", monitored = true },
                new Episode { id = 305, seriesId = 620, seasonNumber = 3, episodeNumber = 5, title = "Broadway, Baby!", airDate = "2010-09-30", monitored = true },
                new Episode { id = 306, seriesId = 620, seasonNumber = 3, episodeNumber = 6, title = "TDWT Aftermath I: Bridgette Over Troubled Waters", airDate = "2010-10-07", monitored = true }
            };

            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.GetTvExternals(request.ParentRequest.ExternalProviderId))
                .ReturnsAsync(new TvExternals());
            _mocker.GetMock<ITvRequestRepository>()
                .Setup(x => x.Save())
                .Returns(Task.CompletedTask);
            SetupRootFolder(settings);
            _sonarr.Setup(x => x.GetSeries(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[] { existingSeries });
            _sonarr.Setup(x => x.GetEpisodes(existingSeries.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(episodes);
            _sonarr.Setup(x => x.SeasonSearch(existingSeries.id, 3, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(true);

            var result = await _subject.SendToSonarr(request, settings);

            Assert.That(result, Is.Not.Null);
            Assert.That(request.ParentRequest.TvDbId, Is.EqualTo(83294));
            _sonarr.Verify(x => x.SeasonSearch(existingSeries.id, 3, settings.ApiKey, settings.FullUri), Times.Once);
            _sonarr.Verify(x => x.SeasonSearch(existingSeries.id, 1, settings.ApiKey, settings.FullUri), Times.Never);
        }

        [Test]
        public void MissingTvDb_ShiftedEpisodeSubset_DoesNotRepairConsolidatedParent()
        {
            var settings = CreateSettings();
            var request = new ChildRequests
            {
                Id = 296168,
                RequestedUserId = "user",
                SeriesType = SeriesType.Standard,
                ParentRequest = new TvRequests
                {
                    Title = "Total Drama Pahkitew Island",
                    ExternalProviderId = 296168,
                    TvDbId = 0,
                    TotalSeasons = 1
                },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests { EpisodeNumber = 1, Title = "So, Uh, This Is My Team?" },
                            new EpisodeRequests { EpisodeNumber = 2, Title = "I Love You, Grease Pig!" },
                            new EpisodeRequests { EpisodeNumber = 3, Title = "Twinning Isn't Everything" },
                            new EpisodeRequests { EpisodeNumber = 4, Title = "I Love You, I Love You Knots" }
                        }
                    }
                }
            };
            var existingSeries = new SonarrSeries
            {
                id = 620,
                title = "Total Drama",
                tvdbId = 83294,
                tmdbId = 0
            };
            var episodes = new[]
            {
                new Episode { id = 501, seriesId = 620, seasonNumber = 5, episodeNumber = 1, title = "All Stars One" },
                new Episode { id = 502, seriesId = 620, seasonNumber = 5, episodeNumber = 2, title = "All Stars Two" },
                new Episode { id = 503, seriesId = 620, seasonNumber = 5, episodeNumber = 3, title = "All Stars Three" },
                new Episode { id = 504, seriesId = 620, seasonNumber = 5, episodeNumber = 4, title = "All Stars Four" },
                new Episode { id = 505, seriesId = 620, seasonNumber = 5, episodeNumber = 5, title = "So, Uh, This Is My Team?" },
                new Episode { id = 506, seriesId = 620, seasonNumber = 5, episodeNumber = 6, title = "I Love You, Grease Pig!" },
                new Episode { id = 507, seriesId = 620, seasonNumber = 5, episodeNumber = 7, title = "Twinning Isn't Everything" },
                new Episode { id = 508, seriesId = 620, seasonNumber = 5, episodeNumber = 8, title = "I Love You, I Love You Knots" }
            };

            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.GetTvExternals(request.ParentRequest.ExternalProviderId))
                .ReturnsAsync(new TvExternals());
            _sonarr.Setup(x => x.GetSeries(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[] { existingSeries });
            _sonarr.Setup(x => x.GetEpisodes(existingSeries.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(episodes);

            var exception = Assert.CatchAsync<System.Exception>(
                async () => await _subject.SendToSonarr(request, settings));

            StringAssert.Contains(TvSender.MissingTvDbAfterRefreshPrefix, exception.Message);
            Assert.That(request.ParentRequest.TvDbId, Is.Zero);
            _mocker.GetMock<ITvRequestRepository>().Verify(x => x.Save(), Times.Never);
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
