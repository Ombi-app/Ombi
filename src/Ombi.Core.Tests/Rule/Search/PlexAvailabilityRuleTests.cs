using System.Threading.Tasks;
using Moq;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Xunit;

namespace Ombi.Core.Tests.Rule.Search
{
    public class PlexAvailabilityRuleTests
    {
        public PlexAvailabilityRuleTests()
        {
            ContextMock = new Mock<IPlexContentRepository>();
            Rule = new PlexAvailabilityRule(ContextMock.Object);
        }

        private PlexAvailabilityRule Rule { get; }
        private Mock<IPlexContentRepository> ContextMock { get; }

        [Fact]
        public async Task ShouldBe_Available_WhenFoundInPlex()
        {
            ContextMock.Setup(x => x.Get(It.IsAny<string>())).ReturnsAsync(new PlexContent
            {
                Url = "TestUrl"
            });
            var search = new SearchMovieViewModel();
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.Equal(search.PlexUrl, "TestUrl");
            Assert.Equal(search.Available, true);
        }

        [Fact]
        public async Task ShouldBe_NotAvailable_WhenNotFoundInPlex()
        {
            ContextMock.Setup(x => x.Get(It.IsAny<string>())).Returns(Task.FromResult(default(PlexContent)));
            var search = new SearchMovieViewModel();
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.Equal(search.PlexUrl, null);
            Assert.Equal(search.Available, false);
        }
    }
}