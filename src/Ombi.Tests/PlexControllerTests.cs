using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ombi.Api.External.MediaServers.Plex;
using Ombi.Api.External.MediaServers.Plex.Models;
using Ombi.Controllers.V1.External;
using Ombi.Core.Authentication;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Test.Common;

namespace Ombi.Tests
{
    [TestFixture]
    public class PlexControllerTests
    {
        private Mock<IPlexApi> _plexApi;
        private Mock<ISettingsService<PlexSettings>> _plexSettings;
        private Mock<IPlexOAuthManager> _oAuthManager;
        private Mock<IPlexService> _plexService;
        private Mock<OmbiUserManager> _userManager;
        private PlexController _subject;

        [SetUp]
        public void Setup()
        {
            _plexApi = new Mock<IPlexApi>();
            _plexSettings = new Mock<ISettingsService<PlexSettings>>();
            _oAuthManager = new Mock<IPlexOAuthManager>();
            _plexService = new Mock<IPlexService>();
            _userManager = MockHelper.MockUserManager(new List<OmbiUser>());

            _subject = new PlexController(_plexApi.Object, _plexSettings.Object,
                Mock.Of<ILogger<PlexController>>(), _oAuthManager.Object, _plexService.Object, _userManager.Object);
        }

        private void SetAdminExists(bool exists)
        {
            var admins = exists
                ? new List<OmbiUser> { new OmbiUser { Id = "admin-id", UserName = "admin" } }
                : new List<OmbiUser>();
            _userManager.Setup(x => x.GetUsersInRoleAsync(OmbiRoles.Admin)).ReturnsAsync(admins);
        }

        [Test]
        public async Task SignIn_ReturnsNull_WhenAdminAlreadyExists()
        {
            // SignIn is [AllowAnonymous] and overwrites the Plex configuration. Once an admin exists,
            // setup is complete and it must refuse before touching Plex.tv or the stored config.
            SetAdminExists(true);

            var result = await _subject.SignIn(new UserRequest { login = "attacker", password = "pw" });

            Assert.That(result, Is.Null);
            _plexApi.Verify(x => x.SignIn(It.IsAny<UserRequest>()), Times.Never);
            _plexSettings.Verify(x => x.SaveSettingsAsync(It.IsAny<PlexSettings>()), Times.Never);
        }

        [Test]
        public async Task SignIn_ReturnsNull_WhenPlexAlreadyConfigured()
        {
            // Regression test for the inverted guard: "!settings.Servers?.Any() ?? false" let the
            // request through whenever Plex was already configured. The corrected guard must block it.
            SetAdminExists(false);
            _plexSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings
            {
                Servers = new List<PlexServers> { new PlexServers() }
            });

            var result = await _subject.SignIn(new UserRequest { login = "attacker", password = "pw" });

            Assert.That(result, Is.Null);
            _plexApi.Verify(x => x.SignIn(It.IsAny<UserRequest>()), Times.Never);
        }

        [Test]
        public async Task SignIn_ProceedsPastGuard_WhenNoAdminAndPlexUnconfigured()
        {
            // The legitimate first-run wizard case: no admin yet and Plex not configured. The request
            // must be allowed through to Plex.tv. A returned authentication with a null user makes the
            // action return without saving any server configuration.
            SetAdminExists(false);
            _plexSettings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new PlexSettings { Servers = null });
            _plexApi.Setup(x => x.SignIn(It.IsAny<UserRequest>())).ReturnsAsync(new PlexAuthentication());

            var result = await _subject.SignIn(new UserRequest { login = "admin", password = "pw" });

            _plexApi.Verify(x => x.SignIn(It.IsAny<UserRequest>()), Times.Once);
            Assert.That(result, Is.Not.Null);
            _plexSettings.Verify(x => x.SaveSettingsAsync(It.IsAny<PlexSettings>()), Times.Never);
        }
    }
}
