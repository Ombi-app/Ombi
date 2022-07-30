﻿using System.Collections.Generic;
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
    public class JellyfinAvailabilityRuleTests
    {
        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IJellyfinContentRepository>();
            LoggerMock = new Mock<ILogger<JellyfinAvailabilityRule>>();
            FeatureMock = new Mock<IFeatureService>();
            Rule = new JellyfinAvailabilityRule(ContextMock.Object, LoggerMock.Object, FeatureMock.Object);
        }

        private JellyfinAvailabilityRule Rule { get; set; }
        private Mock<IJellyfinContentRepository> ContextMock { get; set; }
        private Mock<ILogger<JellyfinAvailabilityRule>> LoggerMock { get; set; }
        private Mock<IFeatureService> FeatureMock { get; set; }

        [Test]
        public async Task Movie_ShouldBe_Available_WhenFoundInJellyfin()
        {
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new JellyfinContent
            {
                TheMovieDbId = "123",
                Quality = "1080"
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
        public async Task Movie_ShouldBe_Available_WhenFoundInJellyfin_4K()
        {
            FeatureMock.Setup(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(true);
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new JellyfinContent
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
            Assert.False(search.Available);
            Assert.True(search.Available4K);
        }

        [Test]
        public async Task Movie_ShouldBe_Available_WhenFoundInJellyfin_Both()
        {
            FeatureMock.Setup(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(true);
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new JellyfinContent
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
            Assert.True(search.Available);
            Assert.True(search.Available4K);
        }

        [Test]
        public async Task Movie_Uses_Default_Url_When()
        {
            ContextMock.Setup(x => x.GetByTheMovieDbId(It.IsAny<string>())).ReturnsAsync(new JellyfinContent
            {
                TheMovieDbId = "123",
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
