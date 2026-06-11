using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the Radarr/Sonarr endpoints the mobile app calls when building a request.
    /// With the DVRs disabled (the default seeded state) these short-circuit without touching the
    /// external APIs, so they deterministically validate the routes and response contracts.
    /// </summary>
    [TestFixture]
    public class ExternalDvrContractTests : IntegrationTestBase
    {
        [Test]
        public async Task RadarrEnabled_ReturnsBoolean()
        {
            var (status, body) = await GetAsync("/api/v1/Radarr/enabled");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.Trim(), Is.AnyOf("true", "false"));
        }

        [Test]
        public async Task RadarrProfiles_ReturnsArray()
        {
            var (status, body) = await GetAsync("/api/v1/Radarr/Profiles");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(AsArray(body), Is.Not.Null);
        }

        [Test]
        public async Task RadarrRootFolders_ReturnsSuccess()
        {
            // When Radarr is disabled the action returns null, which MVC serializes as 204 No Content.
            var (status, body) = await GetAsync("/api/v1/Radarr/RootFolders");
            Assert.That(status, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.NoContent));
            if (status == HttpStatusCode.OK)
            {
                Assert.That(AsArray(body), Is.Not.Null);
            }
        }

        [Test]
        public async Task SonarrEnabled_ReturnsBoolean()
        {
            var (status, body) = await GetAsync("/api/v1/Sonarr/enabled");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.Trim(), Is.AnyOf("true", "false"));
        }

        [Test]
        public async Task SonarrProfiles_ReturnsSuccess()
        {
            // Disabled Sonarr returns null -> 204 No Content.
            var (status, body) = await GetAsync("/api/v1/Sonarr/Profiles");
            Assert.That(status, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.NoContent));
            if (status == HttpStatusCode.OK)
            {
                Assert.That(AsArray(body), Is.Not.Null);
            }
        }

        [Test]
        public async Task SonarrRootFolders_ReturnsSuccess()
        {
            var (status, body) = await GetAsync("/api/v1/Sonarr/RootFolders");
            Assert.That(status, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.NoContent));
            if (status == HttpStatusCode.OK)
            {
                Assert.That(AsArray(body), Is.Not.Null);
            }
        }
    }
}
