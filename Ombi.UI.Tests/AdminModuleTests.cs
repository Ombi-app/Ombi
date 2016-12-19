#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: UserLoginModuleTests.cs
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

using System.Collections.Generic;
using Moq;
using Nancy;
using Nancy.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Plex;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.Services.Interfaces;
using Ombi.Services.Jobs;
using Ombi.Store.Models;
using Ombi.Store.Repository;
using Ombi.UI.Models;
using Ombi.UI.Modules.Admin;

namespace Ombi.UI.Tests
{
    [TestFixture]
    public class AdminModuleTests
    {
        private Mock<ISettingsService<PlexRequestSettings>> PlexRequestMock { get; set; }
        private Mock<ISettingsService<CouchPotatoSettings>> CpMock { get; set; }
        private Mock<ISettingsService<AuthenticationSettings>> AuthMock { get; set; }
        private Mock<ISettingsService<PlexSettings>> PlexSettingsMock { get; set; }
        private Mock<ISettingsService<SonarrSettings>> SonarrSettingsMock { get; set; }
        private Mock<ISettingsService<SickRageSettings>> SickRageSettingsMock { get; set; }
        private Mock<ISettingsService<ScheduledJobsSettings>> ScheduledJobsSettingsMock { get; set; }
        private Mock<ISettingsService<EmailNotificationSettings>> EmailMock { get; set; }
        private Mock<ISettingsService<PushbulletNotificationSettings>> PushbulletSettings { get; set; }
        private Mock<ISettingsService<PushoverNotificationSettings>> PushoverSettings { get; set; }
        private Mock<ISettingsService<HeadphonesSettings>> HeadphonesSettings { get; set; }
        private Mock<IPlexApi> PlexMock { get; set; }
        private Mock<ISonarrApi> SonarrApiMock { get; set; }
        private Mock<IPushbulletApi> PushbulletApi { get; set; }
        private Mock<IPushoverApi> PushoverApi { get; set; }
        private Mock<ICouchPotatoApi> CpApi { get; set; }
        private Mock<IJobRecord> RecorderMock { get; set; }
        private Mock<IRepository<LogEntity>> LogRepo { get; set; }
        private Mock<INotificationService> NotificationService { get; set; }
        private Mock<ICacheProvider> Cache { get; set; }
        private Mock<ISettingsService<LogSettings>> Log { get; set; }
        private Mock<ISettingsService<SlackNotificationSettings>> SlackSettings { get; set; }
        private Mock<ISettingsService<LandingPageSettings>> LandingPageSettings { get; set; }
        private Mock<ISlackApi> SlackApi { get; set; }
        private Mock<IAnalytics> Analytics { get; set; }
        private Mock<ISettingsService<NotificationSettingsV2>> NotifyV2 { get; set; }
        private Mock<IRecentlyAdded> RecentlyAdded { get; set; }

        private ConfigurableBootstrapper Bootstrapper { get; set; }

        [SetUp]
        public void Setup()
        {
            AuthMock = new Mock<ISettingsService<AuthenticationSettings>>();
            var expectedSettings = new AuthenticationSettings { UserAuthentication = false };
            AuthMock.Setup(x => x.GetSettings()).Returns(expectedSettings);
            PlexSettingsMock = new Mock<ISettingsService<PlexSettings>>();
            PlexSettingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings {PlexAuthToken = "abc"});

            PlexMock = new Mock<IPlexApi>();
            PlexMock.Setup(x => x.SignIn("Username1", "Password1"))
                    .Returns(new PlexAuthentication { user = new User { authentication_token = "abc", title = "Username1" } });

            PlexRequestMock = new Mock<ISettingsService<PlexRequestSettings>>();
            PlexRequestMock.Setup(x => x.GetSettings()).Returns(new PlexRequestSettings() );
            CpMock = new Mock<ISettingsService<CouchPotatoSettings>>();

            SonarrApiMock = new Mock<ISonarrApi>();
            SonarrSettingsMock = new Mock<ISettingsService<SonarrSettings>>();
            EmailMock = new Mock<ISettingsService<EmailNotificationSettings>>();
            PushbulletApi = new Mock<IPushbulletApi>();
            PushbulletSettings = new Mock<ISettingsService<PushbulletNotificationSettings>>();
            CpApi = new Mock<ICouchPotatoApi>();
            SickRageSettingsMock = new Mock<ISettingsService<SickRageSettings>>();
            LogRepo = new Mock<IRepository<LogEntity>>();
            PushoverSettings = new Mock<ISettingsService<PushoverNotificationSettings>>();
            PushoverApi = new Mock<IPushoverApi>();
            NotificationService = new Mock<INotificationService>();
            HeadphonesSettings = new Mock<ISettingsService<HeadphonesSettings>>();
            Cache = new Mock<ICacheProvider>();
            Log = new Mock<ISettingsService<LogSettings>>();
            SlackApi = new Mock<ISlackApi>();
            SlackSettings = new Mock<ISettingsService<SlackNotificationSettings>>();
            LandingPageSettings = new Mock<ISettingsService<LandingPageSettings>>();
            ScheduledJobsSettingsMock = new Mock<ISettingsService<ScheduledJobsSettings>>();
            RecorderMock = new Mock<IJobRecord>();
            Analytics = new Mock<IAnalytics>();
            NotifyV2= new Mock<ISettingsService<NotificationSettingsV2>>();
            RecentlyAdded = new Mock<IRecentlyAdded>();


