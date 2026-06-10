using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the settings endpoints the mobile app reads. These are backed by the real
    /// settings controller/service against the (default) seeded settings.
    /// </summary>
    [TestFixture]
    public class SettingsContractTests : IntegrationTestBase
    {
        [Test]
        public async Task Customization_ReturnsExpectedShape()
        {
            var (status, body) = await GetAsync("/api/v1/settings/customization");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var obj = AsObject(body);
            AssertHasProperties(obj,
                "applicationName", "applicationUrl", "logo", "customCss",
                "recentlyAddedPage", "useCustomPage", "hideAvailableFromDiscover");
        }

        [Test]
        public async Task Sonarr_ReturnsExpectedShape()
        {
            var (status, body) = await GetAsync("/api/v1/Settings/sonarr");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var obj = AsObject(body);
            AssertHasProperties(obj,
                "enabled", "apiKey", "qualityProfile", "seasonFolders", "rootPath");
        }

        [Test]
        public async Task Authentication_ReturnsExpectedShape()
        {
            var (status, body) = await GetAsync("/api/v1/settings/authentication");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body),
                "allowNoPassword", "enableOAuth", "enableHeaderAuth",
                "requireDigit", "requiredLength");
        }

        [Test]
        public async Task Plex_ReturnsExpectedShape()
        {
            var (status, body) = await GetAsync("/api/v1/Settings/plex");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), "enable", "servers");
        }

        [Test]
        public async Task ClientId_ReturnsString()
        {
            var (status, body) = await GetAsync("/api/v1/settings/clientid");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            // Returned as a bare text/plain string (the client id), not a JSON object.
            Assert.That(body.Trim().Length, Is.GreaterThan(0), "Expected a non-empty client id");
        }

        [Test]
        public async Task IssuesEnabled_ReturnsBoolean()
        {
            var (status, body) = await GetAsync("/api/v1/settings/issuesenabled");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.Trim(), Is.AnyOf("true", "false"));
        }
    }
}
