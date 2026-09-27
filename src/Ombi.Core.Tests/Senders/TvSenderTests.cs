using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.Sonarr;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Core.Senders;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Senders
{
    [TestFixture]
    public class TvSenderTests
    {
        private AutoMocker _mocker;
        private TvSender _subject;
        private Mock<ISonarrV3Api> _sonarr;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<TvSender>();
            _sonarr = _mocker.GetMock<ISonarrV3Api>();

            _mocker.GetMock<IRepository<UserQualityProfiles>>()
                .Setup(x => x.GetAll())
                .Returns(new List<UserQualityProfiles>().AsQueryable().BuildMock());
        }

        [TestCase(2, false)]
        [TestCase(1, true)]
        public async Task ExistingSeries_ProfileOverride_SearchesRequestedSeasonOnly(
            int existingProfileId,
            bool expectProfileUpdate)
        {
            var settings = new SonarrSettings
            {
                ApiKey = "key",
                Ip = "localhost",
                Port = 8989,
                RootPath = "1",
                QualityProfile = "1",
                AddOnly = false
            };

            var request = new ChildRequests
            {
                RequestedUserId = "user",
                SeriesType = SeriesType.Standard,
                QualityOverride = 2,
                ParentRequest = new TvRequests
                {
                    Title = "Test Show",
                    TvDbId = 123,
                    TotalSeasons = 1
                },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        SeasonNumber = 1,
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                EpisodeNumber = 1,
                                Title = "Pilot"
                            }
                        }
                    }
                }
            };

            var series = new SonarrSeries
            {
                id = 10,
                tvdbId = 123,
                qualityProfileId = existingProfileId,
                monitored = true,
                seasons = new[]
                {
                    new Season
                    {
                        seasonNumber = 1,
                        monitored = true
                    }
                }
            };

            _sonarr.Setup(x => x.GetRootFolders(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new SonarrRootFolder
                    {
                        id = 1,
                        path = "/tv"
                    }
                });
            _sonarr.Setup(x => x.GetSeries(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[] { series });
            _sonarr.Setup(x => x.UpdateSeries(It.IsAny<SonarrSeries>(), settings.ApiKey, settings.FullUri))
                .ReturnsAsync((SonarrSeries updated, string _, string __) => updated);
            _sonarr.Setup(x => x.GetEpisodes(series.id, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(new[]
                {
                    new Episode
                    {
                        id = 100,
                        seriesId = series.id,
                        seasonNumber = 1,
                        episodeNumber = 1,
                        title = "Pilot",
                        monitored = true
                    }
                });
            _sonarr.Setup(x => x.SeasonSearch(series.id, 1, settings.ApiKey, settings.FullUri))
                .ReturnsAsync(true);

            await _subject.SendToSonarr(request, settings);

            _sonarr.Verify(
                x => x.UpdateSeries(
                    It.Is<SonarrSeries>(updated => updated.qualityProfileId == 2),
                    settings.ApiKey,
                    settings.FullUri),
                expectProfileUpdate ? Times.Once() : Times.Never());
            _sonarr.Verify(
                x => x.SeasonSearch(series.id, 1, settings.ApiKey, settings.FullUri),
                Times.Once);
            _sonarr.Verify(
                x => x.SeriesSearch(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
