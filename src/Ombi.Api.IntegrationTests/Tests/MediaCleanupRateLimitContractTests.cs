using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;

namespace Ombi.Api.IntegrationTests.Tests
{
    [TestFixture]
    public class MediaCleanupRateLimitContractTests : IntegrationTestBase
    {
        [Test]
        public async Task MutationEndpoint_IsLimitedAfterTwentyRequestsPerWindow()
        {
            for (var i = 0; i < 20; i++)
            {
                using var response = await Client.PostAsync($"/api/v1/mediacleanup/cancel/rate-limit-contract-{i}", null);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Request {i + 1} should be admitted by the mutation limiter.");
            }

            using var rejected = await Client.PostAsync("/api/v1/mediacleanup/cancel/rate-limit-contract-rejected", null);
            Assert.That(rejected.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        }
    }
}
