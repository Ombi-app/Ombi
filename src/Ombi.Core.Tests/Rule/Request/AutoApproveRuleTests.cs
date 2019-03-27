using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Store.Entities.Requests;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Helpers;

namespace Ombi.Core.Tests.Rule.Request
{
    [TestFixture]
    public class AutoApproveRuleTests
    {
        [SetUp]
        public void Setup()
        {

            PrincipalMock = new Mock<IPrincipal>();
            Rule = new AutoApproveRule(PrincipalMock.Object, null);
        }


        private AutoApproveRule Rule { get; set; }
        private Mock<IPrincipal> PrincipalMock { get; set; }

        [Test]
        public async Task Should_ReturnSuccess_WhenAdminAndRequestMovie()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.Admin)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenAdminAndRequestTV()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.Admin)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenAutoApproveMovieAndRequestMovie()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.AutoApproveMovie)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenAutoApproveTVAndRequestTV()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.AutoApproveTv)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.True(request.Approved);
        }

        [Test]
        public async Task Should_ReturnFail_WhenNoClaimsAndRequestMovie()
        {
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }

        [Test]
        public async Task Should_ReturnFail_WhenNoClaimsAndRequestTV()
        {
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
            Assert.False(request.Approved);
        }
    }
}
