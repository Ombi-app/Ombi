using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Rule.Search
{
    public class EmbyAvailabilityRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IEmbyContentRepository>();
            LoggerMock = new Mock<ILogger<EmbyAvailabilityRule>>();
            FeatureMock = new Mock<IFeatureService>();
            Rule = new EmbyAvailabilityRule(ContextMock.Object, LoggerMock.Object, FeatureMock.Object);
        }

        private EmbyAvailabilityRule Rule { get; set; }
        private Mock<IEmbyContentRepository> ContextMock { get; set; }
        private Mock<ILogger<EmbyAvailabilityRule>> LoggerMock { get; set; }
        private Mock<IFeatureService> FeatureMock { get; set; }

        [Test]
        public async Task Movie_ShouldBe_Available_WhenFoundInEmby()
        {
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new EmbyContent
            {
                TheMovieDbId = "123",
                Quality = "1"
            });
            var search = new SearchMovieViewModel()
            {
                TheMovieDbId = "123",
            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.True(search.Available);
        }

        [Test]
        public async Task Movie_ShouldBe_Available_WhenFoundInEmby_4K()
        {
            FeatureMock.Setup(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(true);
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new EmbyContent
            {
                TheMovieDbId = "123",
                Has4K = true
            });
            var search = new SearchMovieViewModel()
            {
                TheMovieDbId = "123",
            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.True(search.Available4K);
            Assert.False(search.Available);
        }

        [Test]
        public async Task Movie_ShouldBe_Available_WhenFoundInEmby_Both()
        {
            FeatureMock.Setup(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(true);
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new EmbyContent
            {
                TheMovieDbId = "123",
                Has4K = true,
                Quality = "1"
            });
            var search = new SearchMovieViewModel()
            {
                TheMovieDbId = "123",
            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.True(search.Available4K);
            Assert.True(search.Available);
        }

        [Test]
        public async Task Movie_ShouldBe_NotAvailable_WhenNotFoundInEmby()
        {
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).Returns(Task.FromResult(default(EmbyContent)));
            var search = new SearchMovieViewModel();
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.False(search.Available);
        }
    }
}
