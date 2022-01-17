using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Rule.Request
{
    [TestFixture]
    public class ExistingPlexRequestRuleTests
    {
        private ExistingPlexRequestRule Rule;
        private Mock<IPlexContentRepository> PlexContentRepo;

        [SetUp]
        public void SetUp()
        {
            PlexContentRepo = new Mock<IPlexContentRepository>();
            Rule = new ExistingPlexRequestRule(PlexContentRepo.Object);
        }

        [Test]
        public async Task RequestShow_DoesNotExistAtAll_IsSuccessful()
        {
            PlexContentRepo.Setup(x => x.GetAll()).Returns(new List<PlexServerContent>().AsQueryable().BuildMock().Object);
            var req = new ChildRequests
            {
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                               Id = 1,
                               EpisodeNumber = 1,
                            }
                        },
                        SeasonNumber = 1
                    }
                }
            };
            var result = await Rule.Execute(req);


            Assert.That(result.Success, Is.True);
        }

        [Test]
        public async Task RequestShow_AllEpisodesAreaRequested_IsNotSuccessful()
        {
            SetupMockData();

            var req = new ChildRequests
            {
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                               Id = 1,
                               EpisodeNumber = 1,
                            },
                            new EpisodeRequests
                            {
                               Id = 1,
                               EpisodeNumber = 2,
                            },
                        },
                        SeasonNumber = 1
                    }
                },
                Id = 1,
            };
            var result = await Rule.Execute(req);


            Assert.That(result.Success, Is.False);
        }


        [Test]
        public async Task RequestShow_SomeEpisodesAreaRequested_IsSuccessful()
        {
            SetupMockData();

            var req = new ChildRequests
            {
                RequestType = RequestType.TvShow,
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                               Id = 1,
                               EpisodeNumber = 1,
                            },
                            new EpisodeRequests
                            {
                               Id = 2,
                               EpisodeNumber = 2,
                            },
                            new EpisodeRequests
                            {
                               Id = 3,
                               EpisodeNumber = 3,
                            },
                        },
                        SeasonNumber = 1
                    }
                },
                Id = 1,
            };
            var result = await Rule.Execute(req);


            Assert.That(result.Success, Is.True);

            var episodes = req.SeasonRequests.SelectMany(x => x.Episodes);
            Assert.That(episodes.Count() == 1, "We didn't remove the episodes that have already been requested!");
            Assert.That(episodes.First().EpisodeNumber == 3, "We removed the wrong episode");
        }

        [Test]
        public async Task RequestShow_NewSeasonRequest_IsSuccessful()
        {
            SetupMockData();

            var req = new ChildRequests
            {
                RequestType = RequestType.TvShow,
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                               Id = 1,
                               EpisodeNumber = 1,
                            },
                            new EpisodeRequests
                            {
                               Id = 2,
                               EpisodeNumber = 2,
                            },
                            new EpisodeRequests
                            {
                               Id = 3,
                               EpisodeNumber = 3,
                            },
                        },
                        SeasonNumber = 2
                    }
                },
                Id = 1,
            };
            var result = await Rule.Execute(req);

            Assert.That(result.Success, Is.True);
        }

        private void SetupMockData()
        {
            var childRequests = new List<PlexServerContent>
            {
                new PlexServerContent
                {
                    Type = MediaType.Series,
                    TheMovieDbId = "1",
                    Title = "Test",
                    ReleaseYear = "2001",
                    Episodes = new List<IMediaServerEpisode>
                    {
                        new PlexEpisode
                        {
                            EpisodeNumber = 1,
                            Id = 1,
                            SeasonNumber = 1,
                        },
                        new PlexEpisode
                        {
                            EpisodeNumber = 2,
                            Id = 2,
                            SeasonNumber = 1,
                        },
                    }
                }
            };
            PlexContentRepo.Setup(x => x.GetAll()).Returns(childRequests.AsQueryable().BuildMock().Object);
        }
    }
}
