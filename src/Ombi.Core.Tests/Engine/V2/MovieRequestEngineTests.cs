using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ombi.Api.TheMovieDb;
using Ombi.Core.Engine;
using Ombi.Core.Models.Requests;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Engine.V2
{
    [TestFixture]
    public class MovieRequestEngineTests
    {
        private MovieRequestEngine _engine;
        private Mock<IMovieRequestRepository> _movieRequestRepository;
        [SetUp]
        public void Setup()
        {
            var movieApi = new Mock<IMovieDbApi>();
            var requestService = new Mock<IRequestServiceMain>();
            _movieRequestRepository = new Mock<IMovieRequestRepository>();
            requestService.Setup(x => x.MovieRequestService).Returns(_movieRequestRepository.Object);
            var user = new Mock<IPrincipal>();
            var notificationHelper = new Mock<INotificationHelper>();
            var rules = new Mock<IRuleEvaluator>();
            var movieSender = new Mock<IMovieSender>();
            var logger = new Mock<ILogger<MovieRequestEngine>>();
            var userManager = MockHelper.MockUserManager(new List<OmbiUser>());
            var requestLogRepo = new Mock<IRepository<RequestLog>>();
            var cache = new Mock<ICacheService>();
            var ombiSettings = new Mock<ISettingsService<OmbiSettings>>();
            var requestSubs = new Mock<IRepository<RequestSubscription>>();
            _engine = new MovieRequestEngine(movieApi.Object, requestService.Object, user.Object, notificationHelper.Object, rules.Object, movieSender.Object,
                logger.Object, userManager.Object, requestLogRepo.Object, cache.Object, ombiSettings.Object, requestSubs.Object);
        }

        [Test]
        [Ignore("Needs to be tested")]
        public async Task Get_UnavailableRequests()
        {
            _movieRequestRepository.Setup(x => x.GetWithUser()).Returns(new List<MovieRequests>
            {
                new MovieRequests
                {
                    Available = true
                },
                new MovieRequests
                {
                    Available = false,
                    Approved = false
                },
                new MovieRequests
                {
                    Available = false,
                    Approved = true,
                    Title = "Come get me"
                }
            }.AsQueryable());
            var result = await _engine.GetUnavailableRequests(100, 0, "RequestedDate", "asc");

            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Collection.FirstOrDefault().Title, Is.EqualTo("Come get me"));
        }
    }
}