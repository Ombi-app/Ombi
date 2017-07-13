using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using Ombi.Core.Claims;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Store.Entities.Requests;
using Xunit;

namespace Ombi.Core.Tests.Rule.Request
{
    public class AutoApproveRuleTests
    {
        public AutoApproveRuleTests()
        {
            PrincipalMock = new Mock<IPrincipal>();
            Rule = new AutoApproveRule(PrincipalMock.Object);
        }

        private AutoApproveRule Rule { get; }
        private Mock<IPrincipal> PrincipalMock { get; }

        [Fact]
        public async Task Should_ReturnSuccess_WhenAdminAndRequestMovie()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.Admin)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.Equal(result.Success, true);
            Assert.Equal(request.Approved, true);
        }

        [Fact]
        public async Task Should_ReturnSuccess_WhenAdminAndRequestTV()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.Admin)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.Equal(result.Success, true);
            Assert.Equal(request.Approved, true);
        }

        [Fact]
        public async Task Should_ReturnSuccess_WhenAutoApproveMovieAndRequestMovie()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.AutoApproveMovie)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.Equal(result.Success, true);
            Assert.Equal(request.Approved, true);
        }

        [Fact]
        public async Task Should_ReturnSuccess_WhenAutoApproveTVAndRequestTV()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.AutoApproveTv)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.Equal(result.Success, true);
            Assert.Equal(request.Approved, true);
        }

        [Fact]
        public async Task Should_ReturnFail_WhenNoClaimsAndRequestMovie()
        {
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.Equal(result.Success, true);
            Assert.Equal(request.Approved, false);
        }

        [Fact]
        public async Task Should_ReturnFail_WhenNoClaimsAndRequestTV()
        {
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.Equal(result.Success, true);
            Assert.Equal(request.Approved, false);
        }
    }
}
