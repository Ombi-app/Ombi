using System.Threading.Tasks;
using Moq;  
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Xunit;

namespace Ombi.Core.Tests.Rule.Search
{
    public class RadarrCacheRuleTests
    {
        public RadarrCacheRuleTests()
        {
            ContextMock = new Mock<IOmbiContext>();
            Rule = new RadarrCacheRule(ContextMock.Object);
        }

        private RadarrCacheRule Rule { get; }
        private Mock<IOmbiContext> ContextMock { get; }

        [Fact]
        public async Task Should_ReturnApproved_WhenMovieIsInRadarr()
        {
            var list = DbHelper.GetQueryableMockDbSet(new RadarrCache
            {
                TheMovieDbId = 123
            });

            ContextMock.Setup(x => x.RadarrCache).Returns(list);

            var request = new SearchMovieViewModel { Id = 123 };
            var result =await  Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }


        [Fact]
        public async Task Should_ReturnNotApproved_WhenMovieIsNotInRadarr()
        {
            var list = DbHelper.GetQueryableMockDbSet(new RadarrCache
            {
                TheMovieDbId = 000012
            });

            ContextMock.Setup(x => x.RadarrCache).Returns(list);

            var request = new SearchMovieViewModel { Id = 123 };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }
    }
}
