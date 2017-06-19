using System.Threading.Tasks;
using Moq;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Requests.Movie;
using Ombi.Core.Models.Search;
using Ombi.Core.Requests.Models;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
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

        // TODO continue tests
        // https://stackoverflow.com/questions/27483709/testing-ef-async-methods-with-sync-methods-with-moq
        public async Task ShouldBe_Requested_WhenExisitngMovie()
        {
            var list = DbHelper.GetQueryableMockDbSet(new MovieRequestModel
            {
                ProviderId = 123,
                Approved = true
            });
            MovieMock.Setup(x => x.GetAllQueryable()).Returns(list);
            var search = new SearchMovieViewModel
            {
                Id = 123,
                
            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.Equal(search.Approved, true);
        }


    }
}