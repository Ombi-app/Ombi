using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using AutoFixture;
using Hqub.MusicBrainz.API.Entities;
using Moq;
using NUnit.Framework;
using Ombi.Api.Lidarr;
using Ombi.Api.Lidarr.Models;
using Ombi.Api.MusicBrainz;
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
using Artist = Hqub.MusicBrainz.API.Entities.Artist;

namespace Ombi.Core.Tests.Engine.V2
{
    [TestFixture]
    public class MusicSearchEngineV2Tests
    {

        private MusicSearchEngineV2 _engine;

        private Mock<IMusicBrainzApi> _musicApi;
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
            _musicApi = new Mock<IMusicBrainzApi>();
            _lidarrSettings = new Mock<ISettingsService<LidarrSettings>>();
            _lidarrApi = new Mock<ILidarrApi>();
            _lidarrSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new LidarrSettings());
            _engine = new MusicSearchEngineV2(principle.Object, requestService.Object, ruleEval.Object,
                um.Object, cache.Object, ombiSettings.Object, requestSub.Object, _musicApi.Object,
                _lidarrSettings.Object, _lidarrApi.Object);
        }


        [Test]
        public async Task GetBasicArtistInformation_SingleArtist_Test()
        {
            _musicApi.Setup(x => x.GetArtistInformation("pretend-artist-id")).ReturnsAsync(F.Create<Artist>());

            var result = await _engine.GetArtistInformation("pretend-artist-id");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReleaseGroups.Any(), Is.True, "Release Groups are null");
            Assert.That(result.Members.Any(), Is.False, "Members somehow populated?");
        }

        [Test]
        public async Task GetBasicArtistInformation_Group_Test()
        {
            var musicReturnVal = F.Build<Artist>().With(x => x.Relations, new List<Relation>
            {
                new Relation
                {
                    TypeId = RelationLinks.BandMember,
                    Artist = new Artist
                    {
                        Name = "Mr Artist"
                    },
                    Attributes = new []{"a nobody"},
                    Begin = "1992",
                    End = "2019",
                    Ended = true
                },
                new Relation
                {
                    TypeId = RelationLinks.BandMember,
                    Artist = new Artist
                    {
                        Name = "Mr Artist2"
                    },
                    Attributes = new []{"a nobody2"},
                    Begin = "1993",
                }
            });
            _musicApi.Setup(x => x.GetArtistInformation("pretend-artist-id")).ReturnsAsync(musicReturnVal.Create());

            var result = await _engine.GetArtistInformation("pretend-artist-id");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReleaseGroups.Any(), Is.True, "Release Groups are null");
            Assert.That(result.Members.Any(), Is.True, "Members IS NULL!");

            Assert.That(result.Members.FirstOrDefault(x => x.Name == "Mr Artist").End, Is.EqualTo("2019"));
            Assert.That(result.Members.FirstOrDefault(x => x.Name == "Mr Artist").Attributes.Length, Is.EqualTo(1));
            Assert.That(result.Members.FirstOrDefault(x => x.Name == "Mr Artist").IsCurrentMember, Is.EqualTo(false));
            Assert.That(result.Members.FirstOrDefault(x => x.Name == "Mr Artist").Start, Is.EqualTo("1992"));

            Assert.That(result.Members.FirstOrDefault(x => x.Name == "Mr Artist2").IsCurrentMember, Is.EqualTo(true));
        }

        [TestCaseSource(nameof(LinksData))]
        public async Task<string> GetBasicArtistInformation_Links_Test(string url, string typeId, Func<ArtistInformation, string> func)
        {
            var musicReturnVal = F.Build<Artist>().With(x => x.Relations, new List<Relation>
            {
                new Relation
                {
                    TypeId = typeId,
                    Url = new Url
                    {
                        Resource = url
                    }
                },

            });
            _musicApi.Setup(x => x.GetArtistInformation("pretend-artist-id")).ReturnsAsync(musicReturnVal.Create());

            var result = await _engine.GetArtistInformation("pretend-artist-id");

            Assert.That(result, Is.Not.Null);
            return func(result);
        }

        private static IEnumerable<TestCaseData> LinksData
        {
            get
            {
                yield return new TestCaseData("twitter.com", RelationLinks.SocialNetwork, new Func<ArtistInformation, string>(artist => artist.Links.Twitter)).Returns("twitter.com").SetName("ArtistInformation_Links_Twitter");
                yield return new TestCaseData("allmusic", RelationLinks.AllMusic, new Func<ArtistInformation, string>(artist => artist.Links.AllMusic)).Returns("allmusic").SetName("ArtistInformation_Links_AllMusic");
                yield return new TestCaseData("bbcmusic", RelationLinks.BbcMusic, new Func<ArtistInformation, string>(artist => artist.Links.BbcMusic)).Returns("bbcmusic").SetName("ArtistInformation_Links_BbcMusic");
                yield return new TestCaseData("discogs", RelationLinks.Discogs, new Func<ArtistInformation, string>(artist => artist.Links.Discogs)).Returns("discogs").SetName("ArtistInformation_Links_Discogs");
                yield return new TestCaseData("homepage", RelationLinks.Homepage, new Func<ArtistInformation, string>(artist => artist.Links.HomePage)).Returns("homepage").SetName("ArtistInformation_Links_Homepage");
                yield return new TestCaseData("imdb", RelationLinks.Imdb, new Func<ArtistInformation, string>(artist => artist.Links.Imdb)).Returns("imdb").SetName("ArtistInformation_Links_Imdb");
                yield return new TestCaseData("lastfm", RelationLinks.LastFm, new Func<ArtistInformation, string>(artist => artist.Links.LastFm)).Returns("lastfm").SetName("ArtistInformation_Links_LastFm");
                yield return new TestCaseData("myspace", RelationLinks.MySpace, new Func<ArtistInformation, string>(artist => artist.Links.MySpace)).Returns("myspace").SetName("ArtistInformation_Links_MySpace");
                yield return new TestCaseData("onlinecommunity", RelationLinks.OnlineCommunity, new Func<ArtistInformation, string>(artist => artist.Links.OnlineCommunity)).Returns("onlinecommunity").SetName("ArtistInformation_Links_OnlineCommunity");
                yield return new TestCaseData("www.facebook.com", RelationLinks.SocialNetwork, new Func<ArtistInformation, string>(artist => artist.Links.Facebook)).Returns("www.facebook.com").SetName("ArtistInformation_Links_Facebook");
                yield return new TestCaseData("www.instagram.com", RelationLinks.SocialNetwork, new Func<ArtistInformation, string>(artist => artist.Links.Instagram)).Returns("www.instagram.com").SetName("ArtistInformation_Links_insta");
                yield return new TestCaseData("www.vk.com", RelationLinks.SocialNetwork, new Func<ArtistInformation, string>(artist => artist.Links.Vk)).Returns("www.vk.com").SetName("ArtistInformation_Links_vk");
                yield return new TestCaseData("app.spotify.com", RelationLinks.Streams, new Func<ArtistInformation, string>(artist => artist.Links.Spotify)).Returns("app.spotify.com").SetName("ArtistInformation_Links_Spotify");
                yield return new TestCaseData("deezer.com", RelationLinks.Streams, new Func<ArtistInformation, string>(artist => artist.Links.Deezer)).Returns("deezer.com").SetName("ArtistInformation_Links_Deezer");
                yield return new TestCaseData("play.google.com", RelationLinks.Download, new Func<ArtistInformation, string>(artist => artist.Links.Google)).Returns("play.google.com").SetName("ArtistInformation_Links_Google");
                yield return new TestCaseData("itunes.apple.com", RelationLinks.Download, new Func<ArtistInformation, string>(artist => artist.Links.Apple)).Returns("itunes.apple.com").SetName("ArtistInformation_Links_Apple");
            }
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
            _musicApi.Setup(x => x.GetArtistInformation("pretend-artist-id")).ReturnsAsync(F.Create<Artist>());

            var result = await _engine.GetArtistInformation("pretend-artist-id");

            Assert.That(result.Banner, Is.EqualTo("bannerUrl"));
            Assert.That(result.Poster, Is.EqualTo("posterUrl"));
            Assert.That(result.Logo, Is.EqualTo("logoUrl"));
            Assert.That(result.FanArt, Is.EqualTo("fanartUrl"));
        }
    }
}