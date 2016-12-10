#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: TvSenderTests.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Moq;

using NUnit.Framework;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Sonarr;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Store;
using PlexRequests.UI.Helpers;

using Ploeh.AutoFixture;

namespace PlexRequests.UI.Tests
{
    [TestFixture]
    public class TvSenderTests
    {

        private Mock<ISonarrApi> SonarrMock { get; set; }
        private Mock<ISickRageApi> SickrageMock { get; set; }

        private TvSender Sender { get; set; }
        private Fixture F { get; set; }

        [SetUp]
        public void Setup()
        {
            F = new Fixture();
            SonarrMock = new Mock<ISonarrApi>();
            SickrageMock = new Mock<ISickRageApi>();
            Sender = new TvSender(SonarrMock.Object, SickrageMock.Object);
        }

        [Test]
        [Ignore("Needs work")]
        public async Task HappyPathSendSeriesToSonarrAllSeason()
        {
            var seriesResult = new SonarrAddSeries() { title = "ABC"};
            SonarrMock.Setup(x => x.GetSeries(It.IsAny<string>(), It.IsAny<Uri>())).Returns(F.Build<Series>().With(x => x.tvdbId, 1).With(x => x.title, "ABC").CreateMany().ToList());

            Sender = new TvSender(SonarrMock.Object, SickrageMock.Object);

            var request = new RequestedModel {SeasonsRequested = "All", ProviderId = 1, Title = "ABC"};

            var result = await Sender.SendToSonarr(GetSonarrSettings(), request);

            Assert.That(result.title, Is.EqualTo("ABC"));
            SonarrMock.Verify(x => x.AddSeries(It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string>(),
                    It.IsAny<Uri>(),
                    true,  It.IsAny<bool>()), Times.Never);
        }

        [Test]
        [Ignore("Needs rework")]
        public async Task HappyPathSendEpisodeWithExistingSeriesToSonarr()
        {
            var seriesResult = new SonarrAddSeries { monitored = true, title = "TitleReturned" };
            var selectedSeries = F.Build<Series>().With(x => x.tvdbId, 1).CreateMany();
            SonarrMock.Setup(x => x.GetSeries(It.IsAny<string>(), It.IsAny<Uri>())).Returns(selectedSeries.ToList());
            SonarrMock.Setup(x => x.AddSeries(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string>(),
                    It.IsAny<Uri>(),
                    It.IsAny<bool>(), It.IsAny<bool>())).Returns(seriesResult);
            var sonarrEpisodes = new SonarrEpisodes()
            {
                title = "abc",
                seasonNumber = 2,
                episodeNumber = 1,
                monitored = false
            };
            var episodesList = F.CreateMany<SonarrEpisodes>().ToList();
            episodesList.Add(sonarrEpisodes);

            SonarrMock.Setup(x => x.GetEpisodes(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Uri>())).Returns(F.CreateMany<SonarrEpisodes>());

            Sender = new TvSender(SonarrMock.Object, SickrageMock.Object);
            var episodes = new List<EpisodesModel>
            {
                new EpisodesModel
                {
                    EpisodeNumber = 1,
                    SeasonNumber = 2
                }
            };
            var model = F.Build<RequestedModel>().With(x => x.ProviderId, 1)
                .With(x => x.Episodes, episodes).Create();

            var result = await Sender.SendToSonarr(GetSonarrSettings(), model, "2");

            Assert.That(result, Is.EqualTo(seriesResult));
            SonarrMock.Verify(x => x.AddSeries(It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int[]>(),
                    It.IsAny<string>(),
                    It.IsAny<Uri>(),
                    true, It.IsAny<bool>()), Times.Once);
        }


        private SonarrSettings GetSonarrSettings()
        {
            var sonarrSettings = new SonarrSettings
            {
                ApiKey = "abc",
                Enabled = true,
                Ip = "192.168.1.1",
                Port = 8989,
            };
            return sonarrSettings;
        }
    }
}