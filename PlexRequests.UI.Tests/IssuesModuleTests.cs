#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: IssuesModuleTests.cs
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
using System.Linq;
using System.Threading.Tasks;

using Moq;

using Nancy;
using Nancy.Testing;

using Newtonsoft.Json;

using NUnit.Framework;

using PlexRequests.Core;
using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.UI.Models;
using PlexRequests.UI.Modules;

using Ploeh.AutoFixture;

namespace PlexRequests.UI.Tests
{
    [TestFixture]
    public class IssuesModuleTests
    {
        [SetUp]
        public void Setup()
        {
            var f = new Fixture();
            ModelList = f.CreateMany<IssuesModel>();
            PlexRequestMock = new Mock<ISettingsService<PlexRequestSettings>>();
            PlexRequestMock.Setup(x => x.GetSettings()).Returns(new PlexRequestSettings());
            PlexRequestMock.Setup(x => x.GetSettingsAsync()).Returns(Task.FromResult(new PlexRequestSettings()));
            IssueServiceMock = new Mock<IIssueService>();
            RequestServiceMock = new Mock<IRequestService>();
            NotificationServiceMock = new Mock<INotificationService>();
            IssueServiceMock.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(ModelList));

            Bootstrapper = new ConfigurableBootstrapper(
                with =>
                {
                    with.Module<IssuesModule>();
                    with.Dependency(PlexRequestMock.Object);
                    with.Dependency(IssueServiceMock.Object);
                    with.Dependency(RequestServiceMock.Object);
                    with.Dependency(NotificationServiceMock.Object);
                    with.RootPathProvider<TestRootPathProvider>();
                });

            Bootstrapper.WithSession(new Dictionary<string, object> { { SessionKeys.UsernameKey, "abc" } });
        }

        private Mock<ISettingsService<PlexRequestSettings>> PlexRequestMock { get; set; }
        private Mock<IIssueService> IssueServiceMock { get; set; }
        private Mock<IRequestService> RequestServiceMock { get; set; }
        private Mock<INotificationService> NotificationServiceMock { get; set; }
        private ConfigurableBootstrapper Bootstrapper { get; set; }
        private IEnumerable<IssuesModel> ModelList { get; set; }

        private IEnumerable<IssuesModel> NonResolvedModel => ModelList.Where(x => x.IssueStatus != IssueStatus.ResolvedIssue);

        [Test]
        public void GetIssuesNonAdminButAllCanSee()
        {
            var browser = new Browser(Bootstrapper);
            var result = browser.Get(
                "/issues/pending",
                with =>
                {
                    with.HttpRequest();
                    with.Header("Accept", "application/json");
                });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("abc"));

            var body = JsonConvert.DeserializeObject<List<IssuesViewModel>>(result.Body.AsString());

            Assert.That(body.Count, Is.EqualTo(NonResolvedModel.Count()));
            Assert.That(body[0].Title, Is.Not.Empty);
        }

        [Test]
        public void GetIssuesForAdmin()
        {
            Bootstrapper = new ConfigurableBootstrapper(
                with =>
                {
                    with.Module<IssuesModule>();
                    with.Dependency(PlexRequestMock.Object);
                    with.Dependency(IssueServiceMock.Object);
                    with.Dependency(RequestServiceMock.Object);
                    with.Dependency(NotificationServiceMock.Object);
                    with.RootPathProvider<TestRootPathProvider>();
                    with.RequestStartup(
                        (container, pipelines, context) =>
                        {
                            context.CurrentUser = new UserIdentity() { Claims = new[] { UserClaims.Admin } };
                        });
                });

            Bootstrapper.WithSession(new Dictionary<string, object> { { SessionKeys.UsernameKey, "abc" } });
            var browser = new Browser(Bootstrapper);
            var result = browser.Get(
                "/issues/pending",
                with =>
                {
                    with.HttpRequest();
                    with.Header("Accept", "application/json");
                });

            Assert.That(HttpStatusCode.OK, Is.EqualTo(result.StatusCode));
            Assert.That(result.Context.Request.Session[SessionKeys.UsernameKey], Is.EqualTo("abc"));

            var body = JsonConvert.DeserializeObject<List<IssuesViewModel>>(result.Body.AsString());
            Assert.That(body.Count, Is.EqualTo(NonResolvedModel.Count()));
            Assert.That(body[0].Title, Is.Not.Empty);
        }
    }
}