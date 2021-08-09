using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Ombi.Api.Lidarr;
using Ombi.Api.Lidarr.Models;
using Ombi.Core.Engine.V2;
using Ombi.Core.Models.Requests;
using Ombi.Core.Models.Search.V2.Music;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine.V2
{
    [TestFixture]
    public class MusicSearchEngineV2Tests
    {

        private MusicSearchEngineV2 _engine;

        // private Mock<ILidarrApi> _musicApi;
        private Mock<ILidarrApi> _lidarrApi;
        private Mock<ISettingsService<LidarrSettings>> _lidarrSettings;
        private Fixture F;

        [SetUp]
        public void Setup()
        {
            F = new Fixture();
            F.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => F.Behaviors.Remove(b));
            F.Behaviors.Add(new OmitOnRecursionBehavior());

            var principle = new Mock<IPrincipal>();
            var requestService = new Mock<IRequestServiceMain>();
            var ruleEval = new Mock<IRuleEvaluator>();
            var um = MockHelper.MockUserManager(new List<OmbiUser>());
            var cache = new Mock<ICacheService>();
            var ombiSettings = new Mock<ISettingsService<OmbiSettings>>();
            var requestSub = new Mock<IRepository<RequestSubscription>>();
            _lidarrSettings = new Mock<ISettingsService<LidarrSettings>>();
            _lidarrApi = new Mock<ILidarrApi>();
            _lidarrSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new LidarrSettings());
            _engine = new MusicSearchEngineV2(principle.Object, requestService.Object, ruleEval.Object,
                um.Object, cache.Object, ombiSettings.Object, requestSub.Object,
                _lidarrSettings.Object, _lidarrApi.Object);
        }

        [Test]
        public async Task GetArtistInformation_WithPosters()
        {
            _lidarrSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new LidarrSettings
            {
                Enabled = true,
                ApiKey = "dasdsa",
                Ip = "192.168.1.7"
            });
            _lidarrApi.Setup(x => x.GetArtistByForeignId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None))
                .ReturnsAsync(new ArtistResult
                {
                    images = new Image[]
                    {
                        new Image
                        {
                            coverType = "poster",
                            url = "posterUrl"
                        },
                        new Image
                        {
                            coverType = "logo",
                            url = "logoUrl"
                        },
                        new Image
                        {
                            coverType = "banner",
                            url = "bannerUrl"
                        },
                        new Image
                        {
                            coverType = "fanArt",
                            url = "fanartUrl"
                        },
                    }
                });

            var result = await _engine.GetArtistInformation("pretend-artist-id");

            Assert.That(result.Banner, Is.EqualTo("bannerUrl"));
            Assert.That(result.Poster, Is.EqualTo("posterUrl"));
            Assert.That(result.Logo, Is.EqualTo("logoUrl"));
            Assert.That(result.FanArt, Is.EqualTo("fanartUrl"));
        }
    }
}