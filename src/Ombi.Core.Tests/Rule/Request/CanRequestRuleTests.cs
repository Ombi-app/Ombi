//using System.Security.Principal;
//using System.Threading.Tasks;
//using Moq;
//using Ombi.Core.Claims;
//using Ombi.Core.Models.Requests;
//using Ombi.Core.Rule.Rules;
//using Xunit;

//namespace Ombi.Core.Tests.Rule
//{
//    public class CanRequestRuleTests
//    {
//        public CanRequestRuleTests()
//        {
//            PrincipalMock = new Mock<IPrincipal>();
//            Rule = new CanRequestRule(PrincipalMock.Object);
//        }

//        private CanRequestRule Rule { get; }
//        private Mock<IPrincipal> PrincipalMock { get; }

//        [Fact]
//        public async Task Should_ReturnSuccess_WhenRequestingMovieWithMovieRole()
//        {
//            PrincipalMock.Setup(x => x.IsInRole(OmbiClaims.RequestMovie)).Returns(true);
//            var request = new BaseRequestModel() { Type = Store.Entities.RequestType.Movie };
//            var result = await Rule.Execute(request);

//            Assert.Equal(result.Success, true);
//        }

//        [Fact]
//        public async Task Should_ReturnFail_WhenRequestingMovieWithoutMovieRole()
//        {
//            PrincipalMock.Setup(x => x.IsInRole(OmbiClaims.RequestMovie)).Returns(false);
//            var request = new BaseRequestModel() { Type = Store.Entities.RequestType.Movie };
//            var result = await Rule.Execute(request);

//            Assert.Equal(result.Success, false);
//            Assert.Equal(string.IsNullOrEmpty(result.Message), false);
//        }

//        [Fact]
//        public async Task Should_ReturnSuccess_WhenRequestingMovieWithAdminRole()
//        {
//            PrincipalMock.Setup(x => x.IsInRole(OmbiClaims.Admin)).Returns(true);
//            var request = new BaseRequestModel() { Type = Store.Entities.RequestType.Movie };
//            var result = await Rule.Execute(request);

//            Assert.Equal(result.Success, true);
//        }

//        [Fact]
//        public async Task Should_ReturnSuccess_WhenRequestingTVWithAdminRole()
//        {
//            PrincipalMock.Setup(x => x.IsInRole(OmbiClaims.Admin)).Returns(true);
//            var request = new BaseRequestModel() { Type = Store.Entities.RequestType.TvShow };
//            var result = await Rule.Execute(request);

//            Assert.Equal(result.Success, true);
//        }

//        [Fact]
//        public async Task Should_ReturnSuccess_WhenRequestingTVWithTVRole()
//        {
//            PrincipalMock.Setup(x => x.IsInRole(OmbiClaims.RequestTv)).Returns(true);
//            var request = new BaseRequestModel() { Type = Store.Entities.RequestType.TvShow };
//            var result = await Rule.Execute(request);

//            Assert.Equal(result.Success, true);
//        }

//        [Fact]
//        public async Task Should_ReturnFail_WhenRequestingTVWithoutTVRole()
//        {
//            PrincipalMock.Setup(x => x.IsInRole(OmbiClaims.RequestTv)).Returns(false);
//            var request = new BaseRequestModel() { Type = Store.Entities.RequestType.TvShow };
//            var result = await Rule.Execute(request);

//            Assert.Equal(result.Success, false);
//            Assert.Equal(string.IsNullOrEmpty(result.Message), false);
//        }
//    }
//}
