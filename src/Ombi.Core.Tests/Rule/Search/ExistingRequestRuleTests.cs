using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Xunit;

namespace Ombi.Core.Tests.Rule.Search
{
    public class ExistignRequestRuleTests
    {
        public ExistignRequestRuleTests()
        {
            MovieMock = new Mock<IMovieRequestRepository>();
            TvMock = new Mock<ITvRequestRepository>();
            Rule = new ExistingRule(MovieMock.Object, TvMock.Object);
        }

        private ExistingRule Rule { get; }
        private Mock<IMovieRequestRepository> MovieMock { get; }
        private Mock<ITvRequestRepository> TvMock { get; }


        [Fact]
        public async Task ShouldBe_Requested_WhenExisitngMovie()
        {
            var list = new MovieRequests
            {
                TheMovieDbId = 123,
                Approved = true
            };

            MovieMock.Setup(x => x.GetRequest(123)).ReturnsAsync(list);
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
            var list = new MovieRequests
            {
                TheMovieDbId = 123,
                Approved = true
            };

            MovieMock.Setup(x => x.GetRequest(123)).ReturnsAsync(list);
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
            var list = new TvRequests
            {
                TvDbId = 123,
                ChildRequests = new List<ChildRequests>
                {
                    new ChildRequests()
                    {
                        Approved = true

                    }
                }
            };

            TvMock.Setup(x => x.GetRequest(123)).ReturnsAsync(list);
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
            var list = new TvRequests
            {
                TvDbId = 123,
                ChildRequests = new List<ChildRequests>
                {
                    new ChildRequests()
                    {
                        Approved = true

                    }
                }
            };


            TvMock.Setup(x => x.GetRequest(123)).ReturnsAsync(list);
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