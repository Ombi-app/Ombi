
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Tests.Rule.Search
{
    public class CouchPotatoCacheRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IExternalRepository<CouchPotatoCache>>();
            Rule = new CouchPotatoCacheRule(ContextMock.Object);

        }

        private CouchPotatoCacheRule Rule { get; set; }
        private Mock<IExternalRepository<CouchPotatoCache>> ContextMock { get; set; }

        [Test]
        public async Task Should_ReturnApproved_WhenMovieIsInCouchPotato()
        {
            var list = new CouchPotatoCache
            {
                TheMovieDbId = 123
            };
            
            ContextMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<CouchPotatoCache, bool>>>())).ReturnsAsync(list);

            var request = new SearchMovieViewModel { Id = 123 };
            var result =await  Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }


        [Test]
        public async Task Should_ReturnNotApproved_WhenMovieIsNotCouchPotato()
        {
            var list = DbHelper.GetQueryableMockDbSet(new CouchPotatoCache
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
