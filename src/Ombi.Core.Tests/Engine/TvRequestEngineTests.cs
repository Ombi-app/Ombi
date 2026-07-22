using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.ExternalApis.TheMovieDb;
using Ombi.Api.External.ExternalApis.Sonarr;
using Ombi.Api.External.ExternalApis.Sonarr.Models;
using Ombi.Api.External.ExternalApis.TvMaze;
using Ombi.Api.External.ExternalApis.TvMaze.Models;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Helpers;
using Ombi.Core.Models;
using Ombi.Core.Models.Requests;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models.External;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class TvRequestEngineTests
    {
        private AutoMocker _mocker;
        private TvRequestEngine _subject;
        private OmbiUser _user;
        private Mock<OmbiUserManager> _userManager;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _user = new OmbiUser { Id = "user", UserName = "user" };
            _userManager = MockHelper.MockUserManager(new List<OmbiUser> { _user });
            _mocker.Use(_userManager.Object);
            _subject = _mocker.CreateInstance<TvRequestEngine>();
        }

        [Test]
        public async Task QualityOverride_RequiresDedicatedRoleAndAllowlist()
        {
            SetupRole(OmbiRoles.SelectSonarrQualityProfile);
            SetupAllowlist();

            var result = await Validate(2);

            Assert.That(result.Result, Is.False);
            _mocker.GetMock<ISonarrV3Api>().Verify(x => x.GetProfiles(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task QualityOverride_RejectsProfileMissingFromLiveSonarr()
        {
            SetupRole(OmbiRoles.SelectSonarrQualityProfile);
            SetupAllowlist(2);
            SetupSonarr(3);

            var result = await Validate(2);

            Assert.That(result.Result, Is.False);
        }

        [Test]
        public async Task QualityOverride_AllowsDedicatedRoleWithAllowedLiveProfile()
        {
            SetupRole(OmbiRoles.SelectSonarrQualityProfile);
            SetupAllowlist(2);
            SetupSonarr(2);

            Assert.That(await Validate(2), Is.Null);
        }

        [Test]
        public async Task MissingQualityOverride_KeepsDefaultBehavior()
        {
            Assert.That(await Validate(null), Is.Null);
            _mocker.GetMock<IRepository<UserSelectableQualityProfile>>().Verify(x => x.GetAll(), Times.Never);
            _mocker.GetMock<ISonarrV3Api>().Verify(x => x.GetProfiles(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void V1NewRequest_PersistsQualityOverride()
        {
            var builder = new TvShowRequestBuilder(Mock.Of<ITvMazeApi>(), Mock.Of<IMovieDbApi>(), Mock.Of<ILogger>());
            typeof(TvShowRequestBuilder).GetProperty("ShowInfo", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(builder, new TvMazeShow { name = "Show", summary = string.Empty, externals = new Externals() });

            builder.CreateNewRequest(new TvRequestViewModel { TvDbId = 1, QualityPathOverride = 7 });

            Assert.That(builder.NewRequest.QualityOverride, Is.EqualTo(7));
        }

        private async Task<RequestEngineResult> Validate(int? profileId)
        {
            var method = typeof(TvRequestEngine).GetMethod("ValidateQualityProfile", BindingFlags.Instance | BindingFlags.NonPublic);
            return await (Task<RequestEngineResult>)method.Invoke(_subject, new object[] { _user, profileId, false });
        }

        private void SetupRole(params string[] roles) =>
            _userManager.Setup(x => x.IsInRoleAsync(_user, It.IsAny<string>()))
                .ReturnsAsync((OmbiUser _, string role) => roles.Contains(role));

        private void SetupAllowlist(params int[] profileIds) =>
            _mocker.GetMock<IRepository<UserSelectableQualityProfile>>().Setup(x => x.GetAll())
                .Returns(profileIds.Select(id => new UserSelectableQualityProfile
                {
                    UserId = _user.Id,
                    Application = SelectableQualityProfileApplication.Sonarr,
                    QualityProfileId = id
                }).AsQueryable().BuildMock());

        private void SetupSonarr(params int[] profileIds)
        {
            var settings = new SonarrSettings { Enabled = true, ApiKey = "key", Ip = "sonarr", Port = 8989 };
            _mocker.GetMock<Ombi.Core.Settings.ISettingsService<SonarrSettings>>().Setup(x => x.GetSettingsAsync()).ReturnsAsync(settings);
            _mocker.GetMock<ISonarrV3Api>().Setup(x => x.GetProfiles(settings.ApiKey, settings.FullUri))
                .ReturnsAsync(profileIds.Select(id => new SonarrProfile { id = id, name = $"Profile {id}" }));
        }
    }
}
