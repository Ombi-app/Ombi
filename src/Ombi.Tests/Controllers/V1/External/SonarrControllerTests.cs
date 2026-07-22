using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.Sonarr;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Controllers.V1.External;
using Ombi.Core.Helpers;
using Ombi.Core.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;

namespace Ombi.Tests.Controllers.V1.External
{
    [TestFixture]
    public class SonarrControllerTests
    {
        [Test]
        public async Task SelectableProfiles_ReturnOnlyLiveProfilesInUsersAllowlist()
        {
            var mocker = new AutoMocker();
            var settings = new SonarrSettings { Enabled = true, ApiKey = "key", Ip = "sonarr", Port = 8989 };
            mocker.GetMock<ISettingsService<SonarrSettings>>().Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);
            mocker.GetMock<ISonarrV3Api>().Setup(x => x.GetProfiles(settings.ApiKey, settings.FullUri)).ReturnsAsync(new[]
            {
                new SonarrProfile { id = 7, name = "HD" },
                new SonarrProfile { id = 8, name = "UHD" }
            });
            var user = new OmbiUser { Id = "user", UserName = "user" };
            mocker.GetMock<ICurrentUser>().Setup(x => x.GetUser()).ReturnsAsync(user);
            var userManager = MockHelper.MockUserManager(new List<OmbiUser> { user });
            userManager.Setup(x => x.IsInRoleAsync(user, It.IsAny<string>())).ReturnsAsync(false);
            mocker.Use(userManager.Object);
            mocker.GetMock<IRepository<UserSelectableQualityProfile>>().Setup(x => x.GetAll()).Returns(new[]
            {
                new UserSelectableQualityProfile { UserId = user.Id, Application = SelectableQualityProfileApplication.Sonarr, QualityProfileId = 7 }
            }.AsQueryable().BuildMock());

            var profiles = (await mocker.CreateInstance<SonarrController>().GetSelectableProfiles()).ToList();

            Assert.That(profiles.Select(x => x.id), Is.EqualTo(new[] { 7 }));
        }

        [Test]
        public void SelectableProfiles_RequireDedicatedOrElevatedRole()
        {
            var authorize = typeof(SonarrController).GetMethod(nameof(SonarrController.GetSelectableProfiles))
                .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();

            Assert.That(authorize.Roles, Is.EqualTo($"{OmbiRoles.Admin},{OmbiRoles.PowerUser},{OmbiRoles.SelectSonarrQualityProfile}"));
        }
    }
}
