using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using Ombi.Test.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Tests.Middlewear
{
    [TestFixture]
    public class ApiKeyMiddlewearTests
    {
        private AutoMocker _mocker;
        private ApiKeyMiddlewear _subject;
        private Mock<IServiceProvider> _serviceProviderMock;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _mocker.Use(_serviceProviderMock);
            _subject = _mocker.CreateInstance<ApiKeyMiddlewear>();
        }

        [Test]
        public async Task NonApiAccess()
        {
            var context = GetContext();
            context.Request.Path = "/notanapi";
            await _subject.Invoke(context);

            _mocker.Verify<IServiceProvider>(x => x.GetService(It.IsAny<Type>()), Times.Never);
        }

        [Test]
        public async Task ValidateUserAccessToken()
        {
            var context = GetContext();
            context.Request.Path = "/api";
            context.Request.Headers["UserAccessToken"] = "test";
            var user = new Store.Entities.OmbiUser
            {
                UserAccessToken = "test",
                UserName = "unit test"
            };
            var umMock = MockHelper.MockUserManager(new List<Store.Entities.OmbiUser>
            {
               user
            });
            umMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
#nullable enable
            _mocker.Setup<IServiceProvider, object?>(x => x.GetService(typeof(OmbiUserManager)))
                .Returns(umMock.Object);
#nullable disable


            await _subject.Invoke(context);

            _mocker.Verify<IServiceProvider>(x => x.GetService(It.IsAny<Type>()), Times.Once);
            umMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Test]
        public async Task ValidateUserAccessToken_Token_Invalid()
        {
            var context = GetContext();
            context.Request.Path = "/api";
            context.Request.Headers["UserAccessToken"] ="invalid";
            var user = new Store.Entities.OmbiUser
            {
                UserAccessToken = "test",
                UserName = "unit test"
            };
            var umMock = MockHelper.MockUserManager(
            [
               user
            ]);
            umMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });
#nullable enable
            _mocker.Setup<IServiceProvider, object?>(x => x.GetService(typeof(OmbiUserManager)))
                .Returns(umMock.Object);
#nullable disable

            await _subject.Invoke(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            umMock.Verify(x => x.UpdateAsync(user), Times.Never);
        }

        [Test]
        public async Task ValidateApiKey_InvalidUser()
        {
            var context = GetContext();
            context.Request.Path = "/api";
            context.Request.Headers["ApiKey"] = "validkey";
            context.Request.Headers["UserName"] = "nonexistent";

            using (var bodyStream = new MemoryStream()) {
                context.Response.Body = bodyStream;

                var settingsMock = new Mock<ISettingsService<OmbiSettings>>();
                settingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new OmbiSettings { ApiKey = "validkey" });
#nullable enable
                _mocker.Setup<IServiceProvider, object?>(x => x.GetService(typeof(ISettingsService<OmbiSettings>))).Returns(settingsMock.Object);
#nullable disable

                var umMock = MockHelper.MockUserManager(new List<Store.Entities.OmbiUser>());
#nullable enable
                _mocker.Setup<IServiceProvider, object?>(x => x.GetService(typeof(OmbiUserManager))).Returns(umMock.Object);
#nullable disable

                var nextMock = new Mock<RequestDelegate>();
                nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
                var subject = new ApiKeyMiddlewear(nextMock.Object);

                await subject.Invoke(context);

                Assert.That(context.Response.StatusCode, Is.EqualTo(401));
                bodyStream.Position = 0;
                using (var reader = new StreamReader(bodyStream))
                {
                    var body = await reader.ReadToEndAsync();
                    Assert.That(body, Is.EqualTo("Invalid User"));
                    nextMock.Verify(x => x.Invoke(It.IsAny<HttpContext>()), Times.Never);
                }
            }
        }

        [Test]
        public async Task ValidateApiKey_NoUserNameHeader()
        {
            var context = GetContext();
            context.Request.Path = "/api";
            context.Request.Headers["ApiKey"] = "validkey";

            var settingsMock = new Mock<ISettingsService<OmbiSettings>>();
            settingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new OmbiSettings { ApiKey = "validkey" });
#nullable enable
            _mocker.Setup<IServiceProvider, object?>(x => x.GetService(typeof(ISettingsService<OmbiSettings>))).Returns(settingsMock.Object);
#nullable disable

            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            var subject = new ApiKeyMiddlewear(nextMock.Object);

            await subject.Invoke(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(200)); // Default status
            Assert.That(context.User.Identity.Name, Is.EqualTo("API"));
            nextMock.Verify(x => x.Invoke(It.IsAny<HttpContext>()), Times.Once);
        }

        private HttpContext GetContext()
        {
            var context = new DefaultHttpContext();
            context.RequestServices = _serviceProviderMock.Object;
            return context;
        }
    }
}
