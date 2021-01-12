using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Ombi.Api.Plex;
using Ombi.Api.Plex.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
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
            AuthenticationSettings = new Mock<ISettingsService<AuthenticationSettings>>();

            AuthenticationSettings.Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new AuthenticationSettings());
            _um = new OmbiUserManager(UserStore.Object, null, null, null, null, null, null, null, null,
                PlexApi.Object, null, null, null, null, AuthenticationSettings.Object);
        }

        public OmbiUserManager _um { get; set; }
        private Mock<IUserStore<OmbiUser>> UserStore { get; set; }
        private Mock<IPlexApi> PlexApi { get; set; }
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
                    user = new User
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
                    user = new User
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
    }
}
