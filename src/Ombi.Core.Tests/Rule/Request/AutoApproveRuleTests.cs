using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Store.Entities.Requests;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Test.Common;
using System.Collections.Generic;
using Ombi.Store.Entities;
using System;
using Ombi.Core.Services;

namespace Ombi.Core.Tests.Rule.Request
{
    [TestFixture]
    public class AutoApproveRuleTests
    {
        private List<OmbiUser> _users = new List<OmbiUser>
        {
             new OmbiUser { Id = Guid.NewGuid().ToString("N"), UserName="abc", NormalizedUserName = "ABC", UserType =  UserType.LocalUser},
             new OmbiUser { Id = Guid.NewGuid().ToString("N"), UserName="Sys", NormalizedUserName = "SYS", UserType =  UserType.SystemUser}
        };

        [SetUp]
        public void Setup()
        {

            PrincipalMock = new Mock<IPrincipal>();
            PrincipalMock.Setup(x => x.Identity.Name).Returns("abc");
            FeatureService = new Mock<IFeatureService>();

            UserManager = MockHelper.MockUserManager(_users);
            Rule = new AutoApproveRule(PrincipalMock.Object, UserManager.Object, FeatureService.Object);
        }


        private AutoApproveRule Rule { get; set; }
        private Mock<IPrincipal> PrincipalMock { get; set; }
        private Mock<OmbiUserManager> UserManager { get; set; }
        private Mock<IFeatureService> FeatureService { get; set; }

        [Test]
        public async Task Should_ReturnSuccess_WhenAdminAndRequestMovie()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.Admin)).ReturnsAsync(true);
            var request = new MovieRequests() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenAdminAndRequestTV()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.Admin)).ReturnsAsync(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenAutoApproveMovieAndRequestMovie()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.AutoApproveMovie)).ReturnsAsync(true);
            var request = new MovieRequests() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnFail_WhenAutoApproveMovie_And_RequestTV()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.AutoApproveMovie)).ReturnsAsync(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenAutoApproveTVAndRequestTV()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.AutoApproveTv)).ReturnsAsync(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenSystemUserAndRequestTV()
        {
            PrincipalMock.Setup(x => x.Identity.Name).Returns("sys");
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.AutoApproveTv)).ReturnsAsync(false);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnFail_WhenAutoApproveTV_And_RequestMovie()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), OmbiRoles.AutoApproveTv)).ReturnsAsync(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }

        [Test]
        public async Task Should_ReturnFail_WhenNoClaimsAndRequestMovie()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(false);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }

        [Test]
        public async Task Should_ReturnFail_WhenNoClaimsAndRequestTV()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(false);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }

        [Test]
        public async Task Should_ReturnFail_When4kRequestAndFeatureNotEnabled()
        {
            UserManager.Setup(x => x.IsInRoleAsync(It.IsAny<OmbiUser>(), It.IsAny<string>())).ReturnsAsync(false);
            var request = new MovieRequests() { RequestType = Store.Entities.RequestType.Movie, Is4kRequest = true };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
            Assert.False(request.Approved4K);
        }
    }
}
