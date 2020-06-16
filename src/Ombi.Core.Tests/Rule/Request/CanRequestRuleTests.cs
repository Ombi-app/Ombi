using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Rule.Rules;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Rule.Request
{
    public class CanRequestRuleTests
    {
        private List<OmbiUser> _users = new List<OmbiUser>
        {
             new OmbiUser { Id = Guid.NewGuid().ToString("N"), UserName="abc", NormalizedUserName = "ABC", UserType = UserType.LocalUser},
             new OmbiUser { Id = Guid.NewGuid().ToString("N"), UserName="sys", NormalizedUserName = "SYS", UserType = UserType.SystemUser}
        };

        [SetUp]
        public void Setup()
        {

            PrincipalMock = new Mock<IPrincipal>();
            PrincipalMock.Setup(x => x.Identity.Name).Returns("abc");

            UserManager = MockHelper.MockUserManager(_users);
            Rule = new CanRequestRule(PrincipalMock.Object, UserManager.Object);
        }


        private CanRequestRule Rule { get; set; }
        private Mock<IPrincipal> PrincipalMock { get; set; }
        private Mock<OmbiUserManager> UserManager { get; set; }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingMovieWithMovieRole()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.RequestMovie)).ReturnsAsync(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnFail_WhenRequestingMovieWithoutMovieRole()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.RequestMovie)).ReturnsAsync(false);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.False(result.Success);
            Assert.False(string.IsNullOrEmpty(result.Message));
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingMovieWithAdminRole()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.Admin)).ReturnsAsync(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingMovieWithSystemRole()
        {
            PrincipalMock.Setup(x => x.Identity.Name).Returns("sys");
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.Admin)).ReturnsAsync(false);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingTVWithAdminRole()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.Admin)).ReturnsAsync(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingTVWithTVRole()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.RequestTv)).ReturnsAsync(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnFail_WhenRequestingTVWithoutTVRole()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.RequestTv)).ReturnsAsync(false);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.False(result.Success);
            Assert.False(string.IsNullOrEmpty(result.Message));
        }
    }
}
