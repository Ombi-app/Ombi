using System.Reflection;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
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