            Bootstrapper = new ConfigurableBootstrapper(with =>
            {
                with.Module<AdminModule>();
                with.Dependency(AuthMock.Object);
                with.Dependency(PlexRequestMock.Object);
                with.Dependency(CpMock.Object);
                with.Dependency(PlexSettingsMock.Object);
                with.Dependency(SonarrApiMock.Object);
                with.Dependency(SonarrSettingsMock.Object);
                with.Dependency(PlexMock.Object);
                with.Dependency(EmailMock.Object);
                with.Dependency(PushbulletApi.Object);
                with.Dependency(PushbulletSettings.Object);
                with.Dependency(CpApi.Object);
                with.Dependency(SickRageSettingsMock.Object);
                with.Dependency(LogRepo.Object);
                with.Dependency(PushoverSettings.Object);
                with.Dependency(PushoverApi.Object);
                with.Dependency(NotifyV2.Object);
                with.Dependency(NotificationService.Object);
                with.Dependency(Analytics.Object);
                with.Dependency(HeadphonesSettings.Object);
                with.Dependency(Cache.Object);
                with.Dependency(Log.Object);
                with.Dependency(SlackApi.Object);
                with.Dependency(LandingPageSettings.Object);
                with.Dependency(SlackSettings.Object);
                with.Dependency(ScheduledJobsSettingsMock.Object);
                with.Dependency(RecorderMock.Object);
                with.Dependency(RecentlyAdded.Object);
                with.RootPathProvider<TestRootPathProvider>();
                with.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new UserIdentity { UserName = "user", Claims = new List<string> { "Admin" } };
                });
            });

            Bootstrapper.WithSession(new Dictionary<string, object>());
        }

        [Test]
        public void RequestAuthTokenTestNewSettings()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Post("/admin/requestauth", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", "Username1");
                with.FormValue("password", "Password1");

            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<JsonResponseModel>(result.Body.AsString());
            Assert.That(body.Result, Is.EqualTo(true));
            PlexMock.Verify(x => x.SignIn("Username1", "Password1"), Times.Once);
            PlexSettingsMock.Verify(x => x.SaveSettings(It.IsAny<PlexSettings>()), Times.Once);
        }

        [Test]
        public void RequestAuthTokenTestEmptyCredentials()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Post("/admin/requestauth", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", string.Empty);
                with.FormValue("password", "Password1");

            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<JsonResponseModel>(result.Body.AsString());
            Assert.That(body.Result, Is.EqualTo(false));
            Assert.That(body.Message, Is.Not.Empty);

            PlexMock.Verify(x => x.SignIn("Username1", "Password1"), Times.Never);
            AuthMock.Verify(x => x.GetSettings(), Times.Never);
            AuthMock.Verify(x => x.SaveSettings(It.IsAny<AuthenticationSettings>()), Times.Never);
        }

        [Test]
        public void RequestAuthTokenTesPlexSignInFail()
        {
            var browser = new Browser(Bootstrapper);

            var result = browser.Post("/admin/requestauth", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", "Badusername");
                with.FormValue("password", "Password1");

            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<JsonResponseModel>(result.Body.AsString());
            Assert.That(body.Result, Is.EqualTo(false));
            Assert.That(body.Message, Is.Not.Empty);

            PlexMock.Verify(x => x.SignIn("Badusername", "Password1"), Times.Once);
            AuthMock.Verify(x => x.GetSettings(), Times.Never);
            AuthMock.Verify(x => x.SaveSettings(It.IsAny<AuthenticationSettings>()), Times.Never);
        }

        [Test]
        public void RequestAuthTokenTestExistingSettings()
        {
            AuthMock.Setup(x => x.GetSettings()).Returns(() => null);
            var browser = new Browser(Bootstrapper);

            var result = browser.Post("/admin/requestauth", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", "Username1");
                with.FormValue("password", "Password1");

            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<JsonResponseModel>(result.Body.AsString());
            Assert.That(body.Result, Is.EqualTo(true));

            PlexMock.Verify(x => x.SignIn("Username1", "Password1"), Times.Once);
        }

        [Test]
        public void GetUsersSuccessfully()
        {
            var users = new PlexFriends { User = new[] { new UserFriends { Title = "abc2" }, } };
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(users);
            var browser = new Browser(Bootstrapper);

            var result = browser.Get("/admin/getusers", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", "Username1");
                with.FormValue("password", "Password1");

            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));


            var body = JsonConvert.DeserializeObject<JObject>(result.Body.AsString());
            var user = body["users"];
            Assert.That(body, Is.Not.Null);
            Assert.That(user.ToString().Contains("abc"), Is.True);

            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetUsersReturnsNoUsers()
        {
            var users = new PlexFriends();
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(users);
            var browser = new Browser(Bootstrapper);

            var result = browser.Get("/admin/getusers", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", "Username1");
                with.FormValue("password", "Password1");


            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<string>(result.Body.AsString());
            Assert.That(body, Is.Not.Null);
            Assert.That(string.IsNullOrWhiteSpace(body), Is.True);

            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetUsersReturnsNull()
        {
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(() => null);
            var browser = new Browser(Bootstrapper);

            var result = browser.Get("/admin/getusers", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", "Username1");
                with.FormValue("password", "Password1");

            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<string>(result.Body.AsString());
            Assert.That(body, Is.Not.Null);
            Assert.That(string.IsNullOrWhiteSpace(body), Is.True);

            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void GetUsersTokenIsNull()
        {
            PlexSettingsMock.Setup(x => x.GetSettings()).Returns(new PlexSettings());
            var browser = new Browser(Bootstrapper);

            var result = browser.Get("/admin/getusers", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", "Username1");
                with.FormValue("password", "Password1");

            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));

            var body = JsonConvert.DeserializeObject<JObject>(result.Body.AsString());
            var user = (string)body["users"];
            Assert.That(body, Is.Not.Null);
            Assert.That(string.IsNullOrWhiteSpace(user), Is.True);

            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }
    }
}