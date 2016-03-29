#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexAvailabilityCheckerTests.cs
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

using Moq;

using NUnit.Framework;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers.Exceptions;
using PlexRequests.Services.Interfaces;
using PlexRequests.Store;

namespace PlexRequests.Services.Tests
{
    [TestFixture]
    public class PlexAvailabilityCheckerTests
    {
        public IAvailabilityChecker Checker { get; set; }

        [Test]
        public void IsAvailableWithEmptySettingsTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();
            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            Assert.Throws<ApplicationSettingsException>(() => Checker.IsAvailable("title", "2013"), "We should be throwing an exception since we cannot talk to the services.");
        }

        [Test]
        public void IsAvailableTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();

            var searchResult = new PlexSearch { Video = new List<Video> { new Video { Title = "title", Year = "2011" } } };

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "abc" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            plexMock.Setup(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(searchResult);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            var result = Checker.IsAvailable("title", "2011");

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsAvailableDirectoryTitleTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();

            var searchResult = new PlexSearch { Directory = new Directory1 { Title = "title", Year = "2013" } };

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "abc" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            plexMock.Setup(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(searchResult);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            var result = Checker.IsAvailable("title", "2013");

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsAvailableDirectoryTitleWithoutYearTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();

            var searchResult = new PlexSearch { Directory = new Directory1 { Title = "title", } };

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "abc" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            plexMock.Setup(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(searchResult);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            var result = Checker.IsAvailable("title", null);

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsNotAvailableTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();

            var searchResult = new PlexSearch { Video = new List<Video> { new Video { Title = "wrong title", Year = "2011" } } };

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "abc" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            plexMock.Setup(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(searchResult);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            var result = Checker.IsAvailable("title", "2011");

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsNotAvailableTestWihtoutYear()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();

            var searchResult = new PlexSearch { Video = new List<Video> { new Video { Title = "wrong title" } } };

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "abc" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            plexMock.Setup(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(searchResult);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            var result = Checker.IsAvailable("title", null);

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsYearDoesNotMatchTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();

            var searchResult = new PlexSearch { Video = new List<Video> { new Video { Title = "title", Year = "2019" } } };

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "abc" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            plexMock.Setup(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(searchResult);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            var result = Checker.IsAvailable("title", "2011");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TitleDoesNotMatchTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();

            var searchResult = new PlexSearch { Video = new List<Video> { new Video { Title = "title23", Year = "2019" } } };

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "abc" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            plexMock.Setup(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(searchResult);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            var result = Checker.IsAvailable("title", "2019");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TitleDoesNotMatchWithoutYearTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();

            var searchResult = new PlexSearch { Video = new List<Video> { new Video { Title = "title23" } } };

            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "abc" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            plexMock.Setup(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>())).Returns(searchResult);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            var result = Checker.IsAvailable("title", null);

            Assert.That(result, Is.False);
        }


        [Test]
        public void CheckAndUpdateNoPlexSettingsTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            Checker.CheckAndUpdateAll(1);

            requestMock.Verify(x => x.BatchUpdate(It.IsAny<List<RequestedModel>>()), Times.Never);
            requestMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
            plexMock.Verify(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>()), Times.Never);
        }

        [Test]
        public void CheckAndUpdateNoAuthSettingsTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();
            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "123" });

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            Checker.CheckAndUpdateAll(1);

            requestMock.Verify(x => x.BatchUpdate(It.IsAny<List<RequestedModel>>()), Times.Never);
            requestMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
            plexMock.Verify(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>()), Times.Never);
        }

        [Test]
        public void CheckAndUpdateNoRequestsTest()
        {
            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();
            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "192.168.1.1" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            requestMock.Setup(x => x.GetAll()).Returns(new List<RequestedModel>());

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            Checker.CheckAndUpdateAll(1);

            requestMock.Verify(x => x.BatchUpdate(It.IsAny<List<RequestedModel>>()), Times.Never);
            requestMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
            plexMock.Verify(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>()), Times.Never);
        }


        [Test]
        [Ignore("Need to work out Plex Directory vs Video objects")]
        public void CheckAndUpdateRequestsTest()
        {

            var requests = new List<RequestedModel> {
                new RequestedModel
                {
                    Id = 123,
                    Title = "title1",
                    Available = false,
                },
                new RequestedModel
                {
                    Id=222,
                    Title = "title3",
                    Available = false
                },
                new RequestedModel
                {
                    Id = 333,
                    Title= "missingTitle",
                    Available = false
                },
                new RequestedModel
                {
                    Id= 444,
                    Title = "already found",
                    Available = true
                }
            };

            var search = new PlexSearch { };

            var settingsMock = new Mock<ISettingsService<PlexSettings>>();
            var authMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var requestMock = new Mock<IRequestService>();
            var plexMock = new Mock<IPlexApi>();
            settingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings { Ip = "192.168.1.1" });
            authMock.Setup(x => x.GetSettings()).Returns(new AuthenticationSettings { PlexAuthToken = "abc" });
            requestMock.Setup(x => x.GetAll()).Returns(requests);

            Checker = new PlexAvailabilityChecker(settingsMock.Object, authMock.Object, requestMock.Object, plexMock.Object);

            Checker.CheckAndUpdateAll(1);

            requestMock.Verify(x => x.BatchUpdate(It.IsAny<List<RequestedModel>>()), Times.Never);
            requestMock.Verify(x => x.Get(It.IsAny<int>()), Times.Never);
            plexMock.Verify(x => x.SearchContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Uri>()), Times.Never);
        }
    }
}