using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Tests.Rule.Search
{
    public class PlexAvailabilityRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IPlexContentRepository>();
            Rule = new PlexAvailabilityRule(ContextMock.Object);
        }

        private PlexAvailabilityRule Rule { get; set; }
        private Mock<IPlexContentRepository> ContextMock { get; set; }

        [Test]
        public async Task ShouldBe_Available_WhenFoundInPlex()
        {
            ContextMock.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(new PlexContent
            {
                Url = "TestUrl"
            });
            var search = new SearchMovieViewModel();
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.AreEqual("TestUrl", search.PlexUrl);
            Assert.True(search.Available);
        }

        [Test]
        public async Task ShouldBe_NotAvailable_WhenNotFoundInPlex()
        {
            ContextMock.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult(default(PlexContent)));
            var search = new SearchMovieViewModel();
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.Null(search.PlexUrl);
            Assert.False(search.Available);
        }
    }
}