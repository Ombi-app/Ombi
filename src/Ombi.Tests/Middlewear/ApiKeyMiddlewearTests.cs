using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Authentication;
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

            var umMock = new Mock<OmbiUserManager>();
            _mocker.Setup<IServiceProvider, OmbiUserManager>(x => (OmbiUserManager)x.GetService(typeof(OmbiUserManager)))
                .Returns(umMock.Object);
            
            
            await _subject.Invoke(context);

            _mocker.Verify<IServiceProvider>(x => x.GetService(It.IsAny<Type>()), Times.Never);
        }

        private HttpContext GetContext()
        {
            var context = new DefaultHttpContext();
            context.RequestServices = _serviceProviderMock.Object;
            return context;
        }
    }
}
