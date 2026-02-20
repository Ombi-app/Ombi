using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Api.External.MediaServers.Plex.Models;
using Ombi.Api.External.MediaServers.Jellyfin;
using Ombi.Api.External.MediaServers.Jellyfin.Models;
using Ombi.Api.External.MediaServers.Emby;
using Ombi.Api.External.MediaServers.Emby.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Test.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Authentication
{
    [TestFixture]
    public class OmbiUserManagerTests
    {

        [SetUp]
        public void Setup()
        {

            UserStore = new Mock<IUserStore<OmbiUser>>();
            PlexApi = new Mock<IPlexApi>();
            EmbyApiFactory = new Mock<IEmbyApiFactory>();
            EmbyApi = new Mock<IEmbyApi>();
            EmbySettings = new Mock<ISettingsService<EmbySettings>>();
            JellyfinApiFactory = new Mock<IJellyfinApiFactory>();
            JellyfinApi = new Mock<IJellyfinApi>();
            JellyfinSettings = new Mock<ISettingsService<JellyfinSettings>>();
            AuthenticationSettings = new Mock<ISettingsService<AuthenticationSettings>>();

            AuthenticationSettings.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new AuthenticationSettings());

            EmbySettings.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new EmbySettings
                {
                    Servers = new List<EmbyServers>
                    {
                        new EmbyServers
                        {
                            ApiKey = "emby-test-key",
                            Ip = "localhost",
                            Port = 8096,
                            Ssl = false
                        }
                    }
                });

            EmbyApiFactory.Setup(x => x.CreateClient(It.IsAny<EmbySettings>()))
                .Returns(EmbyApi.Object);

            JellyfinSettings.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new JellyfinSettings
                {
                    Servers = new List<JellyfinServers>
                    {
                        new JellyfinServers
                        {
                            ApiKey = "test-key",
                            Ip = "localhost",
                            Port = 8096,
                            Ssl = false
                        }
                    }
                });

            JellyfinApiFactory.Setup(x => x.CreateClient(It.IsAny<JellyfinSettings>()))
                .Returns(JellyfinApi.Object);

            _um = new OmbiUserManager(UserStore.Object, null, null, null, null, null, null, null, null,
                PlexApi.Object, EmbyApiFactory.Object, EmbySettings.Object, JellyfinApiFactory.Object, JellyfinSettings.Object, AuthenticationSettings.Object);
        }

        public OmbiUserManager _um { get; set; }
        private Mock<IUserStore<OmbiUser>> UserStore { get; set; }
        private Mock<IPlexApi> PlexApi { get; set; }
        private Mock<IEmbyApiFactory> EmbyApiFactory { get; set; }
        private Mock<IEmbyApi> EmbyApi { get; set; }
        private Mock<ISettingsService<EmbySettings>> EmbySettings { get; set; }
        private Mock<IJellyfinApiFactory> JellyfinApiFactory { get; set; }
        private Mock<IJellyfinApi> JellyfinApi { get; set; }
        private Mock<ISettingsService<JellyfinSettings>> JellyfinSettings { get; set; }
        private Mock<ISettingsService<AuthenticationSettings>> AuthenticationSettings { get; set; }

        [Test]
        public async Task CheckPassword_PlexUser_EmailLogin_ValidPassword()
        {
            var user = new OmbiUser
            {
                UserType = UserType.PlexUser,
                EmailLogin = true,
                Email = "MyEmail@email.com"
            };
            PlexApi.Setup(x => x.SignIn(It.IsAny<UserRequest>()))
                .ReturnsAsync(new PlexAuthentication
                {
                    user = new Ombi.Api.External.MediaServers.Plex.Models.User
                    {
                        authentication_token = "abc"
                    }
                });
            var result = await _um.CheckPasswordAsync(user, "pass");

            Assert.That(result, Is.True);
            PlexApi.Verify(x => x.SignIn(It.Is<UserRequest>(c => c.login == "MyEmail@email.com")), Times.Once);
        }

        [Test]
        public async Task CheckPassword_PlexUser_UserNameLogin_ValidPassword()
        {
            var user = new OmbiUser
            {
                UserType = UserType.PlexUser,
                EmailLogin = false,
                Email = "MyEmail@email.com",
                UserName = "heyhey"
            };
            PlexApi.Setup(x => x.SignIn(It.IsAny<UserRequest>()))
                .ReturnsAsync(new PlexAuthentication
                {
                    user = new Ombi.Api.External.MediaServers.Plex.Models.User
                    {
                        authentication_token = "abc"
                    }
                });
            var result = await _um.CheckPasswordAsync(user, "pass");

            Assert.That(result, Is.True);
            PlexApi.Verify(x => x.SignIn(It.Is<UserRequest>(c => c.login == "heyhey")), Times.Once);
        }

        [Test]
        public async Task CheckPassword_PlexUser_UserNameLogin_InvalidPassword()
        {
            var user = new OmbiUser
            {
                UserType = UserType.PlexUser,
                EmailLogin = false,
                Email = "MyEmail@email.com",
                UserName = "heyhey"
            };
            PlexApi.Setup(x => x.SignIn(It.IsAny<UserRequest>()))
                .ReturnsAsync(new PlexAuthentication());
            var result = await _um.CheckPasswordAsync(user, "pass");

            Assert.That(result, Is.False);
            PlexApi.Verify(x => x.SignIn(It.Is<UserRequest>(c => c.login == "heyhey")), Times.Once);
        }

        [Test]
        public async Task CheckPassword_JellyfinUser_InvalidPassword_ShouldReturnFalse()
        {
            // Arrange - After fix, LogIn returns null when authentication fails
            var user = new OmbiUser
            {
                UserType = UserType.JellyfinUser,
                UserName = "testuser"
            };

            // When auth fails (no AccessToken), LogIn now returns null
            JellyfinApi.Setup(x => x.LogIn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((JellyfinUser)null);

            // Act
            var result = await _um.CheckPasswordAsync(user, "wrongpassword");

            // Assert - Should return FALSE when authentication fails
            Assert.That(result, Is.False, "Authentication should fail with invalid password");
            JellyfinApi.Verify(x => x.LogIn("testuser", "wrongpassword", "test-key", "http://localhost:8096/"), Times.Once);
        }

        [Test]
        public async Task CheckPassword_JellyfinUser_ValidPassword_ShouldReturnTrue()
        {
            // Arrange - Simulate successful authentication
            var user = new OmbiUser
            {
                UserType = UserType.JellyfinUser,
                UserName = "testuser"
            };

            // When auth succeeds, LogIn should return a valid user with ID
            JellyfinApi.Setup(x => x.LogIn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new JellyfinUser
                {
                    Id = "user-id-123",
                    Name = "testuser"
                });

            // Act
            var result = await _um.CheckPasswordAsync(user, "correctpassword");

            // Assert - Should return true when authentication succeeds
            Assert.That(result, Is.True, "Authentication should succeed with valid password");
            JellyfinApi.Verify(x => x.LogIn("testuser", "correctpassword", "test-key", "http://localhost:8096/"), Times.Once);
        }

        [Test]
        public async Task CheckPassword_EmbyUser_InvalidPassword_ShouldReturnFalse()
        {
            // Arrange - After fix, LogIn returns null when authentication fails
            var user = new OmbiUser
            {
                UserType = UserType.EmbyUser,
                UserName = "embyuser"
            };

            // When auth fails (no AccessToken), LogIn now returns null
            EmbyApi.Setup(x => x.LogIn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((EmbyUser)null);

            // Act
            var result = await _um.CheckPasswordAsync(user, "wrongpassword");

            // Assert - Should return FALSE when authentication fails
            Assert.That(result, Is.False, "Authentication should fail with invalid password");
            EmbyApi.Verify(x => x.LogIn("embyuser", "wrongpassword", "emby-test-key", "http://localhost:8096/", It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task CheckPassword_EmbyUser_ValidPassword_ShouldReturnTrue()
        {
            // Arrange - Simulate successful authentication
            var user = new OmbiUser
            {
                UserType = UserType.EmbyUser,
                UserName = "embyuser"
            };

            // When auth succeeds, LogIn should return a valid user with ID
            EmbyApi.Setup(x => x.LogIn(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmbyUser
                {
                    Id = "emby-user-id-123",
                    Name = "embyuser"
                });

            // Act
            var result = await _um.CheckPasswordAsync(user, "correctpassword");

            // Assert - Should return true when authentication succeeds
            Assert.That(result, Is.True, "Authentication should succeed with valid password");
            EmbyApi.Verify(x => x.LogIn("embyuser", "correctpassword", "emby-test-key", "http://localhost:8096/", It.IsAny<string>()), Times.Once);
        }
    }
}
