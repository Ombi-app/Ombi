using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using MockQueryable.Moq;
using NUnit.Framework;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Rules.Search;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;
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

        [Test]
        public async Task TvShow_ShouldNotCheckEpisodes_WhenSonarrPriorityEnabled()
        {
            var sonarrSettingsMock = new Mock<ISettingsService<SonarrSettings>>();
            sonarrSettingsMock.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = true });

            var rule = new EmbyAvailabilityRule(ContextMock.Object, LoggerMock.Object, FeatureMock.Object, null, sonarrSettingsMock.Object);

            var search = new SearchTvShowViewModel
            {
                TheTvDbId = "123",
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests { EpisodeNumber = 1 }
                        }
                    }
                }
            };

            ContextMock.Setup(x => x.GetByTvDbId("123"))
                .ReturnsAsync(new EmbyContent { Title = "Test Show", Type = MediaType.Series, TvDbId = "123" });

            var result = await rule.Execute(search);

            Assert.True(result.Success);
            Assert.False(search.Available);
            Assert.False(search.SeasonRequests[0].Episodes[0].Available);

            ContextMock.Verify(x => x.GetAllEpisodes(), Times.Never);
        }

        [Test]
        public async Task TvShow_ShouldCheckEpisodes_WhenSonarrPriorityDisabled()
        {
            var sonarrSettingsMock = new Mock<ISettingsService<SonarrSettings>>();
            sonarrSettingsMock.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new SonarrSettings { Enabled = true, ScanForAvailability = true, PrioritizeArrAvailability = false });

            var rule = new EmbyAvailabilityRule(ContextMock.Object, LoggerMock.Object, FeatureMock.Object, null, sonarrSettingsMock.Object);

            var search = new SearchTvShowViewModel
            {
                TheTvDbId = "123",
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests { EpisodeNumber = 1 }
                        }
                    }
                }
            };

            ContextMock.Setup(x => x.GetByTvDbId("123"))
                .ReturnsAsync(new EmbyContent { Title = "Test Show", Type = MediaType.Series, TvDbId = "123" });

            ContextMock.Setup(x => x.GetAllEpisodes()).Returns(new List<EmbyEpisode>
            {
                new EmbyEpisode
                {
                    EpisodeNumber = 1,
                    SeasonNumber = 1,
                    Series = new EmbyContent { Title = "Test Show", TvDbId = "123" }
                }
            }.AsQueryable().BuildMock());

            var result = await rule.Execute(search);

            Assert.True(result.Success);
            Assert.True(search.Available);
            Assert.True(search.SeasonRequests[0].Episodes[0].Available);

            ContextMock.Verify(x => x.GetAllEpisodes(), Times.Once);
        }
    }
}
