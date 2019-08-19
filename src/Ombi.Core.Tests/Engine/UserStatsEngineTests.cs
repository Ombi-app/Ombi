

using System;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using Ombi.Core.Engine;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class UserStatsEngineTests
    {
        private UserStatsEngine _engine;
        private Mock<IMovieRequestRepository> _movieRequestRepo;
        private Mock<ITvRequestRepository> _tvRequestRepo;

        [SetUp]
        public void Setup()
        {
            _movieRequestRepo = new Mock<IMovieRequestRepository>();
            _tvRequestRepo = new Mock<ITvRequestRepository>();
            _engine = new UserStatsEngine(_movieRequestRepo.Object, _tvRequestRepo.Object);
        }

        [Test]
        [Ignore("Needs some more work")]
        public async Task Test()
        {
            var f = new Fixture();
            f.Build<MovieRequests>().With(x => x.Available, true).CreateMany(200);
            var result = await _engine.GetSummary(new SummaryRequest
            {
                From = new DateTime(2019,08,01),
                To = new DateTime(2019,09,01)
            });

            Assert.That(result.CompletedRequests, Is.EqualTo(200));
            Assert.That(result.TotalRequests, Is.EqualTo(400));
            Assert.That(result.TotalMovieRequests, Is.EqualTo(200));
            Assert.That(result.TotalTvRequests, Is.EqualTo(200));
            Assert.That(result.CompletedRequestsMovies, Is.EqualTo(100));
            Assert.That(result.CompletedRequestsTv, Is.EqualTo(100));
        }
    }
}