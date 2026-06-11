using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.RottenTomatoes.Models;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
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

        private static readonly string[] MovieListProps =
        {
            "id", "title", "overview", "posterPath", "backdropPath", "releaseDate",
            "voteAverage", "voteCount", "available", "approved", "requested", "type"
        };

        private static readonly string[] TvListProps =
        {
            "id", "title", "status", "firstAired", "banner", "available",
            "approved", "requested", "type"
        };

        private static List<SearchMovieViewModel> OneMovie() => new List<SearchMovieViewModel>
        {
            new SearchMovieViewModel { Id = 1, Title = "M", Overview = "o", PosterPath = "/p", BackdropPath = "/b", VoteAverage = 5 }
        };

        private static List<SearchTvShowViewModel> OneShow() => new List<SearchTvShowViewModel>
        {
            new SearchTvShowViewModel { Id = 1, Title = "S", Status = "Continuing", FirstAired = "2020", Banner = "/banner" }
        };

        [Test]
        public async Task TopRatedMovies_ReturnsExpectedShape()
        {
            Factory.MovieEngineV2.Setup(x => x.TopRatedMovies(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(OneMovie());
            var (status, body) = await GetAsync("/api/v2/search/movie/toprated/0/10");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties((JObject)AsArray(body)[0], MovieListProps);
        }

        [Test]
        public async Task UpcomingMovies_ReturnsExpectedShape()
        {
            Factory.MovieEngineV2.Setup(x => x.UpcomingMovies(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(OneMovie());
            var (status, body) = await GetAsync("/api/v2/search/movie/upcoming/0/10");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties((JObject)AsArray(body)[0], MovieListProps);
        }

        [Test]
        public async Task NowPlayingMovies_ReturnsExpectedShape()
        {
            Factory.MovieEngineV2.Setup(x => x.NowPlayingMovies(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(OneMovie());
            var (status, body) = await GetAsync("/api/v2/search/movie/nowplaying/0/10");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties((JObject)AsArray(body)[0], MovieListProps);
        }

        [Test]
        public async Task PopularTv_ReturnsExpectedShape()
        {
            Factory.TvSearchEngineV2.Setup(x => x.Popular(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(OneShow());
            var (status, body) = await GetAsync("/api/v2/search/tv/popular/0/10");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties((JObject)AsArray(body)[0], TvListProps);
        }

        [Test]
        public async Task TrendingTv_ReturnsExpectedShape()
        {
            Factory.TvSearchEngineV2.Setup(x => x.Trending(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(OneShow());
            var (status, body) = await GetAsync("/api/v2/search/tv/trending/0/10");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties((JObject)AsArray(body)[0], TvListProps);
        }

        [Test]
        public async Task AnticipatedTv_ReturnsExpectedShape()
        {
            Factory.TvSearchEngineV2.Setup(x => x.Anticipated(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(OneShow());
            var (status, body) = await GetAsync("/api/v2/search/tv/anticipated/0/10");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties((JObject)AsArray(body)[0], TvListProps);
        }

        [Test]
        public async Task MovieStreams_ReturnsExpectedShape()
        {
            Factory.MovieEngineV2.Setup(x => x.GetStreamInformation(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StreamingData> { new StreamingData { Order = 1, StreamingProvider = "Netflix", Logo = "/logo" } });
            var (status, body) = await GetAsync("/api/v2/search/stream/movie/123");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties((JObject)AsArray(body)[0], "order", "streamingProvider", "logo");
        }

        [Test]
        public async Task TvStreams_ReturnsExpectedShape()
        {
            Factory.TvSearchEngineV2.Setup(x => x.GetStreamInformation(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StreamingData> { new StreamingData { Order = 1, StreamingProvider = "Hulu", Logo = "/logo" } });
            var (status, body) = await GetAsync("/api/v2/search/stream/tv/123");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties((JObject)AsArray(body)[0], "order", "streamingProvider", "logo");
        }

        [Test]
        public async Task ActorMovieCredits_ReturnsExpectedShape()
        {
            Factory.MovieEngineV2.Setup(x => x.GetMoviesByActor(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new ActorCredits { id = 1, cast = new[] { new Cast { id = 2, character = "Hero", poster_path = "/p" } } });
            var (status, body) = await GetAsync("/api/v2/search/actor/1/movie");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), "id", "cast", "crew");
        }

        [Test]
        public async Task ActorTvCredits_ReturnsExpectedShape()
        {
            Factory.TvSearchEngineV2.Setup(x => x.GetTvByActor(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new ActorCredits { id = 1, cast = new[] { new Cast { id = 2, character = "Hero", poster_path = "/p" } } });
            var (status, body) = await GetAsync("/api/v2/search/actor/1/tv");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), "id", "cast", "crew");
        }

        [Test]
        public async Task MovieRatings_ReturnsExpectedShape()
        {
            Factory.RottenTomatoesApi.Setup(x => x.GetMovieRatings(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new MovieRatings { critics_rating = "Certified Fresh", critics_score = 98, audience_rating = "Upright", audience_score = 90 });
            var (status, body) = await GetAsync("/api/v2/search/ratings/movie/Up/2009");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), "critics_rating", "critics_score", "audience_rating", "audience_score");
        }

        [Test]
        public async Task TvRatings_ReturnsExpectedShape()
        {
            Factory.RottenTomatoesApi.Setup(x => x.GetTvRatings(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new TvRatings { Class = "fresh", Score = 90 });
            var (status, body) = await GetAsync("/api/v2/search/ratings/tv/Lost/2004");
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            AssertHasProperties(AsObject(body), "class", "score");
        }
    }
}
