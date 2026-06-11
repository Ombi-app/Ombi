using System;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;
using Ombi.Core.Models.UI;
using Ombi.Store.Entities.Requests;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the paginated request lists the mobile app renders. The request engines are
    /// mocked to return a populated <see cref="RequestsViewModel{T}"/> so the wrapper and item shapes
    /// the app deserializes are pinned.
    /// </summary>
    [TestFixture]
    public class RequestsContractTests : IntegrationTestBase
    {
        [Test]
        public async Task MovieRequests_ReturnsWrapperAndItemShape()
        {
            Factory.MovieRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RequestsViewModel<MovieRequests>
                {
                    Total = 1,
                    Collection = new[]
                    {
                        new MovieRequests
                        {
                            Id = 10,
                            Title = "Requested Movie",
                            TheMovieDbId = 999,
                            Approved = true,
                            Available = false,
                            RequestedDate = DateTime.UtcNow,
                            Subscribed = false,
                            ShowSubscribe = true,
                        }
                    }
                });

            var (status, body) = await GetAsync("/api/v2/requests/movie/10/0/requestedDate/desc");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var obj = AsObject(body);
            AssertHasProperties(obj, "collection", "total");

            var item = (JObject)((JArray)obj["collection"])[0];
            AssertHasProperties(item,
                "id", "title", "theMovieDbId", "approved", "available",
                "requestedDate", "subscribed", "showSubscribe",
                "rootPathOverride", "qualityOverride", "requestType");
        }

        [Test]
        public async Task TvRequests_ReturnsWrapperAndItemShape()
        {
            Factory.TvRequestEngine
                .Setup(x => x.GetRequests(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RequestsViewModel<ChildRequests>
                {
                    Total = 1,
                    Collection = new[]
                    {
                        new ChildRequests
                        {
                            Id = 20,
                            Title = "Requested Show",
                            Approved = true,
                            Available = false,
                            RequestedDate = DateTime.UtcNow,
                            Subscribed = false,
                            ShowSubscribe = true,
                        }
                    }
                });

            var (status, body) = await GetAsync("/api/v2/requests/tv/10/0/requestedDate/desc");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var obj = AsObject(body);
            AssertHasProperties(obj, "collection", "total");

            var item = (JObject)((JArray)obj["collection"])[0];
            AssertHasProperties(item,
                "id", "title", "approved", "available", "requestedDate",
                "subscribed", "showSubscribe", "requestStatus");
        }
    }
}
