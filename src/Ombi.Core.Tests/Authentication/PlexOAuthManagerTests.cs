using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Api.External.MediaServers.Plex.Models.OAuth;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Settings.Settings.Models;

namespace Ombi.Core.Tests.Authentication
{
    [TestFixture]
    public class PlexOAuthManagerTests
    {
        private AutoMocker _mocker;
        private MemoryCache _memoryCache;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mocker.Use<IMemoryCache>(_memoryCache);
            _mocker.Use(Mock.Of<ILogger<PlexOAuthManager>>());
        }

        [TearDown]
        public void TearDown()
        {
            _memoryCache.Dispose();
        }

        private PlexOAuthManager CreateSubject()
        {
            return _mocker.CreateInstance<PlexOAuthManager>();
        }

        [Test]
        public async Task CreatePin_CachesCode_AndPollingUsesIt()
        {
            var guid = Guid.NewGuid();
            var clientId = guid.ToString("N");

            _mocker.GetMock<IPlexApi>()
                .Setup(x => x.CreatePin())
                .ReturnsAsync(new OAuthContainer
                {
                    Result = new OAuthPin
                    {
                        id = 123,
                        code = "pin-code",
                        clientIdentifier = clientId,
                        expiresIn = 60
                    }
                });

            _mocker.GetMock<IPlexApi>()
                .Setup(x => x.GetPin(123, "pin-code"))
                .ReturnsAsync(new OAuthContainer
                {
                    Result = new OAuthPin
                    {
                        id = 123,
                        code = "pin-code",
                        trusted = true,
                        clientIdentifier = clientId,
                        expiresIn = 60,
                        authToken = "auth-token"
                    }
                });

            _mocker.GetMock<ISettingsService<PlexSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings { InstallId = guid });

            var subject = CreateSubject();

            await subject.CreatePin();
            var token = await subject.GetAccessTokenFromPin(123);

            Assert.AreEqual("auth-token", token);
            _mocker.GetMock<IPlexApi>().Verify(x => x.GetPin(123, "pin-code"), Times.Once);
        }

        [Test]
        public async Task GetAccessTokenFromPin_ReturnsEmpty_WhenPinCodeIsNotCached()
        {
            var subject = CreateSubject();

            var token = await subject.GetAccessTokenFromPin(999);

            Assert.That(token, Is.Empty);
            _mocker.GetMock<IPlexApi>().Verify(x => x.GetPin(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SuccessfulPolling_RemovesCachedPinCode()
        {
            var guid = Guid.NewGuid();
            var clientId = guid.ToString("N");

            _mocker.GetMock<IPlexApi>()
                .Setup(x => x.CreatePin())
                .ReturnsAsync(new OAuthContainer
                {
                    Result = new OAuthPin { id = 456, code = "one-use-code", clientIdentifier = clientId, expiresIn = 60 }
                });

            _mocker.GetMock<IPlexApi>()
                .Setup(x => x.GetPin(456, "one-use-code"))
                .ReturnsAsync(new OAuthContainer
                {
                    Result = new OAuthPin
                    {
                        id = 456,
                        code = "one-use-code",
                        clientIdentifier = clientId,
                        expiresIn = 60,
                        authToken = "auth-token-2"
                    }
                });

            _mocker.GetMock<ISettingsService<PlexSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings { InstallId = guid });

            var subject = CreateSubject();
            await subject.CreatePin();

            Assert.AreEqual("auth-token-2", await subject.GetAccessTokenFromPin(456));
            Assert.That(await subject.GetAccessTokenFromPin(456), Is.Empty);
            _mocker.GetMock<IPlexApi>().Verify(x => x.GetPin(456, "one-use-code"), Times.Once);
        }
    }
}
