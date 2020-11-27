
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
    public class LidarrArtistCacheRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IExternalRepository<LidarrArtistCache>>();
            Rule = new LidarrArtistCacheRule(ContextMock.Object);
        }

        private LidarrArtistCacheRule Rule { get; set; }
        private Mock<IExternalRepository<LidarrArtistCache>> ContextMock { get; set; }

        [Test]
        public async Task Should_Not_Be_Monitored()
        {
            var request = new SearchArtistViewModel { ForignArtistId = "abc" };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Monitored);
        }

        [Test]
        public async Task Should_Be_Monitored()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<LidarrArtistCache>
            {
                new LidarrArtistCache
                {
                    ForeignArtistId = "abc",
                }
            }.AsQueryable());
            var request = new SearchArtistViewModel { ForignArtistId = "abc" };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Monitored);
        }


        [Test]
        public async Task Should_Be_Monitored_Casing()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<LidarrArtistCache>
            {
                new LidarrArtistCache
                {
                    ForeignArtistId = "abc",
                }
            }.AsQueryable());
            var request = new SearchArtistViewModel { ForignArtistId = "abc" };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Monitored);
        }

    }
}
