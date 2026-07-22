using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.Radarr;
using Ombi.Api.External.ExternalApis.Radarr.Models.V3;
using Ombi.Controllers.V1.External;
using Ombi.Core.Helpers;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Core.Models;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;
using MockQueryable.Moq;


namespace Ombi.Tests.Controllers.V1.External
{
    [TestFixture]
    public class RadarrControllerTests
    {
        [Test]
        public async Task Profiles_PreservesFullHistoricalResponse()
        {
            var mocker = new AutoMocker();
            mocker.GetMock<ISettingsService<RadarrSettings>>().Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = true, ApiKey = "key", Ip = "radarr", Port = 7878 });
            mocker.GetMock<IRadarrV3Api>().Setup(x => x.GetProfiles("key", It.IsAny<string>()))
                .ReturnsAsync(new List<RadarrV3QualityProfile> { new RadarrV3QualityProfile { id = 7, name = "HD", upgradeAllowed = true } });

            var result = (OkObjectResult)await mocker.CreateInstance<RadarrController>().GetProfiles();
            var profiles = ((IEnumerable<RadarrV3QualityProfile>)result.Value).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(profiles.Single().id, Is.EqualTo(7));
                Assert.That(profiles.Single().name, Is.EqualTo("HD"));
                Assert.That(profiles.Single().upgradeAllowed, Is.True);
            });
        }

        [TestCase(nameof(RadarrController.GetProfiles))]
        [TestCase(nameof(RadarrController.GetProfiles4K))]
        public void HistoricalProfiles_RequirePowerUserAuthorization(string action)
        {
            var authorize = typeof(RadarrController).GetMethods().Single(x => x.Name == action && x.GetParameters().Length == 0)
                .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();

            Assert.That(authorize.Roles, Is.EqualTo($"{OmbiRoles.Admin},{OmbiRoles.PowerUser}"));
        }

        [Test]
        public async Task SelectableProfiles_ReturnOnlyIdAndName()
        {
            var mocker = new AutoMocker();
            mocker.GetMock<ISettingsService<RadarrSettings>>().Setup(x => x.GetSettingsAsync())
                .ReturnsAsync(new RadarrSettings { Enabled = true, ApiKey = "key", Ip = "radarr", Port = 7878 });
            mocker.GetMock<IRadarrV3Api>().Setup(x => x.GetProfiles("key", It.IsAny<string>()))
                .ReturnsAsync(new List<RadarrV3QualityProfile>
                {
                    new RadarrV3QualityProfile { id = 7, name = "HD", upgradeAllowed = true },
                    new RadarrV3QualityProfile { id = 8, name = "UHD" }
                });
            var user = new OmbiUser { Id = "user", UserName = "user" };
            mocker.GetMock<ICurrentUser>().Setup(x => x.GetUser()).ReturnsAsync(user);
            var userManager = MockHelper.MockUserManager(new List<OmbiUser> { user });
            userManager.Setup(x => x.IsInRoleAsync(user, It.IsAny<string>())).ReturnsAsync(false);
            mocker.Use(userManager.Object);
            mocker.GetMock<IRepository<UserSelectableQualityProfile>>().Setup(x => x.GetAll()).Returns(new[]
            {
                new UserSelectableQualityProfile { UserId = user.Id, Application = SelectableQualityProfileApplication.Radarr, QualityProfileId = 7 }
            }.AsQueryable().BuildMock());

            var result = (OkObjectResult)await mocker.CreateInstance<RadarrController>().GetSelectableProfiles();
            var profiles = ((IEnumerable<RadarrQualityProfileModel>)result.Value).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(profiles.Single().Id, Is.EqualTo(7));
                Assert.That(profiles.Single().Name, Is.EqualTo("HD"));
                Assert.That(typeof(RadarrQualityProfileModel).GetProperties().Select(x => x.Name), Is.EquivalentTo(new[] { "Id", "Name" }));
            });
        }

        [TestCase(nameof(RadarrController.GetSelectableProfiles))]
        [TestCase(nameof(RadarrController.GetSelectableProfiles4K))]
        public void SelectableProfiles_RequireDedicatedOrElevatedRole(string action)
        {
            var authorize = typeof(RadarrController).GetMethods().Single(x => x.Name == action && x.GetParameters().Length == 0)
                .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();

            Assert.That(authorize.Roles, Is.EqualTo($"{OmbiRoles.Admin},{OmbiRoles.PowerUser},{OmbiRoles.SelectRadarrQualityProfile}"));
        }
    }
}
