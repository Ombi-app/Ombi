using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the issues endpoints the mobile app reads. Backed by the real issues
    /// controller and repositories against an empty database, so these assert the route + JSON
    /// container the app expects (item shapes are covered by the entity serialization).
    /// </summary>
    [TestFixture]
    public class IssuesContractTests : IntegrationTestBase
    {
        [Test]
        public async Task Categories_ReturnsCategoryShape()
        {
            var (status, body) = await GetAsync("/api/v1/Issues/categories");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var array = AsArray(body);
            Assert.That(array.Count, Is.GreaterThanOrEqualTo(1));
            foreach (var item in array)
            {
                AssertHasProperties((JObject)item, "id", "value");
            }
        }

        [Test]
        public async Task GetIssues_ReturnsIssueShape()
        {
            var (status, body) = await GetAsync("/api/v1/Issues/");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var array = AsArray(body);
            Assert.That(array.Count, Is.GreaterThanOrEqualTo(1));
            foreach (var item in array)
            {
                AssertHasProperties((JObject)item,
                    "id", "title", "subject", "description", "status", "requestType", "createdDate");
            }
        }

        [Test]
        public async Task GetComments_ForUnknownIssue_ReturnsArray()
        {
            var (status, body) = await GetAsync("/api/v1/Issues/1/comments");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(AsArray(body), Is.Not.Null);
        }
    }
}
