using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.Radarr;
using Ombi.Api.External.ExternalApis.Radarr.Models;
using Ombi.Api.External.ExternalApis.Sonarr;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Helpers;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    [NonParallelizable]
    public class MediaCleanupEngineDestructiveTests
    {
        private AutoMocker _mocker;
        private MediaCleanupEngine _subject;
        private MediaCleanupState _state;
        private MediaCleanupSettings _cleanupSettings;
        private RadarrSettings _radarrSettings;
        private Radarr4KSettings _radarr4KSettings;
        private SonarrSettings _sonarrSettings;
        private Mock<ISettingsService<MediaCleanupState>> _stateService;
        private Mock<ISettingsService<MediaCleanupState>> _checkpointStateService;
        private Mock<IMovieRequestRepository> _movieRequests;
        private Mock<ITvRequestRepository> _tvRequests;
        private Mock<IRadarrV3Api> _radarr;
        private Mock<ISonarrV3Api> _sonarr;
        private Mock<OmbiUserManager> _userManager;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMocker();
            _state = new MediaCleanupState();
            _cleanupSettings = new MediaCleanupSettings
            {
                OwnRequestRemoval = OwnRequestRemovalMode.RequestRemoval,
                CommunityCleanup = CommunityCleanupMode.Off,
                DeleteFiles = true
            };
            _radarrSettings = new RadarrSettings
            {
                Enabled = true,
                ApiKey = "radarr-key",
                Ip = "localhost",
                Port = 7878
            };
            _radarr4KSettings = new Radarr4KSettings
            {
                Enabled = false,
                ApiKey = "radarr-4k-key",
                Ip = "localhost",
                Port = 7879
            };
            _sonarrSettings = new SonarrSettings
            {
                Enabled = true,
                ApiKey = "sonarr-key",
                Ip = "localhost",
                Port = 8989
            };

            _mocker.GetMock<ISettingsService<MediaCleanupSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(() => _cleanupSettings);
            _mocker.GetMock<ISettingsService<RadarrSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(() => _radarrSettings);
            _mocker.GetMock<ISettingsService<Radarr4KSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(() => _radarr4KSettings);
            _mocker.GetMock<ISettingsService<SonarrSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(() => _sonarrSettings);
            _mocker.GetMock<ISettingsService<PlexSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings { Enable = false });

            _stateService = _mocker.GetMock<ISettingsService<MediaCleanupState>>();
            _stateService.Setup(x => x.GetSettingsAsync()).ReturnsAsync(() => _state);
            _stateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>())).ReturnsAsync(true);

            _checkpointStateService = new Mock<ISettingsService<MediaCleanupState>>();
            _checkpointStateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>())).ReturnsAsync(true);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(ISettingsService<MediaCleanupState>)))
                .Returns(_checkpointStateService.Object);
            var serviceScope = new Mock<IServiceScope>();
            serviceScope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider.Object);
            _mocker.GetMock<IServiceScopeFactory>()
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            _movieRequests = _mocker.GetMock<IMovieRequestRepository>();
            _movieRequests.Setup(x => x.GetAll())
                .Returns(new List<MovieRequests>().AsQueryable().BuildMock());
            _movieRequests.Setup(x => x.DeleteRange(It.IsAny<IEnumerable<MovieRequests>>()))
                .Returns(Task.CompletedTask);

            _tvRequests = _mocker.GetMock<ITvRequestRepository>();
            _tvRequests.Setup(x => x.Get())
                .Returns(new List<TvRequests>().AsQueryable().BuildMock());
            _tvRequests.Setup(x => x.GetLite())
                .Returns(new List<TvRequests>().AsQueryable().BuildMock());
            _tvRequests.Setup(x => x.DeleteRange(It.IsAny<IEnumerable<TvRequests>>()))
                .Returns(Task.CompletedTask);

            _mocker.GetMock<IExternalRepository<RadarrCache>>()
                .Setup(x => x.GetAll())
                .Returns(new List<RadarrCache>().AsQueryable().BuildMock());
            _mocker.GetMock<IExternalRepository<SonarrCache>>()
                .Setup(x => x.GetAll())
                .Returns(new List<SonarrCache>().AsQueryable().BuildMock());
            _mocker.GetMock<IExternalRepository<SonarrEpisodeCache>>()
                .Setup(x => x.GetAll())
                .Returns(new List<SonarrEpisodeCache>().AsQueryable().BuildMock());
            _mocker.GetMock<IMediaCacheService>()
                .Setup(x => x.Purge())
                .Returns(Task.CompletedTask);

            _radarr = _mocker.GetMock<IRadarrV3Api>();
            _sonarr = _mocker.GetMock<ISonarrV3Api>();

            // MediaCleanupEngine takes the concrete user manager even though most destructive
            // tests do not need a user. Keep the mock so immediate-removal tests can enable the
            // required roles without changing the rest of the fixture.
            _userManager = MockHelper.MockUserManager(new List<OmbiUser>());
            _mocker.Use(_userManager.Object);

            _subject = _mocker.CreateInstance<MediaCleanupEngine>();
        }

        [Test]
        public async Task ImmediateMovieDeletion_PersistsCleanupBeforeExternalDelete()
        {
            var events = new List<string>();
            var movie = SetupImmediateMovieRequest();

            _radarr.Setup(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MovieResponse>
                {
                    new MovieResponse { id = 44, tmdbId = movie.TheMovieDbId, title = movie.Title }
                });
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .Callback<int, string, string, bool, bool>((_, _, _, _, _) => events.Add("external-delete"))
                .ReturnsAsync(true);
            _checkpointStateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()))
                .Callback<MediaCleanupState>(state =>
                {
                    var record = state.Requests.Single();
                    if (!record.ExternalDeletionCompletedAt.HasValue)
                    {
                        Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.ScheduledForDeletion));
                        Assert.That(record.ScheduledForDeletionAt, Is.Not.Null);
                        Assert.That(record.DeletionPlan, Is.Not.Null);
                        Assert.That(record.DeletionPlan.DeleteFiles, Is.True);
                        Assert.That(record.DeletionPlan.AddImportExclusion, Is.False);
                        Assert.That(record.DeletionPlan.Version, Is.EqualTo(2));
                        Assert.That(record.DeletionPlan.Authorization, Is.Not.Null);
                        Assert.That(record.DeletionPlan.Authorization.RequestClaims, Is.EqualTo(new[] { "movie:100:owner" }));
                        Assert.That(record.DeletionPlan.Targets, Has.Count.EqualTo(1));
                        Assert.That(record.DeletionPlan.Targets[0].Service, Is.EqualTo(MediaCleanupExternalService.Radarr));
                        Assert.That(record.DeletionPlan.Targets[0].Endpoint, Does.Contain("localhost:7878"));
                    }
                    events.Add(record.ExternalDeletionCompletedAt.HasValue
                        ? "post-delete-checkpoint"
                        : "pre-delete-checkpoint");
                })
                .ReturnsAsync(true);

            var result = await _subject.RequestOwnRemoval(RequestType.Movie, movie.Id);

            Assert.That(result.Result, Is.True);
            Assert.That(events, Is.EqualTo(new[]
            {
                "pre-delete-checkpoint",
                "external-delete",
                "post-delete-checkpoint"
            }));
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Exactly(2));
            _stateService.Verify(x => x.SaveSettingsAsync(_state), Times.Once);
        }

        [Test]
        public async Task ImmediateMovieDeletion_PreDeletePersistenceFailure_DoesNotCallRadarr()
        {
            var movie = SetupImmediateMovieRequest();
            _checkpointStateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()))
                .ReturnsAsync(false);

            var result = await _subject.RequestOwnRemoval(RequestType.Movie, movie.Id);

            Assert.That(result.Result, Is.False);
            Assert.That(result.Message, Does.Contain("could not be persisted"));
            Assert.That(result.Message, Does.Contain("No media was deleted"));
            Assert.That(_state.Requests, Is.Empty,
                "A cleanup request that could not be persisted must not remain active in memory.");
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
            _stateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
        }

        [Test]
        public async Task OwnRemovalRequest_PersistenceFalse_DoesNotReportSuccess()
        {
            var movie = SetupImmediateMovieRequest();
            _cleanupSettings.OwnRequestRemoval = OwnRequestRemovalMode.RequestRemoval;
            _stateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()))
                .ReturnsAsync(false);

            var result = await _subject.RequestOwnRemoval(RequestType.Movie, movie.Id);

            Assert.That(result.Result, Is.False);
            Assert.That(result.Message, Does.Contain("could not persist"));
            Assert.That(result.Message, Does.Contain("not confirmed"));
            Assert.That(_state.Requests, Is.Empty,
                "A newly-created cleanup record must not remain in the in-memory state when its initial save fails.");
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task ImmediateMovieDeletion_FinalPersistenceFalse_ReportsDurableRecoveryCheckpoint()
        {
            var movie = SetupImmediateMovieRequest();
            _radarr.Setup(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MovieResponse>
                {
                    new MovieResponse { id = 44, tmdbId = movie.TheMovieDbId, title = movie.Title }
                });
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ReturnsAsync(true);
            _stateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()))
                .ReturnsAsync(false);

            var result = await _subject.RequestOwnRemoval(RequestType.Movie, movie.Id);

            Assert.That(result.Result, Is.False);
            Assert.That(result.Message, Does.Contain("removed from the external service"));
            Assert.That(result.Message, Does.Contain("recovery checkpoint is durable"));
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false), Times.Once);
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Exactly(2));
            _stateService.Verify(x => x.SaveSettingsAsync(_state), Times.Once);
        }

        [Test]
        public async Task Approve_PersistenceFalse_DoesNotReportScheduledSuccess()
        {
            var manager = new OmbiUser { Id = "manager", UserName = "manager", NormalizedUserName = "MANAGER" };
            _mocker.GetMock<ICurrentUser>()
                .Setup(x => x.GetUser())
                .ReturnsAsync(manager);
            _userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var record = new MediaCleanupRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                RequestType = RequestType.Movie,
                MediaRequestId = 100,
                TheMovieDbId = 200,
                Title = "Approval Persistence Test Movie",
                OwnerUserIds = new List<string> { "owner" },
                Origin = MediaCleanupOrigin.OwnRequest,
                Status = MediaCleanupStatus.PendingAdminApproval,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            _state.Requests.Add(record);
            _movieRequests.Setup(x => x.GetAll())
                .Returns(new[] { MovieRequest(record.MediaRequestId, record.TheMovieDbId, "owner") }.AsQueryable().BuildMock());
            _stateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()))
                .ReturnsAsync(false);

            var result = await _subject.Approve(record.Id);

            Assert.That(result.Result, Is.False);
            Assert.That(result.Message, Does.Contain("could not persist"));
            Assert.That(result.Message, Does.Contain("not confirmed"));
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task SuccessfulMovieDeletion_CheckpointsBeforeOmbiRequestDeletion()
        {
            var events = new List<string>();
            var record = AddDueMovie();
            var request = MovieRequest(record.MediaRequestId, record.TheMovieDbId, "owner");
            _movieRequests.Setup(x => x.GetAll())
                .Returns(new List<MovieRequests> { request }.AsQueryable().BuildMock());
            SetupMovieInRadarr(record);
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .Callback<int, string, string, bool, bool>((_, _, _, _, _) => events.Add("external-delete"))
                .ReturnsAsync(true);
            _checkpointStateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()))
                .Callback<MediaCleanupState>(_ => events.Add("checkpoint"))
                .ReturnsAsync(true);
            _movieRequests.Setup(x => x.DeleteRange(It.IsAny<IEnumerable<MovieRequests>>()))
                .Callback<IEnumerable<MovieRequests>>(_ => events.Add("ombi-reconcile"))
                .Returns(Task.CompletedTask);

            await _subject.ProcessPending();

            Assert.That(events, Is.EqualTo(new[] { "external-delete", "checkpoint", "ombi-reconcile" }));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Not.Null);
            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
        }

        [Test]
        public async Task ReconciliationFailure_AfterCheckpoint_RetriesWithoutDeletingExternallyAgain()
        {
            var record = AddDueMovie();
            var request = MovieRequest(record.MediaRequestId, record.TheMovieDbId, "owner");
            _movieRequests.Setup(x => x.GetAll())
                .Returns(new List<MovieRequests> { request }.AsQueryable().BuildMock());
            SetupMovieInRadarr(record);
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ReturnsAsync(true);
            _movieRequests.SetupSequence(x => x.DeleteRange(It.IsAny<IEnumerable<MovieRequests>>()))
                .ThrowsAsync(new InvalidOperationException("database is busy"))
                .Returns(Task.CompletedTask);

            await _subject.ProcessPending();

            Assert.That(record.ExternalDeletionCompletedAt, Is.Not.Null);
            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.ScheduledForDeletion));
            Assert.That(record.RetryCount, Is.EqualTo(1));
            Assert.That(record.FailureReason, Does.Contain("Ombi reconciliation failed"));
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false), Times.Once);

            record.NextRetryAt = DateTime.UtcNow.AddSeconds(-1);
            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
            Assert.That(record.RetryCount, Is.Zero);
            Assert.That(record.NextRetryAt, Is.Null);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false), Times.Once);
            _movieRequests.Verify(x => x.DeleteRange(It.IsAny<IEnumerable<MovieRequests>>()), Times.Exactly(2));
        }

        [Test]
        public async Task CheckpointPersistenceFailure_KeepsCheckpointAndRetriesWithoutSecondExternalDelete()
        {
            var record = AddDueMovie();
            SetupMovieInRadarr(record);
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ReturnsAsync(true);
            _checkpointStateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()))
                .ReturnsAsync(false);

            await _subject.ProcessPending();

            Assert.That(record.ExternalDeletionCompletedAt, Is.Not.Null,
                "The in-memory checkpoint must survive so the normal state save can persist it.");
            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.ScheduledForDeletion));
            Assert.That(record.RetryCount, Is.EqualTo(1));
            Assert.That(record.FailureReason, Does.Contain("external deletion recovery checkpoint failed"));
            _stateService.Verify(x => x.SaveSettingsAsync(_state), Times.Once,
                "ProcessPending should get a second chance to persist the checkpoint through its normal state save.");

            record.NextRetryAt = DateTime.UtcNow.AddSeconds(-1);
            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false), Times.Once);
        }

        [Test]
        public async Task TransientExternalFailure_SchedulesFifteenMinuteRetryWithoutCheckpoint()
        {
            var before = DateTime.UtcNow;
            var record = AddDueMovie();
            _radarr.Setup(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Radarr unavailable"));

            await _subject.ProcessPending();

            var after = DateTime.UtcNow;
            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.ScheduledForDeletion));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.RetryCount, Is.EqualTo(1));
            Assert.That(record.LastFailureAt, Is.Not.Null);
            Assert.That(record.NextRetryAt, Is.Not.Null);
            Assert.That(record.NextRetryAt.Value, Is.InRange(before.AddMinutes(14), after.AddMinutes(16)));
            Assert.That(record.FailureReason, Does.Contain("external deletion failed"));
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task PermanentRadarrQueryHttpFailure_FailsWithoutRetry()
        {
            var record = AddDueMovie();
            _radarr.Setup(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException(
                    "External API request failed with HTTP 401 (Unauthorized).",
                    null,
                    HttpStatusCode.Unauthorized));

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.RetryCount, Is.Zero);
            Assert.That(record.NextRetryAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("401"));
            _radarr.Verify(x => x.DeleteMovie(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()), Times.Never);
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
        }

        [Test]
        public async Task RadarrRejectsDeleteWithoutStatus_IsTerminalFailure()
        {
            var record = AddDueMovie();
            SetupMovieInRadarr(record);
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ReturnsAsync(false);

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.RetryCount, Is.Zero);
            Assert.That(record.NextRetryAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("without an HTTP status"));
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.UnprocessableEntity)]
        public async Task PermanentRadarrHttpFailure_FailsWithoutRetry(HttpStatusCode statusCode)
        {
            var record = AddDueMovie();
            SetupMovieInRadarr(record);
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ThrowsAsync(new HttpRequestException(
                    $"Radarr rejected the movie delete request with HTTP {(int)statusCode} ({statusCode}).",
                    null,
                    statusCode));

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.RetryCount, Is.Zero);
            Assert.That(record.NextRetryAt, Is.Null);
            Assert.That(record.ScheduledForDeletionAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain(((int)statusCode).ToString()));
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
        }

        [TestCase(HttpStatusCode.RequestTimeout)]
        [TestCase(HttpStatusCode.TooManyRequests)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public async Task TransientRadarrHttpFailure_SchedulesRetry(HttpStatusCode statusCode)
        {
            var record = AddDueMovie();
            SetupMovieInRadarr(record);
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ThrowsAsync(new HttpRequestException(
                    $"Radarr rejected the movie delete request with HTTP {(int)statusCode} ({statusCode}).",
                    null,
                    statusCode));

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.ScheduledForDeletion));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.RetryCount, Is.EqualTo(1));
            Assert.That(record.NextRetryAt, Is.Not.Null);
            Assert.That(record.FailureReason, Does.Contain("will retry automatically"));
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
        }

        [Test]
        public async Task WholeMovieRetry_MissingFromRadarr_AssumesPriorDeleteCompletedAndReconciles()
        {
            var record = AddDueMovie();
            record.RetryCount = 1;
            record.LastFailureAt = DateTime.UtcNow.AddMinutes(-15);
            record.NextRetryAt = DateTime.UtcNow.AddSeconds(-1);
            _radarr.Setup(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MovieResponse>());

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Not.Null);
            Assert.That(record.RetryCount, Is.Zero);
            Assert.That(record.FailureReason, Is.Null);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Once);
        }

        [Test]
        public async Task FirstMovieAttempt_MissingFromRadarr_IsTerminalFailure()
        {
            var record = AddDueMovie();
            _radarr.Setup(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MovieResponse>());

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.ScheduledForDeletionAt, Is.Null);
            Assert.That(record.NextRetryAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("could not be found"));
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
        }

        [Test]
        public async Task CheckpointedDeletion_ReconcilesEvenWhenMediaCleanupIsDisabled()
        {
            var record = AddDueMovie();
            record.ExternalDeletionCompletedAt = DateTime.UtcNow.AddMinutes(-1);
            _cleanupSettings.OwnRequestRemoval = OwnRequestRemovalMode.Off;
            _cleanupSettings.CommunityCleanup = CommunityCleanupMode.Off;

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task PartialTvCleanup_SafeSelection_UnmonitorsBeforeDeletingFileAndCheckpoints()
        {
            var events = new List<string>();
            var record = AddDuePartialTv();
            record.SelectedEpisodes.Add(new MediaCleanupEpisodeRecord
            {
                SeasonNumber = 1,
                EpisodeNumber = 2,
                EpisodeFileId = 50,
                SizeOnDisk = 1234
            });
            var series = new SonarrSeries { id = 33, tvdbId = record.TvDbId, title = record.Title };
            _sonarr.Setup(x => x.GetSeriesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[] { series });
            _sonarr.Setup(x => x.GetEpisodesForCleanup(series.id, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[]
                {
                    new Episode { id = 101, seasonNumber = 1, episodeNumber = 1, hasFile = true, episodeFileId = 50 },
                    new Episode { id = 102, seasonNumber = 1, episodeNumber = 2, hasFile = true, episodeFileId = 50 }
                });
            _sonarr.Setup(x => x.MonitorEpisodeForCleanup(It.IsAny<int[]>(), false, It.IsAny<string>(), It.IsAny<string>()))
                .Callback<int[], bool, string, string>((_, _, _, _) => events.Add("unmonitor"))
                .ReturnsAsync(new List<MonitoredEpisodeResult>
                {
                    new MonitoredEpisodeResult { id = 101, seasonNumber = 1, episodeNumber = 1, monitored = false },
                    new MonitoredEpisodeResult { id = 102, seasonNumber = 1, episodeNumber = 2, monitored = false }
                });
            _sonarr.Setup(x => x.DeleteEpisodeFile(50, It.IsAny<string>(), It.IsAny<string>()))
                .Callback<int, string, string>((_, _, _) => events.Add("delete-file"))
                .ReturnsAsync(true);
            _checkpointStateService.Setup(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()))
                .Callback<MediaCleanupState>(_ => events.Add("checkpoint"))
                .ReturnsAsync(true);

            await _subject.ProcessPending();

            Assert.That(events, Is.EqualTo(new[] { "unmonitor", "delete-file", "checkpoint" }));
            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Not.Null);
            _sonarr.Verify(x => x.MonitorEpisodeForCleanup(It.Is<int[]>(ids => ids.OrderBy(x => x).SequenceEqual(new[] { 101, 102 })), false, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _sonarr.Verify(x => x.DeleteEpisodeFile(50, It.IsAny<string>(), It.IsAny<string>()), Times.Once,
                "A shared multi-episode file must only be deleted once when every covered episode is approved.");
            _sonarr.Verify(x => x.DeleteSeries(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task PartialTvDelete_ForbiddenHttpFailure_IsTerminalWithoutRetry()
        {
            var record = AddDuePartialTv();
            var series = new SonarrSeries { id = 33, tvdbId = record.TvDbId, title = record.Title };
            _sonarr.Setup(x => x.GetSeriesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[] { series });
            _sonarr.Setup(x => x.GetEpisodesForCleanup(series.id, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[]
                {
                    new Episode { id = 101, seasonNumber = 1, episodeNumber = 1, hasFile = true, episodeFileId = 50 }
                });
            _sonarr.Setup(x => x.MonitorEpisodeForCleanup(It.IsAny<int[]>(), false, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MonitoredEpisodeResult>
                {
                    new MonitoredEpisodeResult { id = 101, seasonNumber = 1, episodeNumber = 1, monitored = false }
                });
            _sonarr.Setup(x => x.DeleteEpisodeFile(50, It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException(
                    "Sonarr rejected the episode-file delete request with HTTP 403 (Forbidden).",
                    null,
                    HttpStatusCode.Forbidden));

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.RetryCount, Is.Zero);
            Assert.That(record.NextRetryAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("403"));
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
        }

        [Test]
        public async Task PartialTvCleanup_InferredSelectedSeason_DoesNotAuthorizeEpisodeDownloadedLater()
        {
            var record = AddDuePartialTv();
            // This is how a cleanup created when E01 was the only downloaded episode can be stored:
            // SelectedSeasons says the season was complete at selection time, but only E01 was
            // actually approved. A later E02 download must remain outside the authorization scope.
            record.SelectedSeasons.Add(1);

            var series = new SonarrSeries
            {
                id = 33,
                tvdbId = record.TvDbId,
                title = record.Title,
                seasons = new[]
                {
                    new Season { seasonNumber = 1, monitored = true }
                }
            };
            _sonarr.Setup(x => x.GetSeriesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[] { series });
            _sonarr.Setup(x => x.GetEpisodesForCleanup(series.id, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[]
                {
                    new Episode { id = 101, seasonNumber = 1, episodeNumber = 1, hasFile = true, episodeFileId = 50 },
                    new Episode { id = 102, seasonNumber = 1, episodeNumber = 2, hasFile = true, episodeFileId = 51 }
                });
            _sonarr.Setup(x => x.MonitorEpisodeForCleanup(It.IsAny<int[]>(), false, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MonitoredEpisodeResult>
                {
                    new MonitoredEpisodeResult { id = 101, seasonNumber = 1, episodeNumber = 1, monitored = false }
                });
            _sonarr.Setup(x => x.DeleteEpisodeFile(50, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var selectedCacheRow = new SonarrEpisodeCache
            {
                TvDbId = record.TvDbId,
                MovieDbId = record.TheMovieDbId,
                SeasonNumber = 1,
                EpisodeNumber = 1,
                HasFile = true
            };
            var laterCacheRow = new SonarrEpisodeCache
            {
                TvDbId = record.TvDbId,
                MovieDbId = record.TheMovieDbId,
                SeasonNumber = 1,
                EpisodeNumber = 2,
                HasFile = true
            };
            var episodeCache = _mocker.GetMock<IExternalRepository<SonarrEpisodeCache>>();
            episodeCache.Setup(x => x.GetAll())
                .Returns(new[] { selectedCacheRow, laterCacheRow }.AsQueryable().BuildMock());
            IReadOnlyList<SonarrEpisodeCache> deletedCacheRows = Array.Empty<SonarrEpisodeCache>();
            episodeCache.Setup(x => x.DeleteRange(It.IsAny<IEnumerable<SonarrEpisodeCache>>()))
                .Callback<IEnumerable<SonarrEpisodeCache>>(rows => deletedCacheRows = rows.ToList())
                .Returns(Task.CompletedTask);

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Not.Null);
            _sonarr.Verify(x => x.MonitorEpisodeForCleanup(
                It.Is<int[]>(ids => ids.SequenceEqual(new[] { 101 })),
                false,
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
            _sonarr.Verify(x => x.DeleteEpisodeFile(50, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _sonarr.Verify(x => x.DeleteEpisodeFile(51, It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sonarr.Verify(x => x.UpdateSeries(It.IsAny<SonarrSeries>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never,
                "An inferred SelectedSeasons entry must not unmonitor the whole season.");
            Assert.That(deletedCacheRows, Has.Count.EqualTo(1));
            Assert.That(deletedCacheRows[0].EpisodeNumber, Is.EqualTo(1),
                "Cache reconciliation must use the frozen episode scope rather than SelectedSeasons.");
        }

        [Test]
        public async Task PartialTvCleanup_SharedFileWithUnselectedEpisode_FailsBeforeDestructiveCalls()
        {
            var record = AddDuePartialTv();
            record.SelectedSeasons.Add(1);
            var series = new SonarrSeries { id = 33, tvdbId = record.TvDbId, title = record.Title };
            _sonarr.Setup(x => x.GetSeriesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[] { series });
            _sonarr.Setup(x => x.GetEpisodesForCleanup(series.id, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[]
                {
                    new Episode { id = 101, seasonNumber = 1, episodeNumber = 1, hasFile = true, episodeFileId = 50 },
                    new Episode { id = 102, seasonNumber = 1, episodeNumber = 2, hasFile = true, episodeFileId = 50 }
                });

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("S01E02"));
            _sonarr.Verify(x => x.MonitorEpisodeForCleanup(It.IsAny<int[]>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sonarr.Verify(x => x.DeleteEpisodeFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sonarr.Verify(x => x.DeleteSeries(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task PartialTvRetry_MissingSelectedEpisode_DoesNotUseWholeTitleRecoveryShortcut()
        {
            var record = AddDuePartialTv();
            record.RetryCount = 1;
            record.LastFailureAt = DateTime.UtcNow.AddMinutes(-15);
            record.NextRetryAt = DateTime.UtcNow.AddSeconds(-1);
            var series = new SonarrSeries { id = 33, tvdbId = record.TvDbId, title = record.Title };
            _sonarr.Setup(x => x.GetSeriesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[] { series });
            _sonarr.Setup(x => x.GetEpisodesForCleanup(series.id, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<Episode>());

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("selected episodes could not be found"));
            _checkpointStateService.Verify(x => x.SaveSettingsAsync(It.IsAny<MediaCleanupState>()), Times.Never);
            _sonarr.Verify(x => x.DeleteSeries(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
            _sonarr.Verify(x => x.DeleteEpisodeFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ScheduledMovieDeletion_NewSameOwnerRequestAfterAuthorization_FailsClosedBeforeRadarrQuery()
        {
            var record = AddDueMovie();
            var original = MovieRequest(record.MediaRequestId, record.TheMovieDbId, "owner");
            var laterDuplicate = MovieRequest(record.MediaRequestId + 1, record.TheMovieDbId, "owner");
            _movieRequests.Setup(x => x.GetAll())
                .Returns(new[] { original, laterDuplicate }.AsQueryable().BuildMock());

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("request or ownership state changed"));
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task ScheduledMovieDeletion_OriginalRequestRemoved_FailsClosedBeforeRadarrQuery()
        {
            var record = AddDueMovie();
            _movieRequests.Setup(x => x.GetAll())
                .Returns(Array.Empty<MovieRequests>().AsQueryable().BuildMock());

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("no longer exists"));
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task ScheduledTvDeletion_NewOwnerAfterAuthorization_FailsClosedBeforeSonarrQuery()
        {
            var record = AddDuePartialTv();
            var parent = TvRequest(record,
                TvChild(301, "owner"),
                TvChild(302, "new-owner"));
            _tvRequests.Setup(x => x.Get())
                .Returns(new[] { parent }.AsQueryable().BuildMock());

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("request or ownership state changed"));
            _sonarr.Verify(x => x.GetSeriesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sonarr.Verify(x => x.DeleteEpisodeFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _sonarr.Verify(x => x.DeleteSeries(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task VersionOneDeletionPlan_FailsClosedBeforeExternalQuery()
        {
            var record = AddDueMovie();
            record.DeletionPlan.Version = 1;
            record.DeletionPlan.Authorization = null;

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("authorization snapshot"));
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ScheduledMovieDeletion_UsesFrozenDestructiveFlagsAfterSettingsChange()
        {
            var record = AddDueMovie();
            SetupMovieInRadarr(record);

            // Global settings drift after authorization must not change what the old approval means.
            _cleanupSettings.DeleteFiles = false;
            _cleanupSettings.AddImportExclusion = true;
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ReturnsAsync(true);

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false), Times.Once);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), false, true), Times.Never);
        }

        [Test]
        public async Task ScheduledMovieDeletion_ApprovedRadarrDestinationChanged_FailsClosedBeforeQuery()
        {
            var record = AddDueMovie();

            // The cleanup was authorized for localhost:7878, then the configured destination moved.
            _radarrSettings.Port = 7999;

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("destination changed"));
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task ScheduledMovieDeletion_NewRadarr4KEnabledAfterAuthorization_IsNotAddedToOldPlan()
        {
            var record = AddDueMovie();
            SetupMovieInRadarr(record);
            _radarr4KSettings.Enabled = true;
            _radarr.Setup(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), true, false))
                .ReturnsAsync(true);

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Completed));
            _radarr.Verify(x => x.GetMoviesForCleanup(_radarrSettings.ApiKey, It.IsAny<string>()), Times.Once);
            _radarr.Verify(x => x.GetMoviesForCleanup(_radarr4KSettings.ApiKey, It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task LegacyScheduledCleanupWithoutDeletionPlan_FailsClosed()
        {
            var record = AddDueMovie();
            record.DeletionPlan = null;

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Failed));
            Assert.That(record.ExternalDeletionCompletedAt, Is.Null);
            Assert.That(record.FailureReason, Does.Contain("authorized before destructive settings"));
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task AutomaticCommunityAuthorization_SnapshotsDeletionPlanWhenThresholdIsFinal()
        {
            _cleanupSettings.OwnRequestRemoval = OwnRequestRemovalMode.Off;
            _cleanupSettings.CommunityCleanup = CommunityCleanupMode.AutomaticAfterThreshold;
            _cleanupSettings.MinimumDeleteVotes = 2;
            _cleanupSettings.RequiredVoteMargin = 1;
            _cleanupSettings.RequesterCanVeto = false;
            _cleanupSettings.GracePeriodDays = 1;
            var record = AddExpiredCommunityVote();
            record.Votes.AddRange(new[]
            {
                Vote("delete-1", MediaCleanupVoteType.Delete),
                Vote("delete-2", MediaCleanupVoteType.Delete)
            });

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.ScheduledForDeletion));
            Assert.That(record.DeletionPlan, Is.Not.Null);
            Assert.That(record.DeletionPlan.DeleteFiles, Is.True);
            Assert.That(record.DeletionPlan.Authorization, Is.Not.Null);
            Assert.That(record.DeletionPlan.Authorization.RequestClaims, Is.EqualTo(new[] { "movie:600:community-owner" }));
            Assert.That(record.DeletionPlan.Targets, Has.Count.EqualTo(1));
            Assert.That(record.DeletionPlan.Targets[0].Service, Is.EqualTo(MediaCleanupExternalService.Radarr));
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task CommunityCleanup_RequesterKeepVeto_BlocksDeletion()
        {
            _cleanupSettings.OwnRequestRemoval = OwnRequestRemovalMode.Off;
            _cleanupSettings.CommunityCleanup = CommunityCleanupMode.AutomaticAfterThreshold;
            _cleanupSettings.MinimumDeleteVotes = 2;
            _cleanupSettings.RequiredVoteMargin = 1;
            _cleanupSettings.RequesterCanVeto = true;
            _cleanupSettings.AnyKeepVotePreventsRemoval = false;
            _cleanupSettings.GracePeriodDays = 0;
            var record = AddExpiredCommunityVote();
            record.OwnerUserIds.Add("owner");
            record.Votes.AddRange(new[]
            {
                Vote("delete-1", MediaCleanupVoteType.Delete),
                Vote("delete-2", MediaCleanupVoteType.Delete),
                Vote("delete-3", MediaCleanupVoteType.Delete),
                Vote("owner", MediaCleanupVoteType.Keep)
            });

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Rejected));
            Assert.That(record.ScheduledForDeletionAt, Is.Null);
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public async Task CommunityCleanup_GlobalKeepVeto_BlocksDeletion()
        {
            _cleanupSettings.OwnRequestRemoval = OwnRequestRemovalMode.Off;
            _cleanupSettings.CommunityCleanup = CommunityCleanupMode.AutomaticAfterThreshold;
            _cleanupSettings.MinimumDeleteVotes = 2;
            _cleanupSettings.RequiredVoteMargin = 1;
            _cleanupSettings.RequesterCanVeto = false;
            _cleanupSettings.AnyKeepVotePreventsRemoval = true;
            _cleanupSettings.GracePeriodDays = 0;
            var record = AddExpiredCommunityVote();
            record.OwnerUserIds.Add("owner");
            record.Votes.AddRange(new[]
            {
                Vote("delete-1", MediaCleanupVoteType.Delete),
                Vote("delete-2", MediaCleanupVoteType.Delete),
                Vote("delete-3", MediaCleanupVoteType.Delete),
                Vote("someone-else", MediaCleanupVoteType.Keep)
            });

            await _subject.ProcessPending();

            Assert.That(record.Status, Is.EqualTo(MediaCleanupStatus.Rejected));
            Assert.That(record.ScheduledForDeletionAt, Is.Null);
            _radarr.Verify(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _radarr.Verify(x => x.DeleteMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        }

        private MovieRequests SetupImmediateMovieRequest()
        {
            _cleanupSettings.OwnRequestRemoval = OwnRequestRemovalMode.ImmediateDeletion;
            var user = new OmbiUser { Id = "owner", UserName = "owner", NormalizedUserName = "OWNER" };
            _mocker.GetMock<ICurrentUser>()
                .Setup(x => x.GetUser())
                .ReturnsAsync(user);
            _userManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var movie = new MovieRequests
            {
                Id = 100,
                TheMovieDbId = 200,
                Title = "Immediate Safety Test Movie",
                Available = true,
                RequestedUserId = user.Id,
                RequestedDate = DateTime.UtcNow.AddDays(-10),
                MarkedAsAvailable = DateTime.UtcNow.AddDays(-5)
            };
            _movieRequests.Setup(x => x.GetWithUser())
                .Returns(new[] { movie }.AsQueryable().BuildMock());
            _movieRequests.Setup(x => x.GetAll())
                .Returns(new[] { movie }.AsQueryable().BuildMock());
            return movie;
        }

        private MediaCleanupRecord AddDueMovie()
        {
            var record = new MediaCleanupRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                RequestType = RequestType.Movie,
                MediaRequestId = 100,
                TheMovieDbId = 200,
                Title = "Safety Test Movie",
                OwnerUserIds = new List<string> { "owner" },
                Origin = MediaCleanupOrigin.OwnRequest,
                Status = MediaCleanupStatus.ScheduledForDeletion,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ScheduledForDeletionAt = DateTime.UtcNow.AddMinutes(-1),
                DeletionPlan = new MediaCleanupDeletionPlan
                {
                    DeleteFiles = _cleanupSettings.DeleteFiles,
                    AddImportExclusion = _cleanupSettings.AddImportExclusion,
                    AuthorizedAt = DateTime.UtcNow.AddHours(-1),
                    Authorization = new MediaCleanupAuthorizationSnapshot
                    {
                        OwnerUserIds = new List<string> { "owner" },
                        RequestClaims = new List<string> { "movie:100:owner" }
                    },
                    Targets = new List<MediaCleanupExternalTarget>
                    {
                        new MediaCleanupExternalTarget
                        {
                            Service = MediaCleanupExternalService.Radarr,
                            Endpoint = _radarrSettings.FullUri.TrimEnd('/')
                        }
                    }
                }
            };
            _state.Requests.Add(record);
            _movieRequests.Setup(x => x.GetAll())
                .Returns(new[] { MovieRequest(record.MediaRequestId, record.TheMovieDbId, "owner") }.AsQueryable().BuildMock());
            return record;
        }

        private MediaCleanupRecord AddDuePartialTv()
        {
            var record = new MediaCleanupRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                RequestType = RequestType.TvShow,
                MediaRequestId = 300,
                TvDbId = 400,
                TheMovieDbId = 500,
                Title = "Safety Test Series",
                OwnerUserIds = new List<string> { "owner" },
                Origin = MediaCleanupOrigin.OwnRequest,
                Status = MediaCleanupStatus.ScheduledForDeletion,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ScheduledForDeletionAt = DateTime.UtcNow.AddMinutes(-1),
                DeletionPlan = new MediaCleanupDeletionPlan
                {
                    DeleteFiles = true,
                    AddImportExclusion = false,
                    AuthorizedAt = DateTime.UtcNow.AddHours(-1),
                    Authorization = new MediaCleanupAuthorizationSnapshot
                    {
                        OwnerUserIds = new List<string> { "owner" },
                        RequestClaims = new List<string> { "tv-child:300:301:owner" }
                    },
                    Targets = new List<MediaCleanupExternalTarget>
                    {
                        new MediaCleanupExternalTarget
                        {
                            Service = MediaCleanupExternalService.Sonarr,
                            Endpoint = _sonarrSettings.FullUri.TrimEnd('/')
                        }
                    }
                },
                SelectedEpisodes = new List<MediaCleanupEpisodeRecord>
                {
                    new MediaCleanupEpisodeRecord
                    {
                        SeasonNumber = 1,
                        EpisodeNumber = 1,
                        EpisodeFileId = 50,
                        SizeOnDisk = 1234
                    }
                }
            };
            _state.Requests.Add(record);
            _tvRequests.Setup(x => x.Get())
                .Returns(new[] { TvRequest(record, TvChild(301, "owner")) }.AsQueryable().BuildMock());
            return record;
        }

        private MediaCleanupRecord AddExpiredCommunityVote()
        {
            var record = new MediaCleanupRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                RequestType = RequestType.Movie,
                MediaRequestId = 600,
                TheMovieDbId = 700,
                Title = "Community Safety Test Movie",
                Origin = MediaCleanupOrigin.Community,
                Status = MediaCleanupStatus.Voting,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                VotingEndsAt = DateTime.UtcNow.AddMinutes(-1)
            };
            _state.Requests.Add(record);
            _movieRequests.Setup(x => x.GetAll())
                .Returns(new[] { MovieRequest(record.MediaRequestId, record.TheMovieDbId, "community-owner") }.AsQueryable().BuildMock());
            return record;
        }

        private static MovieRequests MovieRequest(int id, int tmdbId, string owner)
        {
            return new MovieRequests
            {
                Id = id,
                TheMovieDbId = tmdbId,
                Title = "Safety Test Movie",
                Available = true,
                RequestedUserId = owner,
                RequestedDate = DateTime.UtcNow.AddDays(-10),
                MarkedAsAvailable = DateTime.UtcNow.AddDays(-5)
            };
        }

        private static ChildRequests TvChild(int id, string owner)
        {
            return new ChildRequests
            {
                Id = id,
                Available = true,
                Approved = true,
                RequestedUserId = owner,
                SeasonRequests = new List<SeasonRequests>()
            };
        }

        private static TvRequests TvRequest(MediaCleanupRecord record, params ChildRequests[] children)
        {
            return new TvRequests
            {
                Id = record.MediaRequestId,
                TvDbId = record.TvDbId,
                ExternalProviderId = record.TheMovieDbId,
                Title = record.Title,
                ChildRequests = children.ToList()
            };
        }

        private void SetupMovieInRadarr(MediaCleanupRecord record)
        {
            _radarr.Setup(x => x.GetMoviesForCleanup(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MovieResponse>
                {
                    new MovieResponse
                    {
                        id = 44,
                        tmdbId = record.TheMovieDbId,
                        title = record.Title
                    }
                });
        }

        private static MediaCleanupVoteRecord Vote(string userId, MediaCleanupVoteType vote)
        {
            return new MediaCleanupVoteRecord
            {
                UserId = userId,
                Vote = vote,
                Date = DateTime.UtcNow.AddHours(-1)
            };
        }
    }
}
