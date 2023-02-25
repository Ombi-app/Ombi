using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Core.Services;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Rule.Request
{
    public class ExistingMovieRequestRuleTests
    {

        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IMovieRequestRepository>();
            FeatureService = new Mock<IFeatureService>();
            Rule = new ExistingMovieRequestRule(ContextMock.Object, FeatureService.Object);
        }


        private ExistingMovieRequestRule Rule { get; set; }
        private Mock<IMovieRequestRepository> ContextMock { get; set; }
        private Mock<IFeatureService> FeatureService { get; set; }

        [Test]
        public async Task ExistingRequestRule_Movie_Has_Been_Requested_With_TheMovieDBId()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<MovieRequests>
            {
                new MovieRequests
                {
                    TheMovieDbId = 1,
                    RequestType = RequestType.Movie
                }
            }.AsQueryable().BuildMock());
            var o = new MovieRequests
            {
                TheMovieDbId = 1,
            };
            var result = await Rule.Execute(o);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.Not.Empty);
        }

        [Test]
        public async Task ExistingRequestRule_Movie_Has_Been_Requested_With_ImdbId()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<MovieRequests>
            {
                new MovieRequests
                {
                    TheMovieDbId = 11111,
                    ImdbId = 1.ToString(),
                    RequestType = RequestType.Movie
                }
            }.AsQueryable().BuildMock());
            var o = new MovieRequests
            {
                ImdbId = 1.ToString(),
            };
            var result = await Rule.Execute(o);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.Not.Empty);
        }

        [Test]
        public async Task ExistingRequestRule_Movie_HasNot_Been_Requested()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<MovieRequests>
            {
                new MovieRequests
                {
                    TheMovieDbId = 2,
                    ImdbId = "2",
                    RequestType = RequestType.Movie
                }
            }.AsQueryable().BuildMock());
            var o = new MovieRequests
            {
                TheMovieDbId = 1,
                ImdbId = "1"
            };
            var result = await Rule.Execute(o);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.Null.Or.Empty);
        }

        [Test]
        public async Task ExistingRequestRule_Movie_HasAlready4K_Request()
        {
            ContextMock.Setup(x => x.GetAll()).Returns(new List<MovieRequests>
            {
                new MovieRequests
                {
                    TheMovieDbId = 2,
                    ImdbId = "2",
                    RequestType = RequestType.Movie,
                    Is4kRequest = true
                }
            }.AsQueryable().BuildMock());
            var o = new MovieRequests
            {
                TheMovieDbId = 2,
                ImdbId = "1",
                Has4KRequest = true
            };
            var result = await Rule.Execute(o);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.Not.Empty);
        }

        [Test]
        public async Task ExistingRequestRule_Movie_4K_Request()
        {
            FeatureService.Setup(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(true);
            ContextMock.Setup(x => x.GetAll()).Returns(new List<MovieRequests>
            {
                new MovieRequests
                {
                    TheMovieDbId = 2,
                    ImdbId = "2",
                    RequestType = RequestType.Movie,
                    Is4kRequest = false
                }
            }.AsQueryable().BuildMock());
            var o = new MovieRequests
            {
                TheMovieDbId = 2,
                ImdbId = "1",
                Is4kRequest = true
            };
            var result = await Rule.Execute(o);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.Null.Or.Empty);
        }

        [Test]
        public async Task ExistingRequestRule_Movie_4K_Request_FeatureNotEnabled()
        {
            FeatureService.Setup(x => x.FeatureEnabled(FeatureNames.Movie4KRequests)).ReturnsAsync(false);
            ContextMock.Setup(x => x.GetAll()).Returns(new List<MovieRequests>
            {
                new MovieRequests
                {
                    TheMovieDbId = 2,
                    ImdbId = "2",
                    RequestType = RequestType.Movie,
                    Is4kRequest = false
                }
            }.AsQueryable().BuildMock());
            var o = new MovieRequests
            {
                TheMovieDbId = 2,
                ImdbId = "1",
                Is4kRequest = true
            };
            var result = await Rule.Execute(o);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.Not.Null);
        }
    }
}
