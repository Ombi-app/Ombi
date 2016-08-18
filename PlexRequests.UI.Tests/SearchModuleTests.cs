#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: SearchModuleTests.cs
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
using Moq;
using NUnit.Framework;
using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Helpers.Analytics;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Jobs;
using PlexRequests.Store;
using PlexRequests.Store.Models;
using PlexRequests.Store.Repository;
using PlexRequests.UI.Modules;
using Ploeh.AutoFixture;

namespace PlexRequests.UI.Tests
{
    [TestFixture]
    public class SearchModuleTests
    {
        private Mock<ISettingsService<HeadphonesSettings>> _headphonesSettings;
        private Mock<INotificationService> _notificationService;
        private Mock<ISettingsService<SickRageSettings>> _sickRageSettingsMock;
        private Mock<ICouchPotatoApi> _cpApi;
        private Mock<ISettingsService<SonarrSettings>> _sonarrSettingsMock;
        private Mock<ISonarrApi> _sonarrApiMock;
        private Mock<ISettingsService<PlexSettings>> _plexSettingsMock;
        private Mock<ISettingsService<CouchPotatoSettings>> _cpMock;
        private Mock<ISettingsService<PlexRequestSettings>> _plexRequestMock;
        private Mock<ISettingsService<AuthenticationSettings>> _authMock;
        private Mock<IAnalytics> _analytics;
        private Mock<IAvailabilityChecker> _availabilityMock;
        private Mock<IRequestService> _rServiceMock;
        private Mock<ISickRageApi> _srApi;
        private Mock<IMusicBrainzApi> _music;
        private Mock<IHeadphonesApi> _hpAPi;
        private Mock<ICouchPotatoCacher> _cpCache;
        private Mock<ISonarrCacher> _sonarrCache;
        private Mock<ISickRageCacher> _srCache;
        private Mock<IPlexApi> _plexApi;
        private Mock<IRepository<UsersToNotify>> _userRepo;
        private Mock<ISettingsService<EmailNotificationSettings>> _emailSettings;
        private Mock<IIssueService> _issueService;
        private Mock<ICacheProvider> _cache;
        private Mock<IRepository<RequestLimit>> RequestLimitRepo { get; set; }
        private SearchModule Search { get; set; }
        private readonly Fixture F = new Fixture();

        [Test]
        public void CheckNoRequestLimitTest()
        {
            var settings = new PlexRequestSettings { AlbumWeeklyRequestLimit = 0, MovieWeeklyRequestLimit = 2, TvWeeklyRequestLimit = 0 };
            var result = Search.CheckRequestLimit(settings, RequestType.Movie).Result;

            Assert.That(result, Is.True);
            RequestLimitRepo.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [TestCaseSource(nameof(MovieLimitData))]
        public bool CheckMovieLimitTest(int requestCount)
        {
            var users = F.CreateMany<RequestLimit>().ToList();
            users.Add(new RequestLimit { Username = "", RequestCount = requestCount, RequestType = RequestType.Movie});
            RequestLimitRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(users);
            var settings = new PlexRequestSettings { AlbumWeeklyRequestLimit = 0, MovieWeeklyRequestLimit = 5, TvWeeklyRequestLimit = 0 };
            var result = Search.CheckRequestLimit(settings, RequestType.Movie).Result;
            
            RequestLimitRepo.Verify(x => x.GetAllAsync(), Times.Once);

            return result;
        }

        private static IEnumerable<TestCaseData> MovieLimitData
        {
            get
            {
                yield return new TestCaseData(1).Returns(true).SetName("1 Request of 5");
                yield return new TestCaseData(2).Returns(true).SetName("2 Request of 5");
                yield return new TestCaseData(3).Returns(true).SetName("3 Request of 5");
                yield return new TestCaseData(4).Returns(true).SetName("4 Request of 5");
                yield return new TestCaseData(5).Returns(false).SetName("5 Request of 5");
                yield return new TestCaseData(6).Returns(false).SetName("6 Request of 5");
                yield return new TestCaseData(0).Returns(true).SetName("0 Request of 5");
            }
        }

        [SetUp]
        public void Setup()
        {
            _authMock = new Mock<Core.ISettingsService<AuthenticationSettings>>();
            _plexRequestMock = new Mock<ISettingsService<PlexRequestSettings>>();
            _plexRequestMock.Setup(x => x.GetSettings()).Returns(new PlexRequestSettings());
            _cpMock = new Mock<Core.ISettingsService<CouchPotatoSettings>>();
            _plexSettingsMock = new Mock<Core.ISettingsService<PlexSettings>>();
            _sonarrApiMock = new Mock<ISonarrApi>();
            _sonarrSettingsMock = new Mock<Core.ISettingsService<SonarrSettings>>();
            _cpApi = new Mock<ICouchPotatoApi>();
            _sickRageSettingsMock = new Mock<Core.ISettingsService<SickRageSettings>>();
            _notificationService = new Mock<INotificationService>();
            _headphonesSettings = new Mock<Core.ISettingsService<HeadphonesSettings>>();
            _cache = new Mock<ICacheProvider>();

            _analytics = new Mock<IAnalytics>();
            _availabilityMock = new Mock<IAvailabilityChecker>();
            _rServiceMock = new Mock<IRequestService>();
            _srApi = new Mock<ISickRageApi>();
            _music = new Mock<IMusicBrainzApi>();
            _hpAPi = new Mock<IHeadphonesApi>();
            _cpCache = new Mock<ICouchPotatoCacher>();
            _sonarrCache = new Mock<ISonarrCacher>();
            _srCache = new Mock<ISickRageCacher>();
            _plexApi = new Mock<IPlexApi>();
            _userRepo = new Mock<IRepository<UsersToNotify>>();
            RequestLimitRepo = new Mock<IRepository<RequestLimit>>();
            _emailSettings = new Mock<ISettingsService<EmailNotificationSettings>>();
            _issueService = new Mock<IIssueService>();
            CreateModule();
        }

        private void CreateModule()
        {
            Search = new SearchModule(_cache.Object, _cpMock.Object, _plexRequestMock.Object, _availabilityMock.Object,
                _rServiceMock.Object, _sonarrApiMock.Object, _sonarrSettingsMock.Object,
                _sickRageSettingsMock.Object, _cpApi.Object, _srApi.Object, _notificationService.Object,
                _music.Object, _hpAPi.Object, _headphonesSettings.Object, _cpCache.Object, _sonarrCache.Object,
                _srCache.Object, _plexApi.Object, _plexSettingsMock.Object, _authMock.Object,
                _userRepo.Object, _emailSettings.Object, _issueService.Object, _analytics.Object, RequestLimitRepo.Object);
        }



    }
}