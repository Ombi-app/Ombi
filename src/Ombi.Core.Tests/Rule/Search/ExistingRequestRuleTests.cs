using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Requests.Tv;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Core.Rule.Rules.Search;
using Xunit;

namespace Ombi.Core.Tests.Rule.Search
{
    public class ExistignRequestRuleTests
    {
        public ExistignRequestRuleTests()
        {
            MovieMock = new Mock<IRequestService<MovieRequestModel>>();
            TvMock = new Mock<IRequestService<TvRequestModel>>();
            Rule = new ExistingRequestRule(MovieMock.Object, TvMock.Object);
        }

        private ExistingRequestRule Rule { get; }
        private Mock<IRequestService<MovieRequestModel>> MovieMock { get; }
        private Mock<IRequestService<TvRequestModel>> TvMock { get; }


        [Fact]
        public async Task ShouldBe_Requested_WhenExisitngMovie()
        {
            var list = new List<MovieRequestModel>{new MovieRequestModel
            {
                ProviderId = 123,
                Approved = true
            }};
            MovieMock.Setup(x => x.GetAllAsync()).ReturnsAsync(list);
            var search = new SearchMovieViewModel
            {
                Id = 123,

            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.Equal(search.Approved, true);
        }

        [Fact]
        public async Task ShouldBe_NotRequested_WhenNewMovie()
        {
            var list = new List<MovieRequestModel>{new MovieRequestModel
            {
                ProviderId = 123,
                Approved = true
            }};
            MovieMock.Setup(x => x.GetAllAsync()).ReturnsAsync(list);
            var search = new SearchMovieViewModel
            {
                Id = 999,

            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.Equal(search.Approved, false);
        }

        [Fact]
        public async Task ShouldBe_Requested_WhenExisitngTv()
        {
            var list = new List<TvRequestModel>{new TvRequestModel
            {
                ProviderId = 123,
                Approved = true
            }};
            TvMock.Setup(x => x.GetAllAsync()).ReturnsAsync(list);
            var search = new SearchTvShowViewModel
            {
                Id = 123,

            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.Equal(search.Approved, true);
        }

        [Fact]
        public async Task ShouldBe_NotRequested_WhenNewTv()
        {
            var list = new List<TvRequestModel>{new TvRequestModel
            {
                ProviderId = 123,
                Approved = true
            }};
            TvMock.Setup(x => x.GetAllAsync()).ReturnsAsync(list);
            var search = new SearchTvShowViewModel()
            {
                Id = 999,

            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.Equal(search.Approved, false);
        }


    }
}