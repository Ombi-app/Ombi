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
    }
}
