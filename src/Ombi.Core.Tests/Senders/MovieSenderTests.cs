using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.Radarr;
using Ombi.Api.External.ExternalApis.Radarr.Models;
using Ombi.Core.Senders;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;

namespace Ombi.Core.Tests.Senders
{
    [TestFixture]
    public class MovieSenderTests
    {
        [TestCase(false, 2)]
        [TestCase(true, 4)]
        public async Task Send_SelectsMatchingQualityOverride(bool is4K, int expectedQuality)
        {
            var mocker = new AutoMocker();
            var standard = Settings("standard", "2");
            var ultraHd = new Radarr4KSettings
            {
                Enabled = true,
                ApiKey = "4k",
                Ip = "radarr",
                Port = 7878,
                DefaultQualityProfile = "4",
                DefaultRootPath = "/movies"
            };
            mocker.GetMock<ISettingsService<RadarrSettings>>().Setup(x => x.GetSettingsAsync()).ReturnsAsync(standard);
            mocker.GetMock<ISettingsService<Radarr4KSettings>>().Setup(x => x.GetSettingsAsync()).ReturnsAsync(ultraHd);
            mocker.GetMock<ISettingsService<CouchPotatoSettings>>().Setup(x => x.GetSettingsAsync()).ReturnsAsync(new CouchPotatoSettings());
            mocker.GetMock<IRepository<UserQualityProfiles>>().Setup(x => x.GetAll())
                .Returns(Array.Empty<UserQualityProfiles>().AsQueryable().BuildMock());
            mocker.GetMock<IRadarrV3Api>().Setup(x => x.GetMovies(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<MovieResponse>());
            mocker.GetMock<IRadarrV3Api>().Setup(x => x.AddMovie(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<List<int>>()))
                .ReturnsAsync(new RadarrAddMovie());
            var request = new MovieRequests
            {
                TheMovieDbId = 1,
                Title = "Movie",
                ReleaseDate = new DateTime(2020, 1, 1),
                QualityOverride = 2,
                QualityOverride4K = 4
            };

            await mocker.CreateInstance<MovieSender>().Send(request, is4K);

            mocker.GetMock<IRadarrV3Api>().Verify(x => x.AddMovie(
                1, "Movie", 2020, expectedQuality, "/movies", is4K ? "4k" : "standard", It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<List<int>>()), Times.Once);
        }

        private static RadarrSettings Settings(string apiKey, string defaultQualityProfile) => new RadarrSettings
        {
            Enabled = true,
            ApiKey = apiKey,
            Ip = "radarr",
            Port = 7878,
            DefaultQualityProfile = defaultQualityProfile,
            DefaultRootPath = "/movies"
        };
    }
}
