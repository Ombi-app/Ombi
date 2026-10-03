using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Hubs;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Tests;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class ArrAvailabilityCheckerTests
    {
        private AutoMocker _mocker;
        private ArrAvailabilityChecker _subject;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            var hub = SignalRHelper.MockHub<NotificationHub>();
            _mocker.Use(hub);

            _mocker.Setup<ISettingsService<RadarrSettings>, Task<RadarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = true, ScanForAvailability = true });
            _mocker.Setup<ISettingsService<SonarrSettings>, Task<SonarrSettings>>(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings { Enabled = false, ScanForAvailability = false });

            _subject = _mocker.CreateInstance<ArrAvailabilityChecker>();
        }

        private void GivenRadarrCache(params RadarrCache[] cache)
            => _mocker.Setup<IExternalRepository<RadarrCache>, IQueryable<RadarrCache>>(x => x.GetAll())
                      .Returns(cache.AsQueryable());

        private void GivenRequests(params MovieRequests[] requests)
            => _mocker.Setup<IMovieRequestRepository, IQueryable<MovieRequests>>(x => x.GetAll())
                      .Returns(requests.AsQueryable());

        /// <summary>
        /// Single Radarr instance whose quality profile grabs 2160p. There is no dedicated
        /// Radarr 4K instance, and the 4K request feature is off, so the movie should simply
        /// be marked as available.
        /// </summary>
        [Test]
        public async Task ProcessMovies_ShouldMarkAvailable_WhenSingleRadarrGrabbed4KFile()
        {
            GivenRadarrCache(new RadarrCache { TheMovieDbId = 99, HasFile = true, Has4K = true, HasRegular = false });
            var request = new MovieRequests { Id = 1, TheMovieDbId = 99, Approved = true, Available = false, Has4KRequest = false };
            GivenRequests(request);

            await _subject.Execute(null);

            Assert.That(request.Available, Is.True, "A 4K file from the only Radarr instance should satisfy a standard request");
        }

        /// <summary>
        /// A 1080p grab must keep working exactly as before.
        /// </summary>
        [Test]
        public async Task ProcessMovies_ShouldMarkAvailable_WhenRadarrGrabbedRegularFile()
        {
            GivenRadarrCache(new RadarrCache { TheMovieDbId = 99, HasFile = true, Has4K = false, HasRegular = true });
            var request = new MovieRequests { Id = 1, TheMovieDbId = 99, Approved = true, Available = false, Has4KRequest = false };
            GivenRequests(request);

            await _subject.Execute(null);

            Assert.That(request.Available, Is.True);
        }

        /// <summary>
        /// With the 4K feature on and a genuine 4K request, the 4K flag is what should be set.
        /// </summary>
        [Test]
        public async Task ProcessMovies_ShouldMark4KAvailable_When4KFeatureEnabledAndIs4KRequest()
        {
            _mocker.Setup<IFeatureService, Task<bool>>(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(true);
            GivenRadarrCache(new RadarrCache { TheMovieDbId = 99, HasFile = true, Has4K = true, HasRegular = false });
            var request = new MovieRequests { Id = 1, TheMovieDbId = 99, Approved4K = true, Available4K = false, Has4KRequest = true };
            GivenRequests(request);

            await _subject.Execute(null);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available4K, Is.True);
                Assert.That(request.Available, Is.False, "the 4K copy should not satisfy a standard request while the 4K feature is on");
            });
        }

        /// <summary>
        /// Nothing downloaded yet - the request must stay unavailable.
        /// </summary>
        [Test]
        public async Task ProcessMovies_ShouldNotMarkAvailable_WhenRadarrHasNoFile()
        {
            GivenRadarrCache(new RadarrCache { TheMovieDbId = 99, HasFile = false, Has4K = false, HasRegular = true });
            var request = new MovieRequests { Id = 1, TheMovieDbId = 99, Approved = true, Available = false };
            GivenRequests(request);

            await _subject.Execute(null);

            Assert.That(request.Available, Is.False);
        }
    }
}
