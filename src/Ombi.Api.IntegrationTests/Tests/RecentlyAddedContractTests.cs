using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;
using Ombi.Core.Models;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the recently-added endpoints the mobile app's discover screen reads. The
    /// recently-added engine is mocked so the serialized item shapes are pinned.
    /// </summary>
    [TestFixture]
    public class RecentlyAddedContractTests : IntegrationTestBase
    {
        [Test]
        public async Task RecentlyAddedMovies_ReturnsExpectedShape()
        {
            Factory.RecentlyAddedEngine
                .Setup(x => x.GetRecentlyAddedMovies(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<RecentlyAddedMovieModel>
                {
                    new RecentlyAddedMovieModel { Id = 1, Title = "M", Overview = "o", ImdbId = "tt1", TheMovieDbId = "123", ReleaseYear = "2020", AddedAt = DateTime.UtcNow }
                });

            var (status, body) = await GetAsync("/api/v1/RecentlyAdded/movies");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var item = (JObject)AsArray(body)[0];
            AssertHasProperties(item, "id", "title", "overview", "imdbId", "theMovieDbId", "releaseYear", "addedAt");
        }

        [Test]
        public async Task RecentlyAddedTv_ReturnsExpectedShape()
        {
            Factory.RecentlyAddedEngine
                .Setup(x => x.GetRecentlyAddedTv(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
                .Returns(new List<RecentlyAddedTvModel>
                {
                    new RecentlyAddedTvModel { Id = 1, Title = "S", Overview = "o", ImdbId = "tt1", TheMovieDbId = "123", ReleaseYear = "2020" }
                });

            var (status, body) = await GetAsync("/api/v1/RecentlyAdded/tv");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var item = (JObject)AsArray(body)[0];
            AssertHasProperties(item, "id", "title", "overview", "imdbId", "theMovieDbId", "releaseYear");
        }
    }
}
