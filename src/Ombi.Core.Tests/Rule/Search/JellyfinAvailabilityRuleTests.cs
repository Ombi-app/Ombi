using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class JellyfinAvailabilityRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IJellyfinContentRepository>();
            SettingsMock = new Mock<ISettingsService<JellyfinSettings>>();
            Rule = new JellyfinAvailabilityRule(ContextMock.Object, SettingsMock.Object);
        }

        private JellyfinAvailabilityRule Rule { get; set; }
        private Mock<IJellyfinContentRepository> ContextMock { get; set; }
        private Mock<ISettingsService<JellyfinSettings>> SettingsMock { get; set; }

        [Test]
        public async Task Movie_ShouldBe_Available_WhenFoundInJellyfin()
        {
            SettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new JellyfinSettings());
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new JellyfinContent
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
        public async Task Movie_Has_Custom_Url_When_Specified_In_Settings()
        {
            SettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new JellyfinSettings
            {
                Enable = true,
                Servers = new List<JellyfinServers>
                {
                    new JellyfinServers
                    {
                        ServerHostname = "http://test.com/",
                        ServerId = "8"
                    }
                }
            });
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new JellyfinContent
            {
                ProviderId = "123",
                JellyfinId = 1.ToString(),
            });
            var search = new SearchMovieViewModel()
            {
                TheMovieDbId = "123",
            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.That(search.JellyfinUrl, Is.EqualTo("http://test.com/web/index.html#!/details?id=1&serverId=8"));
        }

        [Test]
        public async Task Movie_Uses_Default_Url_When()
        {
            SettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new JellyfinSettings
            {
                Enable = true,
                Servers = new List<JellyfinServers>
                {
                    new JellyfinServers
                    {
                        Ip = "8080",
                        Port = 9090,
                        ServerHostname = string.Empty,
                        ServerId = "8"
                    }
                }
            });
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new JellyfinContent
            {
                ProviderId = "123",
                JellyfinId = 1.ToString()
            });
            var search = new SearchMovieViewModel()
            {
                TheMovieDbId = "123",
            };
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Movie_ShouldBe_NotAvailable_WhenNotFoundInJellyfin()
        {
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).Returns(Task.FromResult(default(JellyfinContent)));
            var search = new SearchMovieViewModel();
            var result = await Rule.Execute(search);

            Assert.True(result.Success);
            Assert.False(search.Available);
        }
    }
}
