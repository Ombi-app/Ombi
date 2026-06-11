using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;
using Ombi.Api.External.MediaServers.Plex.Models;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract test for the Plex libraries endpoint used during server setup. The Plex API is mocked
    /// so the wrapped response shape is pinned without contacting a real Plex server.
    /// </summary>
    [TestFixture]
    public class PlexContractTests : IntegrationTestBase
    {
        [Test]
        public async Task GetLibraries_ReturnsWrappedResponseShape()
        {
            Factory.PlexApi
                .Setup(x => x.GetLibrarySections(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PlexContainer());

            var (status, body) = await PostJsonAsync("/api/v1/Plex/Libraries",
                new { name = "server", plexAuthToken = "token", ip = "127.0.0.1", port = 32400 });

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), "data", "successful", "message");
        }
    }
}
