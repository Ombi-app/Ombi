//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Moq;
//using Ombi.Core.Engine;
//using Ombi.Core.Models.Requests;
//using Ombi.Store.Entities.Requests;
//using Ombi.Store.Repository;
//using Xunit;

//namespace Ombi.Core.Tests.Engine
//{
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

//        [Fact]
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

//        //[Fact]
//        //public async Task GetNewRequests_Should_ReturnOnlyNewRequests_WhenThereAreMultipleRequests()
//        //{
//        //    var requests = new List<MovieRequestModel>
//        //    {
//        //        new MovieRequestModel { Available = true },
//        //        new MovieRequestModel { Approved = true },
//        //        new MovieRequestModel(),
//        //    };
//        //    RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//        //    var result = await Engine.GetNewRequests();

//        //    Assert.Equal(result.Count(), 1);
//        //    Assert.All(result, x =>
//        //    {
//        //        Assert.Equal(x.Available, false);
//        //        Assert.Equal(x.Approved, false);
//        //    });
//        //}

//        //[Fact]
//        //public async Task GetApprovedRequests_Should_ReturnEmpty_WhenThereAreNoApprovedRequests()
//        //{
//        //    var requests = new List<MovieRequestModel>
//        //    {
//        //        new MovieRequestModel { Available = true },
//        //    };
//        //    RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//        //    var result = await Engine.GetApprovedRequests();

//        //    Assert.False(result.Any());
//        //}

//        //[Fact]
//        //public async Task GetApprovedRequests_Should_ReturnOnlyApprovedRequests_WhenThereAreMultipleRequests()
//        //{
//        //    var requests = new List<MovieRequestModel>
//        //    {
//        //        new MovieRequestModel { Available = true },
//        //        new MovieRequestModel { Approved = true },
//        //        new MovieRequestModel(),
//        //    };
//        //    RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//        //    var result = await Engine.GetApprovedRequests();

//        //    Assert.Equal(result.Count(), 1);
//        //    Assert.All(result, x =>
//        //    {
//        //        Assert.Equal(x.Available, false);
//        //        Assert.Equal(x.Approved, true);
//        //    });
//        //}

//        //[Fact]
//        //public async Task GetAvailableRequests_Should_ReturnEmpty_WhenThereAreNoAvailableRequests()
//        //{
//        //    var requests = new List<MovieRequestModel>
//        //    {
//        //        new MovieRequestModel { Approved = true },
//        //    };
//        //    RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//        //    var result = await Engine.GetAvailableRequests();

//        //    Assert.False(result.Any());
//        //}

//        //[Fact]
//        //public async Task GetAvailableRequests_Should_ReturnOnlyAvailableRequests_WhenThereAreMultipleRequests()
//        //{
//        //    var requests = new List<MovieRequestModel>
//        //    {
//        //        new MovieRequestModel { Available = true },
//        //        new MovieRequestModel { Approved = true },
//        //        new MovieRequestModel(),
//        //    };
//        //    RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//        //    var result = await Engine.GetAvailableRequests();

//        //    Assert.Equal(result.Count(), 1);
//        //    Assert.All(result, x =>
//        //    {
//        //        Assert.Equal(x.Available, true);
//        //        Assert.Equal(x.Approved, false);
//        //    });
//        //}
//    }
//}