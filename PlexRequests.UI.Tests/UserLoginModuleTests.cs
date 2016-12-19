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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Nancy;
using Nancy.Linker;
using Nancy.Testing;
using NUnit.Framework;
using Ombi.Api.Interfaces;
using Ombi.Api.Models.Plex;
using Ombi.Core;
using Ombi.Core.SettingModels;
using Ombi.Helpers;
using Ombi.Helpers.Analytics;
using Ombi.UI.Modules;

namespace Ombi.UI.Tests
{
    [TestFixture]
    public class UserLoginModuleTests
    {
        private Mock<ISettingsService<AuthenticationSettings>> AuthMock { get; set; }
        private Mock<ISettingsService<PlexRequestSettings>> PlexRequestMock { get; set; }
        private Mock<ISettingsService<LandingPageSettings>> LandingPageMock { get; set; }
        private Mock<ISettingsService<PlexSettings>> PlexSettingsMock { get; set; }
        private ConfigurableBootstrapper Bootstrapper { get; set; }
        private Mock<IPlexApi> PlexMock { get; set; }
        private Mock<IAnalytics> IAnalytics { get; set; }
        private Mock<IResourceLinker> Linker { get; set; }

        [SetUp]
        public void Setup()
        {
            AuthMock = new Mock<ISettingsService<AuthenticationSettings>>();
            PlexMock = new Mock<IPlexApi>();
            LandingPageMock = new Mock<ISettingsService<LandingPageSettings>>();
            PlexRequestMock = new Mock<ISettingsService<PlexRequestSettings>>();
            PlexRequestMock.Setup(x => x.GetSettings()).Returns(new PlexRequestSettings());
            PlexRequestMock.Setup(x => x.GetSettingsAsync()).Returns(Task.FromResult(new PlexRequestSettings()));
            LandingPageMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new LandingPageSettings());
            IAnalytics = new Mock<IAnalytics>();
            Linker = new Mock<IResourceLinker>();
            Linker.Setup(x => x.BuildRelativeUri(It.IsAny<NancyContext>(), "SearchIndex", null)).Returns(new Uri("http://www.searchindex.com"));
            Linker.Setup(x => x.BuildRelativeUri(It.IsAny<NancyContext>(), "LandingPageIndex", null)).Returns(new Uri("http://www.landingpage.com"));
            Linker.Setup(x => x.BuildRelativeUri(It.IsAny<NancyContext>(), "UserLoginIndex", null)).Returns(new Uri("http://www.userloginindex.com"));
            PlexSettingsMock = new Mock<ISettingsService<PlexSettings>>();
            PlexSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings() {PlexAuthToken = "abc"});
            Bootstrapper = new ConfigurableBootstrapper(with =>
            {
                with.Module<UserLoginModule>();
                with.Dependency(PlexRequestMock.Object);
                with.Dependency(AuthMock.Object);
                with.Dependency(PlexMock.Object);
                with.Dependency(LandingPageMock.Object);
                with.Dependency(IAnalytics.Object);
                with.Dependency(Linker.Object);
                with.Dependency(PlexSettingsMock.Object);
                with.RootPathProvider<TestRootPathProvider>();
            });
        }

        [Test]
        public void LoginWithoutAuthentication()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = false };
            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "abc");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("abc"));


            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.searchindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void LoginWithoutAuthenticationWithEmptyUsername()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = false };
            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);


            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", string.Empty);
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));


            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.userloginindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Never);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void LoginWithUsernameSuccessfully()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = true };
            var plexFriends = new PlexFriends
            {
                User = new[]
                {
                    new UserFriends
                    {
                        Title = "abc",
                    },
                }
            };

            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(plexFriends);
            PlexMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(new PlexAccount());

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "abc");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("abc"));



            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.searchindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void LoginWithUsernameUnSuccessfully()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = true };
            var plexFriends = new PlexFriends
            {
                User = new[]
                {
                    new UserFriends
                    {
                        Username = "aaaa",
                    },
                }
            };

            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(plexFriends);
            PlexMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(new PlexAccount());

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);

            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "abc");
            });


            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.Null);

            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.userloginindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void LoginWithUsernameAndPasswordSuccessfully()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = true, UsePassword = true };
            var plexFriends = new PlexFriends
            {
                User = new[]
                {
                    new UserFriends
                    {
                        Title = "abc",
                    }
                }
            };
            var plexAuth = new PlexAuthentication
            {
                user = new User
                {
                    authentication_token = "abc"
                }
            };

            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(plexFriends);
            PlexMock.Setup(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>())).Returns(plexAuth);
            PlexMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(new PlexAccount());

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "abc");
                with.FormValue("Password", "abc");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("abc"));
            
            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.searchindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void LoginWithUsernameAndPasswordUnSuccessfully()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = true, UsePassword = true };
            var plexFriends = new PlexFriends
            {
                User = new[]
                {
                    new UserFriends
                    {
                        Username = "abc",
                    },
                }
            };
            var plexAuth = new PlexAuthentication
            {
                user = null
            };

            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(plexFriends);
            PlexMock.Setup(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>())).Returns(plexAuth);


            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "abc");
                with.FormValue("Password", "abc");
            });


            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.Null);
            
            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.userloginindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void AttemptToLoginAsDeniedUser()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = false, DeniedUsers = "abc" };
            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "abc");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.Null);

            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.userloginindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Logout()
        {
            Bootstrapper.WithSession(new Dictionary<string, object> { { SessionKeys.UsernameKey, "abc" } });

            var browser = new Browser(Bootstrapper);
            var result = browser.Get("/userlogin/logout", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.Null);
        }

        [Test]
        public void LoginWithOwnerUsernameSuccessfully()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = true };
            var plexFriends = new PlexFriends
            {
                User = new[]
                {
                    new UserFriends()
                }
            };

            var account = new PlexAccount { Username = "Jamie" };
            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(plexFriends);
            PlexMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);
            PlexMock.Setup(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>())).Returns(new PlexAuthentication { user = new User { username = "Jamie" } });

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "Jamie");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("Jamie"));
            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.searchindex.com/"))); // Redirect header

            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void LoginWithOwnerUsernameAndPasswordSuccessfully()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = true, UsePassword = true };
            var plexFriends = new PlexFriends
            {
                User = new[]
                {
                    new UserFriends()
                }
            };
            var plexAuth = new PlexAuthentication
            {
                user = new User
                {
                    authentication_token = "abc",
                    username = "Jamie"
                }
            };

            var account = new PlexAccount { Username = "Jamie" };

            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);
            PlexMock.Setup(x => x.GetUsers(It.IsAny<string>())).Returns(plexFriends);
            PlexMock.Setup(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>())).Returns(plexAuth);
            PlexMock.Setup(x => x.GetAccount(It.IsAny<string>())).Returns(account);

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "jamie");
                with.FormValue("Password", "abc");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.searchindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

        [Test]
        [Description("We should go to the landing page as it is enabled after we log in")]
        public void LoginWithLandingPageAfterLogin()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = false };
            LandingPageMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new LandingPageSettings { BeforeLogin = false, Enabled = true });
            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "abc");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("abc"));

            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.landingpage.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

        [Test]
        [Description("We shouldn't go to the landing page as we have already been there!")]
        public void LoginWithLandingPageBefore()
        {
            var expectedSettings = new AuthenticationSettings { UserAuthentication = false };
            LandingPageMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new LandingPageSettings { BeforeLogin = true, Enabled = true });
            AuthMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(expectedSettings);

            Bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(Bootstrapper);
            var result = browser.Post("/userlogin", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("Username", "abc");
            });

            Assert.That(HttpStatusCode.SeeOther, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("abc"));

            Assert.That(result.Headers.Contains(new KeyValuePair<string, string>("Location", "http://www.searchindex.com/"))); // Redirect header
            AuthMock.Verify(x => x.GetSettingsAsync(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

    }
}