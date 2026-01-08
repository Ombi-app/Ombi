using System;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
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

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
        }

        private PlexOAuthManager CreateSubject()
        {
            return _mocker.CreateInstance<PlexOAuthManager>();
        }

        [Test]
        public async Task GetAccessTokenFromPin_ReturnsToken_WhenPinValid_AndClientIdMatches()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var clientId = guid.ToString("N");

            _mocker.GetMock<IPlexApi>()
                .Setup(x => x.GetPin(It.IsAny<int>()))
                .ReturnsAsync(new OAuthContainer
                {
                    Result = new OAuthPin
                    {
                        id = 123,
                        code = "code",
                        trusted = true,
                        clientIdentifier = clientId,
                        expiresIn = 60,
                        authToken = "auth-token"
                    }
                });

            _mocker.GetMock<ISettingsService<CustomizationSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new CustomizationSettings());

            _mocker.GetMock<ISettingsService<PlexSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings { InstallId = guid });

            _mocker.Use(Mock.Of<ILogger<PlexOAuthManager>>());

            var subject = CreateSubject();

            // Act
            var token = await subject.GetAccessTokenFromPin(123);

            // Assert
            Assert.AreEqual("auth-token", token);
        }

        [Test]
        public async Task GetAccessTokenFromPin_ReturnsToken_WhenPinValid_AndClientIdMismatch()
        {
            // Arrange
            var serverGuid = Guid.NewGuid();
            var pinGuid = Guid.NewGuid();

            _mocker.GetMock<IPlexApi>()
                .Setup(x => x.GetPin(It.IsAny<int>()))
                .ReturnsAsync(new OAuthContainer
                {
                    Result = new OAuthPin
                    {
                        id = 456,
                        code = "code",
                        trusted = true,
                        clientIdentifier = pinGuid.ToString("N"),
                        expiresIn = 60,
                        authToken = "auth-token-2"
                    }
                });

            _mocker.GetMock<ISettingsService<CustomizationSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new CustomizationSettings());

            _mocker.GetMock<ISettingsService<PlexSettings>>()
                .Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new PlexSettings { InstallId = serverGuid });

            _mocker.Use(Mock.Of<ILogger<PlexOAuthManager>>());

            var subject = CreateSubject();

            // Act
            var token = await subject.GetAccessTokenFromPin(456);

            // Assert
            Assert.AreEqual("auth-token-2", token);
        }
    }
}
