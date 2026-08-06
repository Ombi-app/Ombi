using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Controllers.V1;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Test.Common;

namespace Ombi.Tests
{
    [TestFixture]
    public class PlexOAuthControllerTests
    {
        private Mock<IPlexOAuthManager> _oAuthManager;
        private Mock<IPlexApi> _plexApi;
        private Mock<ISettingsService<PlexSettings>> _plexSettings;
        private Mock<OmbiUserManager> _userManager;
        private PlexOAuthController _subject;

        [SetUp]
        public void Setup()
        {
            _oAuthManager = new Mock<IPlexOAuthManager>();
            _plexApi = new Mock<IPlexApi>();
            _plexSettings = new Mock<ISettingsService<PlexSettings>>();
            _userManager = MockHelper.MockUserManager(new List<OmbiUser>());

            _subject = new PlexOAuthController(_oAuthManager.Object, _plexApi.Object,
                _plexSettings.Object, Mock.Of<ILogger<PlexOAuthController>>(), _userManager.Object);
        }

        private void SetAdminExists(bool exists)
        {
            var admins = exists
                ? new List<OmbiUser> { new OmbiUser { Id = "admin-id", UserName = "admin" } }
                : new List<OmbiUser>();
            _userManager.Setup(x => x.GetUsersInRoleAsync(OmbiRoles.Admin)).ReturnsAsync(admins);
        }

        [Test]
        public async Task OAuthWizardCallBack_ReturnsUnauthorized_WhenAdminAlreadyExists()
        {
            // This callback belongs to the first-run wizard. Once an admin exists the wizard is
            // complete, so it should do nothing and must not read or write the Plex configuration.
            SetAdminExists(true);

            var result = await _subject.OAuthWizardCallBack(1234);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
            _oAuthManager.Verify(x => x.GetAccessTokenFromPin(It.IsAny<int>()), Times.Never);
            _plexSettings.Verify(x => x.SaveSettingsAsync(It.IsAny<PlexSettings>()), Times.Never);
        }

        [Test]
        public async Task OAuthWizardCallBack_ProceedsPastGuard_WhenNoAdminExists()
        {
            // On a fresh install (no admin yet) the wizard must still be able to use this callback.
            // Returning an empty token makes the action bail out immediately after the guard, which
            // proves the request was allowed through.
            SetAdminExists(false);
            _oAuthManager.Setup(x => x.GetAccessTokenFromPin(1234)).ReturnsAsync(string.Empty);

            var result = await _subject.OAuthWizardCallBack(1234);

            _oAuthManager.Verify(x => x.GetAccessTokenFromPin(1234), Times.Once);
            Assert.That(result, Is.InstanceOf<JsonResult>());
        }
    }
}
