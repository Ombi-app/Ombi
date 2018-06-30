using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Hangfire;
using Moq;
using NUnit.Framework;
using Ombi.Core.Notifications;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class PlexAvailabilityCheckerTests
    {
        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IPlexContentRepository>();
            _tv = new Mock<ITvRequestRepository>();
            _movie = new Mock<IMovieRequestRepository>();
            _notify = new Mock<INotificationService>();
            Checker = new PlexAvailabilityChecker(_repo.Object, _tv.Object, _movie.Object, _notify.Object, new Mock<IBackgroundJobClient>().Object, null);
        }


        private Mock<IPlexContentRepository> _repo;
        private Mock<ITvRequestRepository> _tv;
        private Mock<IMovieRequestRepository> _movie;
        private Mock<INotificationService> _notify;
        private PlexAvailabilityChecker Checker;

        [Test]
        public async Task ProcessMovies_ShouldMarkAvailable_WhenInPlex()
        {
            var request = new MovieRequests
            {
                ImdbId = "test"
            };
            _movie.Setup(x => x.GetAll()).Returns(new List<MovieRequests> { request }.AsQueryable());
            _repo.Setup(x => x.Get("test")).ReturnsAsync(new PlexServerContent());

            await Checker.Start();

            _movie.Verify(x => x.Save(), Times.Once);

            Assert.True(request.Available);
        }

        [Test]
        public async Task ProcessMovies_ShouldNotBeAvailable_WhenInNotPlex()
        {
            var request = new MovieRequests
            {
                ImdbId = "test"
            };
            _movie.Setup(x => x.GetAll()).Returns(new List<MovieRequests> { request }.AsQueryable());

            await Checker.Start();
            
            Assert.False(request.Available);
        }

        [Test]
        [Ignore("EF IAsyncQueryProvider")]
        public async Task ProcessTv_ShouldMark_Episode_Available_WhenInPlex()
        {
            var request = new ChildRequests
            {
                ParentRequest = new TvRequests { TvDbId = 1 },
                SeasonRequests = new EditableList<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new EditableList<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                EpisodeNumber = 1,
                                Season =  new SeasonRequests
                                {
                                    SeasonNumber = 2
                                }
                            }
                        }
                    }
                }
            };
            _tv.Setup(x => x.GetChild()).Returns(new List<ChildRequests> { request }.AsQueryable());
            _repo.Setup(x => x.GetAllEpisodes()).Returns(new List<PlexEpisode>
            {
                new PlexEpisode
                {
                    Series = new  PlexServerContent
                    {
                        ImdbId = 1.ToString(),
                    },
                    EpisodeNumber = 1,
                    SeasonNumber = 2
                }
            }.AsQueryable);
            _repo.Setup(x => x.Include(It.IsAny<IQueryable<PlexEpisode>>(),It.IsAny<Expression<Func<PlexEpisode, PlexServerContent>>>()));

            await Checker.Start();

            _tv.Verify(x => x.Save(), Times.Once);

            Assert.True(request.SeasonRequests[0].Episodes[0].Available);

        }
    }
}