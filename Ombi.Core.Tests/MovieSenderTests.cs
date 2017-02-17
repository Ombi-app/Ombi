#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: MovieSenderTests.cs
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
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Api;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Radarr;
using Ombi.Api.Models.Watcher;
using Ombi.Core.SettingModels;
using Ombi.Store;
using Ploeh.AutoFixture;

namespace Ombi.Core.Tests
{
    public class MovieSenderTests
    {
        private MovieSender Sender { get; set; }
        private Mock<ISettingsService<CouchPotatoSettings>> CpMock { get; set; }
        private Mock<ISettingsService<WatcherSettings>> WatcherMock { get; set; }
        private Mock<ISettingsService<RadarrSettings>> RadarrMock { get; set; }
        private Mock<ICouchPotatoApi> CpApiMock { get; set; }
        private Mock<IWatcherApi> WatcherApiMock { get; set; }
        private Mock<IRadarrApi> RadarrApiMock { get; set; }

        private Fixture F { get; set; }

        [SetUp]
        public void Setup()
        {
            F = new Fixture();
            CpMock = new Mock<ISettingsService<CouchPotatoSettings>>();
            WatcherMock = new Mock<ISettingsService<WatcherSettings>>();
            RadarrApiMock = new Mock<IRadarrApi>();
            RadarrMock = new Mock<ISettingsService<RadarrSettings>>();
            CpApiMock = new Mock<ICouchPotatoApi>();
            WatcherApiMock = new Mock<IWatcherApi>();

            RadarrMock.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(F.Build<RadarrSettings>().With(x => x.Enabled, false).Create());
            WatcherMock.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(F.Build<WatcherSettings>().With(x => x.Enabled, false).Create());
            CpMock.Setup(x => x.GetSettingsAsync())
                 .ReturnsAsync(F.Build<CouchPotatoSettings>().With(x => x.Enabled, false).Create());

            Sender = new MovieSender(CpMock.Object, WatcherMock.Object, CpApiMock.Object, WatcherApiMock.Object, RadarrApiMock.Object, RadarrMock.Object);    
        }

        [Test]
        public async Task SendRadarrMovie()
        {
            RadarrMock.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(F.Build<RadarrSettings>().With(x => x.Enabled, true).Create());
            RadarrApiMock.Setup(x => x.AddMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Uri>(), It.IsAny<bool>())).Returns(new RadarrAddMovie { title = "Abc" });

            var model = F.Create<RequestedModel>();

            var result = await Sender.Send(model, 2.ToString());


            Assert.That(result.Result, Is.True);
            Assert.That(result.Error, Is.False);
            Assert.That(result.MovieSendingEnabled, Is.True);

            RadarrApiMock.Verify(x => x.AddMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), 2, It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Uri>(), It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public async Task SendRadarrMovie_SendingFailed()
        {
            RadarrMock.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(F.Build<RadarrSettings>().With(x => x.Enabled, true).Create());
            RadarrApiMock.Setup(x => x.AddMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Uri>(), It.IsAny<bool>())).Returns(new RadarrAddMovie {  Error = new RadarrError{message = "Movie Already Added"}});

            var model = F.Create<RequestedModel>();

            var result = await Sender.Send(model, 2.ToString());


            Assert.That(result.Result, Is.False);
            Assert.That(result.Error, Is.True);
            Assert.That(result.MovieSendingEnabled, Is.True);

            RadarrApiMock.Verify(x => x.AddMovie(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), 2, It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Uri>(), It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public async Task SendCpMovie()
        {
            CpMock.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(F.Build<CouchPotatoSettings>().With(x => x.Enabled, true).Create());
            CpApiMock.Setup(x => x.AddMovie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Uri>(), It.IsAny<string>())).Returns(true);

            var model = F.Create<RequestedModel>();

            var result = await Sender.Send(model);

            Assert.That(result.Result, Is.True);
            Assert.That(result.Error, Is.False);
            Assert.That(result.MovieSendingEnabled, Is.True);

            CpApiMock.Verify(x => x.AddMovie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task SendWatcherMovie()
        {
            WatcherMock.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(F.Build<WatcherSettings>().With(x => x.Enabled, true).Create());
            WatcherApiMock.Setup(x => x.AddMovie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(F.Create<WatcherAddMovieResult>());

            var model = F.Create<RequestedModel>();

            var result = await Sender.Send(model);

            Assert.That(result.Result, Is.True);
            Assert.That(result.Error, Is.False);
            Assert.That(result.MovieSendingEnabled, Is.True);

            WatcherApiMock.Verify(x => x.AddMovie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>()), Times.Once);
        }
    }
}