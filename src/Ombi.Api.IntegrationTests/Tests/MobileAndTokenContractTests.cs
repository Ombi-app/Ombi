using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the mobile push-notification registration endpoints (real DB-backed) and
    /// the token/login endpoint's failure contract.
    /// </summary>
    [TestFixture]
    public class MobileAndTokenContractTests : IntegrationTestBase
    {
        [Test]
        public async Task RegisterNotificationToken_ReturnsOk()
        {
            var (status, _) = await PostJsonAsync("/api/v2/mobile/notification", new { token = "expo-token-123" });
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task RemoveNotificationToken_ReturnsOk()
        {
            using var resp = await Client.DeleteAsync("/api/v2/mobile/notification");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var (status, _) = await PostJsonAsync("/api/v1/token", new { username = "does-not-exist", password = "wrong" });
            Assert.That(status, Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}
