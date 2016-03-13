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

using NUnit.Framework;

using PlexRequests.Api.Interfaces;
using PlexRequests.Api.Models;
using PlexRequests.Api.Models.Plex;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.UI.Models;
using PlexRequests.UI.Modules;

namespace PlexRequests.UI.Tests
{
    [TestFixture]
    public class AdminModuleTests
    {
        private Mock<ISettingsService<AuthenticationSettings>> AuthMock { get; set; }
        private Mock<IPlexApi> PlexMock { get; set; }

        [SetUp]
        public void Setup()
        {
            AuthMock = new Mock<ISettingsService<AuthenticationSettings>>();
            PlexMock = new Mock<IPlexApi>();
        }

        [Test]
        [Ignore("Need to finish")]
        public void RequestAuthTokenTest()
        {
            var expectedSettings = new AuthenticationSettings {UserAuthentication = false, PlexAuthToken = "abc"};
            AuthMock.Setup(x => x.GetSettings()).Returns(expectedSettings);

            var bootstrapper = new ConfigurableBootstrapper(with =>
            {
                with.Module<UserLoginModule>();
                with.Dependency(AuthMock.Object);
                with.Dependency(PlexMock.Object);
                with.RootPathProvider<TestRootPathProvider>();
                with.RequestStartup((container, pipelines, context) =>
                {
                    context.CurrentUser = new UserIdentity { UserName = "user"};
                });
            });

            bootstrapper.WithSession(new Dictionary<string, object>());

            var browser = new Browser(bootstrapper);
        
            var result = browser.Post("/admin/requestauth", with =>
            {
                with.HttpRequest();
                with.Header("Accept", "application/json");
                with.FormValue("username", "abc");
                with.FormValue("password","pass");
                
            });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("abc"));

            var body = JsonConvert.DeserializeObject<JsonResponseModel>(result.Body.AsString());
            Assert.That(body.Result, Is.EqualTo(true));
            AuthMock.Verify(x => x.GetSettings(), Times.Once);
            PlexMock.Verify(x => x.SignIn(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            PlexMock.Verify(x => x.GetUsers(It.IsAny<string>()), Times.Never);
        }

    }
}