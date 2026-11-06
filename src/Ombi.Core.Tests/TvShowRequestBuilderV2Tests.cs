using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Api.External.ExternalApis.TheMovieDb.Models;
using Ombi.Core.Helpers;
using Ombi.Core.Models.Requests;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Tests
{
    [TestFixture]
    public class TvShowRequestBuilderV2Tests
    {
        [TestCase("en", 16, "Animation", "anime", SeriesType.Anime)]
        [TestCase("ru", 16, "мультфильм", "anime", SeriesType.Anime)]
        [TestCase("en", 16, "Animation", "family", SeriesType.Standard)]
        [TestCase("ru", 16, "мультфильм", null, SeriesType.Standard)]
        [TestCase("en", 18, "Drama", "anime", SeriesType.Standard)]
        public async Task CreateChild_SetsSeriesType(string language, int genreId, string genreName,
            string keyword, SeriesType expectedSeriesType)
        {
            var movieDbApi = new Mock<IMovieDbApi>();
            movieDbApi.Setup(x => x.GetTVInfo("1", language)).ReturnsAsync(new TvInfo
            {
                genres = new[] { new Genre { id = genreId, name = genreName } },
                seasons = new List<Season>(),
                Keywords = keyword == null ? null : new Keywords
                {
                    KeywordsValue = new List<KeywordsValue> { new KeywordsValue { Name = keyword } }
                }
            });
            var builder = new TvShowRequestBuilderV2(movieDbApi.Object);
            await builder.GetShowInfo(1, language);

            builder.CreateChild(new TvRequestViewModelV2 { TheMovieDbId = 1 }, "user", RequestSource.Ombi);

            Assert.That(builder.ChildRequest.SeriesType, Is.EqualTo(expectedSeriesType));
        }
    }
}
