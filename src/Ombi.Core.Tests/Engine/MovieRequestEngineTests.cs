//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Moq;
//using NUnit.Framework;
//using Ombi.Core.Engine;
//using Ombi.Core.Models.Requests;
//using Ombi.Store.Entities.Requests;
//using Ombi.Store.Repository;
//using Assert = Xunit.Assert;

//namespace Ombi.Core.Tests.Engine
//{
//    [TestFixture]
//    public class MovieRequestEngineTests
//    {
//        public MovieRequestEngineTests()
//        {
//            RequestService = new Mock<IMovieRequestRepository>();
//            var requestService = new RequestService(null, RequestService.Object);
//            Engine = new MovieRequestEngine(null, requestService, null, null, null, null, null, null);
//        }

//        private MovieRequestEngine Engine { get; }
//        private Mock<IMovieRequestRepository> RequestService { get; }

//        [Test]
//        public async Task GetNewRequests_Should_ReturnEmpty_WhenThereAreNoNewRequests()
//        {
//            var requests = new List<MovieRequests>
//            {
//                new MovieRequests { Available = true },
//                new MovieRequests { Approved = true },
//            };

//            var r = DbHelper.GetQueryable(requests[0], requests[1]);
//            RequestService.Setup(x => x.Get()).Returns(r);

//            var result = await Engine.GetNewRequests();

//            Assert.False(result.Any());
//        }

//        [Test]
//        public async Task GetNewRequests_Should_ReturnOnlyNewRequests_WhenThereAreMultipleRequests()
//        {
//            var requests = new List<MovieRequests>
//            {
//                new MovieRequests { Available = true },
//                new MovieRequests { Approved = true },
//                new MovieRequests(),
//            };
//            RequestService.Setup(x => x.Get()).Returns(requests.AsQueryable);

//            var result = await Engine.GetNewRequests();

//            Assert.Single(result);
//            Assert.All(result, x =>
//            {
//                Assert.False(x.Available);
//                Assert.False(x.Approved);
//            });
//        }

//        [Test]
//        public async Task GetApprovedRequests_Should_ReturnEmpty_WhenThereAreNoApprovedRequests()
//        {
//            var requests = new List<MovieRequests>
//            {
//                new MovieRequests { Available = true },
//            };
//            RequestService.Setup(x => x.Get()).Returns(requests.AsQueryable);

//            var result = await Engine.GetApprovedRequests();

//            Assert.False(result.Any());
//        }

//        [Test]
//        public async Task GetApprovedRequests_Should_ReturnOnlyApprovedRequests_WhenThereAreMultipleRequests()
//        {
//            var requests = new List<MovieRequests>
//            {
//                new MovieRequests { Available = true },
//                new MovieRequests { Approved = true },
//                new MovieRequests(),
//            };
//            RequestService.Setup(x => x.Get()).Returns(requests.AsQueryable);

//            var result = await Engine.GetApprovedRequests();

//            Assert.Single(result);
//            Assert.All(result, x =>
//            {
//                Assert.False(x.Available);
//                Assert.True(x.Approved);
//            });
//        }

//        [Test]
//        public async Task GetAvailableRequests_Should_ReturnEmpty_WhenThereAreNoAvailableRequests()
//        {
//            var requests = new List<MovieRequests>
//            {
//                new MovieRequests { Approved = true },
//            };
//            RequestService.Setup(x => x.Get()).Returns(requests.AsQueryable);

//            var result = await Engine.GetAvailableRequests();

//            Assert.False(result.Any());
//        }

//        [Test]
//        public async Task GetAvailableRequests_Should_ReturnOnlyAvailableRequests_WhenThereAreMultipleRequests()
//        {
//            var requests = new List<MovieRequests>
//            {
//                new MovieRequests { Available = true },
//                new MovieRequests { Approved = true },
//                new MovieRequests(),
//            };
//            RequestService.Setup(x => x.Get()).Returns(requests.AsQueryable);

//            var result = await Engine.GetAvailableRequests();

//            Assert.Single(result);
//            Assert.All(result, x =>
//            {
//                Assert.True(x.Available);
//                Assert.False(x.Approved);
//            });
//        }
//    }
//}