using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract test for <c>GET api/v1/Identity</c> (the current user), backed by the real identity
    /// controller and the seeded test user.
    /// </summary>
    [TestFixture]
    public class IdentityContractTests : IntegrationTestBase
    {
        [Test]
        public async Task CurrentUser_ReturnsExpectedShape()
        {
            var (status, body) = await GetAsync("/api/v1/Identity");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var obj = AsObject(body);
            AssertHasProperties(obj,
                "id", "userName", "alias", "claims", "emailAddress",
                "language", "hasLoggedIn", "userType",
                "movieRequestLimit", "episodeRequestLimit", "musicRequestLimit",
                "streamingCountry", "movieRequestLimitType");
        }
    }
}
