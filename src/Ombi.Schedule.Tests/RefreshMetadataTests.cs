using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using AutoMapper;
using Ombi.Api;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Schedule.Jobs.Ombi;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class RefreshMetadataTests
    {
        private AutoMocker _mocker;
        private RefreshMetadata _subject;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<RefreshMetadata>();
        }

        [Test]
        public async Task GetTvDbId_WithImdbFallback_ReturnsTvDbId()
        {
            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.Find("tt1234567", ExternalSource.imdb_id))
                .ReturnsAsync(new FindResult
                {
                    tv_results = new[] { new TvResults { id = 42 } }
                });
            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.GetTvExternals(42))
                .ReturnsAsync(new TvExternals
                {
                    imdb_id = "tt1234567",
                    tvdb_id = 98765
                });

            var result = await InvokeGetTvDbId(false, true, string.Empty, "tt1234567", "Example Show");

            Assert.That(result, Is.EqualTo("98765"));
        }

        [Test]
        public async Task GetTvDbId_WithImdbFallbackAndNoTvDbId_ReturnsEmpty()
        {
            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.Find("tt1234567", ExternalSource.imdb_id))
                .ReturnsAsync(new FindResult
                {
                    tv_results = new[] { new TvResults { id = 42 } }
                });
            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.GetTvExternals(42))
                .ReturnsAsync(new TvExternals
                {
                    imdb_id = "tt1234567",
                    tvdb_id = 0
                });

            var result = await InvokeGetTvDbId(false, true, string.Empty, "tt1234567", "Example Show");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetTvDbId_WithImdbFallbackAndNoTmdbMatch_ReturnsEmptyWithoutExternalLookup()
        {
            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.Find("tt1234567", ExternalSource.imdb_id))
                .ReturnsAsync(new FindResult { tv_results = System.Array.Empty<TvResults>() });

            var result = await InvokeGetTvDbId(false, true, string.Empty, "tt1234567", "Example Show");

            Assert.That(result, Is.Empty);
            _mocker.GetMock<IMovieDbApi>()
                .Verify(x => x.GetTvExternals(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetTvDbId_WithTmdbIdAndZeroTvDbId_ReturnsEmpty()
        {
            _mocker.GetMock<IMovieDbApi>()
                .Setup(x => x.GetTvExternals(42))
                .ReturnsAsync(new TvExternals { tvdb_id = 0 });

            var result = await InvokeGetTvDbId(true, false, "42", string.Empty, "Example Show");

            Assert.That(result, Is.Empty);
        }


        [Test]
        public async Task GetTheMovieDbId_WithMissingImdbValue_DoesNotCallFind()
        {
            var result = await _subject.GetTheMovieDbId(false, true, string.Empty, "   ", "Example Movie", true);

            Assert.That(result, Is.Empty);
            _mocker.GetMock<IMovieDbApi>()
                .Verify(x => x.Find(It.IsAny<string>(), It.IsAny<ExternalSource>()), Times.Never);
        }

        [Test]
        public async Task GetTheMovieDbId_WithMissingTvDbValue_DoesNotCallFind()
        {
            var result = await _subject.GetTheMovieDbId(true, false, "   ", string.Empty, "Example Show", false);

            Assert.That(result, Is.Empty);
            _mocker.GetMock<IMovieDbApi>()
                .Verify(x => x.Find(It.IsAny<string>(), It.IsAny<ExternalSource>()), Times.Never);
        }

        [Test]
        public async Task TheMovieDbFind_WithBlankExternalId_DoesNotSendRequest()
        {
            var api = new Mock<IApi>();
            var subject = new Ombi.Api.External.ExternalApis.TheMovieDb.TheMovieDbApi(
                Mock.Of<IMapper>(),
                api.Object,
                Mock.Of<ISettingsService<TheMovieDbSettings>>());

            var result = await subject.Find("   ", ExternalSource.imdb_id);

            Assert.That(result, Is.Not.Null);
            api.Verify(x => x.Request<FindResult>(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private async Task<string> InvokeGetTvDbId(bool hasTheMovieDb, bool hasImdb, string theMovieDbId, string imdbId, string title)
        {
            var method = typeof(RefreshMetadata).GetMethod("GetTvDbId", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null);

            var task = (Task<string>)method.Invoke(_subject, new object[]
            {
                hasTheMovieDb,
                hasImdb,
                theMovieDbId,
                imdbId,
                title
            });

            return await task;
        }
    }
}
