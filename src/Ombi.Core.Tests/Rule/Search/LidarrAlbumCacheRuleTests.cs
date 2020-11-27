
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
    public class LidarrAlbumCacheRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IExternalRepository<LidarrAlbumCache>>();
            Rule = new LidarrAlbumCacheRule(ContextMock.Object);

        }

        private LidarrAlbumCacheRule Rule { get; set; }
        private Mock<IExternalRepository<LidarrAlbumCache>> ContextMock { get; set; }

        [Test]
        public async Task Should_Not_Be_Monitored_Or_Available()
        {
            var request = new SearchAlbumViewModel { ForeignAlbumId = "abc" };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
            Assert.False(request.Monitored);
        }

        [Test]
        public async Task Should_Be_Monitored_But_Not_Available()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<LidarrAlbumCache>
            {
                new LidarrAlbumCache
                {
                    ForeignAlbumId = "abc",
                    PercentOfTracks = 0
                }
            }.AsQueryable());
            var request = new SearchAlbumViewModel { ForeignAlbumId = "abc" };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
            Assert.True(request.Monitored);
            Assert.That(request.PartiallyAvailable, Is.EqualTo(false));
            Assert.That(request.Available, Is.EqualTo(false));
            Assert.That(request.FullyAvailable, Is.EqualTo(false));
        }

        [Test]
        public async Task Should_Be_Monitored_And_Partly_Available()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<LidarrAlbumCache>
            {
                new LidarrAlbumCache
                {
                    ForeignAlbumId = "abc",
                    PercentOfTracks = 1
                }
            }.AsQueryable());
            var request = new SearchAlbumViewModel { ForeignAlbumId = "abc" };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
            Assert.True(request.Monitored);
            Assert.That(request.PartiallyAvailable, Is.EqualTo(true));
            Assert.That(request.Available, Is.EqualTo(false));
            Assert.That(request.FullyAvailable, Is.EqualTo(false));
        }

        [Test]
        public async Task Should_Be_Monitored_And_Fully_Available()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<LidarrAlbumCache>
            {
                new LidarrAlbumCache
                {
                    ForeignAlbumId = "abc",
                    PercentOfTracks = 100
                }
            }.AsQueryable());
            var request = new SearchAlbumViewModel { ForeignAlbumId = "abc" };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
            Assert.True(request.Monitored);
            Assert.That(request.PartiallyAvailable, Is.EqualTo(false));
            Assert.That(request.FullyAvailable, Is.EqualTo(true));
        }

        [Test]
        public async Task Should_Be_Monitored_And_Fully_Available_Casing()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<LidarrAlbumCache>
            {
                new LidarrAlbumCache
                {
                    ForeignAlbumId = "abc",
                    PercentOfTracks = 100
                }
            }.AsQueryable());
            var request = new SearchAlbumViewModel { ForeignAlbumId = "abc" };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
            Assert.True(request.Monitored);
            Assert.That(request.PartiallyAvailable, Is.EqualTo(false));
            Assert.That(request.FullyAvailable, Is.EqualTo(true));
        }
    }
}
