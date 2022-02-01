using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
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
            SettingsMock = new Mock<ISettingsService<EmbySettings>>();
            Rule = new EmbyAvailabilityRule(ContextMock.Object, LoggerMock.Object, SettingsMock.Object);
        }

        private EmbyAvailabilityRule Rule { get; set; }
        private Mock<IEmbyContentRepository> ContextMock { get; set; }
        private Mock<ILogger<EmbyAvailabilityRule>> LoggerMock { get; set; }
        private Mock<ISettingsService<EmbySettings>> SettingsMock { get; set; }

        [Test]
        public async Task Movie_ShouldBe_Available_WhenFoundInEmby()
        {
            SettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new EmbySettings());
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new EmbyContent
            {
                ProviderId = "123"
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
