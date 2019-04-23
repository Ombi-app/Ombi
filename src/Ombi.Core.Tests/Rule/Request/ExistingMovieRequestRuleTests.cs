using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Rule.Rules;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;
using Ombi.Test.Common;

namespace Ombi.Core.Tests.Rule.Request
{
    public class ExistingMovieRequestRuleTests
    {

        [SetUp]
        public void Setup()
        {
            ContextMock = new Mock<IMovieRequestRepository>();
            Rule = new ExistingMovieRequestRule(ContextMock.Object);
        }


        private ExistingMovieRequestRule Rule { get; set; }
        private Mock<IMovieRequestRepository> ContextMock { get; set; }

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
            }.AsQueryable().BuildMock().Object);
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
            }.AsQueryable().BuildMock().Object);
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
            }.AsQueryable().BuildMock().Object);
            var o = new MovieRequests
            {
                TheMovieDbId = 1,
                ImdbId = "1"
            };
            var result = await Rule.Execute(o);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.Null.Or.Empty);
        }
    }
}
