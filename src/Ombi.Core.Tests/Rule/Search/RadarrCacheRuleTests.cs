using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Tests.Rule.Search
{
    public class RadarrCacheRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IExternalRepository<RadarrCache>>();
            Rule = new RadarrCacheRule(ContextMock.Object);

        }

        private RadarrCacheRule Rule { get; set; }
        private Mock<IExternalRepository<RadarrCache>> ContextMock { get; set; }

        [Test]
        public async Task Should_ReturnApproved_WhenMovieIsInRadarr()
        {
            var list = new List<RadarrCache>(){new RadarrCache
            {
                TheMovieDbId = 123
            }}.AsQueryable();
            
            ContextMock.Setup(x => x.GetAll()).Returns(list);

            var request = new SearchMovieViewModel { Id = 123 };
            var result =await  Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }


        [Test]
        public async Task Should_ReturnNotApproved_WhenMovieIsNotInRadarr()
        {
            var list = DbHelper.GetQueryableMockDbSet(new RadarrCache
            {
                TheMovieDbId = 000012
            });

            ContextMock.Setup(x => x.GetAll()).Returns(list);

            var request = new SearchMovieViewModel { Id = 123 };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }
    }
}
