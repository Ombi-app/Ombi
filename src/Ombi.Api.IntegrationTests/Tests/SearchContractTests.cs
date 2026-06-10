using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ombi.Api.IntegrationTests.Harness;
using Ombi.Core.Models.Search;
using Ombi.Core.Models.Search.V2;

namespace Ombi.Api.IntegrationTests.Tests
{
    /// <summary>
    /// Contract tests for the V2 Search endpoints the mobile app consumes. The engines are mocked so
    /// the test pins the serialized HTTP shape (route + camelCase JSON) rather than TheMovieDb data.
    /// </summary>
    [TestFixture]
    public class SearchContractTests : IntegrationTestBase
    {
        [Test]
        public async Task PopularMovies_Paged_ReturnsExpectedShape()
        {
            Factory.MovieEngineV2
                .Setup(x => x.PopularMovies(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
                .ReturnsAsync(new List<SearchMovieViewModel>
                {
                    new SearchMovieViewModel
                    {
                        Id = 123,
                        Title = "Test Movie",
                        Overview = "An overview",
                        PosterPath = "/poster.jpg",
                        BackdropPath = "/backdrop.jpg",
                        VoteAverage = 8.1,
                        VoteCount = 100,
                        Available = true,
                        Approved = true,
                        Requested = true,
                    }
                });

            var (status, body) = await GetAsync("/api/v2/search/movie/popular/0/10");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var array = AsArray(body);
            Assert.That(array.Count, Is.EqualTo(1));
            var item = (JObject)array[0];
            AssertHasProperties(item,
                "id", "title", "overview", "posterPath", "backdropPath",
                "releaseDate", "popularity", "voteCount", "voteAverage",
                "available", "approved", "denied", "requested", "requestId",
                "type", "quality", "plexUrl", "embyUrl", "jellyfinUrl", "imdbId");
        }

        [Test]
        public async Task MovieDetails_ReturnsExpectedShape()
        {
            Factory.MovieEngineV2
                .Setup(x => x.GetFullMovieInformation(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<string>()))
                .ReturnsAsync(new MovieFullInfoViewModel
                {
                    Id = 555,
                    Title = "Full Movie",
                    Overview = "Detailed overview",
                    PosterPath = "/p.jpg",
                    BackdropPath = "/b.jpg",
                    Runtime = 120,
                    VoteAverage = 7.5,
                    Available = false,
                    Requested = false,
                });

            var (status, body) = await GetAsync("/api/v2/search/movie/555");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var obj = AsObject(body);
            AssertHasProperties(obj,
                "id", "title", "overview", "posterPath", "backdropPath",
                "runtime", "releaseDate", "voteAverage", "genres",
                "available", "requested", "type");
        }

        [Test]
        public async Task TvDetails_ReturnsExpectedShape()
        {
            Factory.TvSearchEngineV2
                .Setup(x => x.GetShowInformation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SearchFullInfoTvShowViewModel
                {
                    Id = 777,
                    Title = "Test Show",
                    Overview = "Show overview",
                    Status = "Continuing",
                    FirstAired = "2020-01-01",
                    Available = false,
                    Requested = false,
                });

            var (status, body) = await GetAsync("/api/v2/search/tv/777");

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var obj = AsObject(body);
            AssertHasProperties(obj,
                "id", "title", "overview", "status", "firstAired",
                "genres", "available", "requested", "type");
        }

        [Test]
        public async Task MultiSearch_ReturnsExpectedShape()
        {
            Factory.MultiSearchEngine
                .Setup(x => x.MultiSearch(It.IsAny<string>(), It.IsAny<MultiSearchFilter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MultiSearchResult>
                {
                    new MultiSearchResult
                    {
                        Id = "123",
                        MediaType = "movie",
                        Title = "Result",
                        Poster = "/poster.jpg",
                        Overview = "Overview",
                    }
                });

            var (status, body) = await PostJsonAsync("/api/v2/search/multi/star", new { movies = true, tvShows = true });

            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            var array = AsArray(body);
            Assert.That(array.Count, Is.EqualTo(1));
            var item = (JObject)array[0];
            AssertHasProperties(item, "id", "mediaType", "title", "poster", "overview");
        }
    }
}
