//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Moq;
//using Ombi.Core.Engine;
//using Ombi.Core.Models.Requests;
//using Ombi.Store.Entities.Requests;
//using Ombi.Store.Repository.Requests;
//using Xunit;

//namespace Ombi.Core.Tests.Engine
//{
//    public class TvRequestEngineTests
//    {
//        public TvRequestEngineTests()
//        {
//            RequestService = new Mock<ITvRequestRepository>();
//            var requestService = new RequestService(RequestService.Object, null);
//            Engine = new TvRequestEngine(null, requestService, null, null, null, null, null, null);
//        }

//        private TvRequestEngine Engine { get; }
//        private Mock<ITvRequestRepository> RequestService { get; }

//        //[Fact]
//        //public async Task GetNewRequests_Should_ReturnEmpty_WhenThereAreNoNewRequests()
//        //{
//        //    var requests = new List<TvRequests>
//        //    {
//        //        new TvRequests { Available = true },
//        //        new TvRequests { Approved = true },
//        //    };
//        //    RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//        //    var result = await Engine.GetNewRequests();

//        //    Assert.False(result.Any());
//        //}

//        //[Fact]
//        //public async Task GetNewRequests_Should_ReturnOnlyNewRequests_WhenThereAreMultipleRequests()
//        //{
//        //    var requests = new List<TvRequestModel>
//        //    {
//        //        new TvRequestModel { Available = true },
//        //        new TvRequestModel { Approved = true },
//        //        new TvRequestModel(),
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

//        [Fact]
//        public async Task GetApprovedRequests_Should_ReturnEmpty_WhenThereAreNoApprovedRequests()
//        {
//            var requests = new List<TvRequestModel>
//            {
//                new TvRequestModel { Available = true },
//            };
//            RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//            var result = await Engine.GetApprovedRequests();

//            Assert.False(result.Any());
//        }

//        [Fact]
//        public async Task GetApprovedRequests_Should_ReturnOnlyApprovedRequests_WhenThereAreMultipleRequests()
//        {
//            var requests = new List<TvRequestModel>
//            {
//                new TvRequestModel { Available = true },
//                new TvRequestModel { Approved = true },
//                new TvRequestModel(),
//            };
//            RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//            var result = await Engine.GetApprovedRequests();

//            Assert.Equal(result.Count(), 1);
//            Assert.All(result, x =>
//            {
//                Assert.Equal(x.Available, false);
//                Assert.Equal(x.Approved, true);
//            });
//        }

//        [Fact]
//        public async Task GetAvailableRequests_Should_ReturnEmpty_WhenThereAreNoAvailableRequests()
//        {
//            var requests = new List<TvRequestModel>
//            {
//                new TvRequestModel { Approved = true },
//            };
//            RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//            var result = await Engine.GetAvailableRequests();

//            Assert.False(result.Any());
//        }

//        [Fact]
//        public async Task GetAvailableRequests_Should_ReturnOnlyAvailableRequests_WhenThereAreMultipleRequests()
//        {
//            var requests = new List<TvRequestModel>
//            {
//                new TvRequestModel { Available = true },
//                new TvRequestModel { Approved = true },
//                new TvRequestModel(),
//            };
//            RequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(requests);

//            var result = await Engine.GetAvailableRequests();

//            Assert.Equal(result.Count(), 1);
//            Assert.All(result, x =>
//            {
//                Assert.Equal(x.Available, true);
//                Assert.Equal(x.Approved, false);
//            });
//        }
//    }
//}