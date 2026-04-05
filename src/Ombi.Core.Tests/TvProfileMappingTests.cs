using AutoMapper;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
using Ombi.Core.Models.Search;
using Ombi.Mapping.Profiles;

namespace Ombi.Core.Tests
{
    [TestFixture]
    public class TvProfileMappingTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TvProfile>();
            });
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [Test]
        public void MovieDbSearchResult_Maps_PosterPath_Correctly()
        {
            var source = new MovieDbSearchResult
            {
                Id = 1,
                Title = "Test Show",
                PosterPath = "/poster123.jpg",
                BackdropPath = "/backdrop456.jpg",
                Overview = "A test show",
                VoteAverage = 8.5f,
                ReleaseDate = "2024-01-15",
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.PosterPath, Is.EqualTo("/poster123.jpg"));
        }

        [Test]
        public void MovieDbSearchResult_Maps_BackdropPath_Correctly()
        {
            var source = new MovieDbSearchResult
            {
                Id = 1,
                Title = "Test Show",
                PosterPath = "/poster123.jpg",
                BackdropPath = "/backdrop456.jpg",
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.BackdropPath, Is.EqualTo("/backdrop456.jpg"));
        }

        [Test]
        public void MovieDbSearchResult_PosterPath_And_BackdropPath_Are_Different()
        {
            var source = new MovieDbSearchResult
            {
                Id = 1,
                Title = "Test Show",
                PosterPath = "/poster.jpg",
                BackdropPath = "/backdrop.jpg",
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.PosterPath, Is.Not.EqualTo(result.BackdropPath));
            Assert.That(result.PosterPath, Is.EqualTo("/poster.jpg"));
            Assert.That(result.BackdropPath, Is.EqualTo("/backdrop.jpg"));
        }

        [Test]
        public void MovieDbSearchResult_Maps_Title()
        {
            var source = new MovieDbSearchResult
            {
                Id = 42,
                Title = "Breaking Bad",
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.Title, Is.EqualTo("Breaking Bad"));
        }

        [Test]
        public void MovieDbSearchResult_Maps_Rating()
        {
            var source = new MovieDbSearchResult
            {
                Id = 1,
                VoteAverage = 9.2f,
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.Rating, Is.EqualTo("9.2"));
        }

        [Test]
        public void MovieDbSearchResult_Maps_Id()
        {
            var source = new MovieDbSearchResult
            {
                Id = 550,
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.Id, Is.EqualTo(550));
        }

        [Test]
        public void MovieDbSearchResult_Handles_Null_PosterPath()
        {
            var source = new MovieDbSearchResult
            {
                Id = 1,
                PosterPath = null,
                BackdropPath = "/backdrop.jpg",
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.PosterPath, Is.Null);
            Assert.That(result.BackdropPath, Is.EqualTo("/backdrop.jpg"));
        }

        [Test]
        public void MovieDbSearchResult_Handles_Null_BackdropPath()
        {
            var source = new MovieDbSearchResult
            {
                Id = 1,
                PosterPath = "/poster.jpg",
                BackdropPath = null,
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.PosterPath, Is.EqualTo("/poster.jpg"));
            Assert.That(result.BackdropPath, Is.Null);
        }

        [Test]
        public void TvSearchResult_Maps_PosterPath_Correctly()
        {
            var source = new TvSearchResult
            {
                Id = 1,
                Name = "Test TV",
                PosterPath = "/tv-poster.jpg",
                BackdropPath = "/tv-backdrop.jpg",
                Overview = "A TV show",
                VoteAverage = 7.5f,
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.PosterPath, Is.EqualTo("/tv-poster.jpg"));
        }

        [Test]
        public void TvSearchResult_Maps_BackdropPath_Via_Convention()
        {
            var source = new TvSearchResult
            {
                Id = 1,
                Name = "Test TV",
                PosterPath = "/tv-poster.jpg",
                BackdropPath = "/tv-backdrop.jpg",
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.BackdropPath, Is.EqualTo("/tv-backdrop.jpg"));
        }

        [Test]
        public void TvSearchResult_Banner_Uses_BackdropPath_When_Available()
        {
            var source = new TvSearchResult
            {
                Id = 1,
                Name = "Test",
                BackdropPath = "/backdrop.jpg",
                PosterPath = "/poster.jpg",
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            // Banner should use backdrop when available (converted to https)
            Assert.That(result.Banner, Does.Contain("backdrop.jpg"));
        }

        [Test]
        public void TvSearchResult_Banner_Falls_Back_To_PosterPath()
        {
            var source = new TvSearchResult
            {
                Id = 1,
                Name = "Test",
                BackdropPath = null,
                PosterPath = "/poster.jpg",
            };

            var result = _mapper.Map<SearchTvShowViewModel>(source);

            Assert.That(result.Banner, Is.EqualTo("/poster.jpg"));
        }
    }
}
