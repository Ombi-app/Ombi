using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Moq;
using NUnit.Framework;
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
        public PlexAvailabilityCheckerTests()
        {
            _repo = new Mock<IPlexContentRepository>();
            _tv = new Mock<ITvRequestRepository>();
            _movie = new Mock<IMovieRequestRepository>();
            Checker = new PlexAvailabilityChecker(_repo.Object, _tv.Object, _movie.Object);
        }

        private readonly Mock<IPlexContentRepository> _repo;
        private readonly Mock<ITvRequestRepository> _tv;
        private readonly Mock<IMovieRequestRepository> _movie;
        private PlexAvailabilityChecker Checker { get; }

        [Test]
        public async Task ProcessMovies_ShouldMarkAvailable_WhenInPlex()
        {
            var request = new MovieRequests
            {
                ImdbId = "test"
            };
            _movie.Setup(x => x.Get()).Returns(new List<MovieRequests> { request }.AsQueryable());
            _repo.Setup(x => x.Get("test")).ReturnsAsync(new PlexContent());

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
            _movie.Setup(x => x.Get()).Returns(new List<MovieRequests> { request }.AsQueryable());

            await Checker.Start();

            _movie.Verify(x => x.Save(), Times.Once);
            Assert.False(request.Available);
        }

        [Test]
        [Ignore("EF IAsyncQueryProvider")]
        public async Task ProcessTv_ShouldMark_Episode_Available_WhenInPlex()
        {
            var request = new ChildRequests
            {
                ParentRequest = new TvRequests {TvDbId = 1},
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
                    Series = new  PlexContent
                    {
                        ProviderId = 1.ToString(),
                    },
                    EpisodeNumber = 1,
                    SeasonNumber = 2
                }
            }.AsQueryable);

            await Checker.Start();

            _tv.Verify(x => x.Save(), Times.Once);

            Assert.True(request.SeasonRequests[0].Episodes[0].Available);
            
        }
    }
}