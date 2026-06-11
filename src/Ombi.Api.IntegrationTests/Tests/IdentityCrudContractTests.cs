using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the identity management endpoints the mobile app's admin screens use.
    /// Backed by the real identity controller / user manager against the seeded test user.
    /// </summary>
    [TestFixture]
    public class IdentityCrudContractTests : IntegrationTestBase
    {
        [Test]
        public async Task GetAllUsers_ReturnsUserShape()
        {
            var (status, body) = await GetAsync("/api/v1/Identity/Users");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var array = AsArray(body);
            Assert.That(array.Count, Is.GreaterThanOrEqualTo(1));
            foreach (var item in array)
            {
                AssertHasProperties((JObject)item,
                    "id", "userName", "alias", "claims", "emailAddress", "userType",
                    "movieRequestLimit", "episodeRequestLimit", "streamingCountry");
            }
        }

        [Test]
        public async Task GetUsersDropdown_ReturnsDropdownShape()
        {
            var (status, body) = await GetAsync("/api/v1/identity/dropdown/users");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var array = AsArray(body);
            Assert.That(array.Count, Is.GreaterThanOrEqualTo(1));
            foreach (var item in array)
            {
                AssertHasProperties((JObject)item, "id", "username", "email");
            }
        }

        [Test]
        public async Task GetClaims_ReturnsClaimCheckboxShape()
        {
            var (status, body) = await GetAsync("/api/v1/identity/claims");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var array = AsArray(body);
            Assert.That(array.Count, Is.GreaterThanOrEqualTo(1));
            foreach (var item in array)
            {
                AssertHasProperties((JObject)item, "value", "enabled");
            }
        }
    }
}
