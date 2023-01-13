using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Ombi.Core.Authentication;
using Ombi.Test.Common;
using System;
using System.Collections.Generic;
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
            context.Request.Headers.Add("UserAccessToken", new Microsoft.Extensions.Primitives.StringValues("test"));
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
            _mocker.Setup<IServiceProvider, object?>(x => x.GetService(typeof(OmbiUserManager)))
                .Returns(umMock.Object);


            await _subject.Invoke(context);

            _mocker.Verify<IServiceProvider>(x => x.GetService(It.IsAny<Type>()), Times.Once);
            umMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Test]
        public async Task ValidateUserAccessToken_Token_Invalid()
        {
            var context = GetContext();
            context.Request.Path = "/api";
            context.Request.Headers.Add("UserAccessToken", new Microsoft.Extensions.Primitives.StringValues("invalid"));
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
            _mocker.Setup<IServiceProvider, object?>(x => x.GetService(typeof(OmbiUserManager)))
                .Returns(umMock.Object);


            await _subject.Invoke(context);

            Assert.That(context.Response.StatusCode, Is.EqualTo(401));
            umMock.Verify(x => x.UpdateAsync(user), Times.Never);
        }

        private HttpContext GetContext()
        {
            var context = new DefaultHttpContext();
            context.RequestServices = _serviceProviderMock.Object;
            return context;
        }
    }
}
