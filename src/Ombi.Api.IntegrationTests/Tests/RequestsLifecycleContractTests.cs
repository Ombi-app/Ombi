using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;
using Ombi.Core.Engine;
using Ombi.Core.Models.Requests;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the request mutation flows the mobile app drives. All of these return a
    /// <see cref="RequestEngineResult"/>; the engines are mocked so the serialized shape is pinned.
    /// </summary>
    [TestFixture]
    public class RequestsLifecycleContractTests : IntegrationTestBase
    {
        private static readonly string[] EngineResultProps =
        {
            "result", "message", "isError", "errorMessage", "errorCode", "requestId"
        };

        private static RequestEngineResult OkResult() =>
            new RequestEngineResult { Result = true, Message = "Added", RequestId = 7 };

        [Test]
        public async Task RequestMovie_ReturnsEngineResultShape()
        {
            Factory.MovieRequestEngine.Setup(x => x.RequestMovie(It.IsAny<MovieRequestViewModel>())).ReturnsAsync(OkResult());

            var (status, body) = await PostJsonAsync("/api/v1/request/movie", new { theMovieDbId = 123 });

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), EngineResultProps);
        }

        [Test]
        public async Task RequestTv_ReturnsEngineResultShape()
        {
            // Result=false so the controller skips the real up-vote path and stays deterministic.
            Factory.TvRequestEngine.Setup(x => x.RequestTvShow(It.IsAny<TvRequestViewModelV2>()))
                .ReturnsAsync(new RequestEngineResult { Result = false, Message = "ok", RequestId = 3 });

            var (status, body) = await PostJsonAsync("/api/v2/requests/tv", new { theMovieDbId = 123, requestAll = true });

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), EngineResultProps);
        }

        [Test]
        public async Task ApproveMovie_ReturnsEngineResultShape()
        {
            Factory.MovieRequestEngine.Setup(x => x.ApproveMovieById(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(OkResult());

            var (status, body) = await PostJsonAsync("/api/v1/request/movie/approve", new { id = 1 });

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), EngineResultProps);
        }

        [Test]
        public async Task DenyMovie_ReturnsEngineResultShape()
        {
            Factory.MovieRequestEngine.Setup(x => x.DenyMovieById(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(OkResult());

            var (status, body) = await PutJsonAsync("/api/v1/request/movie/deny", new { id = 1, reason = "no" });

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), EngineResultProps);
        }

        [Test]
        public async Task MarkMovieAvailable_ReturnsEngineResultShape()
        {
            Factory.MovieRequestEngine.Setup(x => x.MarkAvailable(It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(OkResult());

            var (status, body) = await PostJsonAsync("/api/v1/request/movie/available", new { id = 1 });

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), EngineResultProps);
        }

        [Test]
        public async Task UpdateMovieAdvancedOptions_ReturnsEngineResultShape()
        {
            Factory.MovieRequestEngine.Setup(x => x.UpdateAdvancedOptions(It.IsAny<MediaAdvancedOptions>())).ReturnsAsync(OkResult());

            var (status, body) = await PostJsonAsync("/api/v2/requests/movie/advancedoptions", new { requestId = 1, rootPathOverride = 1, qualityOverride = 1 });

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), EngineResultProps);
        }

        [Test]
        public async Task RecentlyRequested_ReturnsArray()
        {
            var (status, body) = await GetAsync("/api/v2/requests/recentlyRequested");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(AsArray(body), Is.Not.Null);
        }
    }
}
