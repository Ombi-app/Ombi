using System.Security.Principal;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Core.Rule.Rules;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Helpers;
using Ombi.Store.Entities.Requests;

namespace Ombi.Core.Tests.Rule.Request
{
    public class CanRequestRuleTests
    {
        [SetUp]
        public void Setup()
        {

            PrincipalMock = new Mock<IPrincipal>();
            Rule = new CanRequestRule(PrincipalMock.Object, null);
        }


        private CanRequestRule Rule { get; set; }
        private Mock<IPrincipal> PrincipalMock { get; set; }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingMovieWithMovieRole()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.RequestMovie)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnFail_WhenRequestingMovieWithoutMovieRole()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.RequestMovie)).Returns(false);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.False(result.Success);
            Assert.False(string.IsNullOrEmpty(result.Message));
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingMovieWithAdminRole()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.Admin)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.Movie };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingTVWithAdminRole()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.Admin)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnSuccess_WhenRequestingTVWithTVRole()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.RequestTv)).Returns(true);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.True(result.Success);
        }

        [Test]
        public async Task Should_ReturnFail_WhenRequestingTVWithoutTVRole()
        {
            PrincipalMock.Setup(x => x.IsInRole(OmbiRoles.RequestTv)).Returns(false);
            var request = new BaseRequest() { RequestType = Store.Entities.RequestType.TvShow };
            var result = await Rule.Execute(request);

            Assert.False(result.Success);
            Assert.False(string.IsNullOrEmpty(result.Message));
        }
    }
}
