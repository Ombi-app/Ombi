using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using Ombi.Core.Rule.Rules.Request;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Rule.Request
{
    [TestFixture]
    public class ExistingTvRequestRuleTests
    {
        private ExistingTvRequestRule Rule;
        private Mock<ITvRequestRepository> TvRequestRepo;

        [SetUp]
        public void SetUp()
        {
            TvRequestRepo = new Mock<ITvRequestRepository>();
            Rule = new ExistingTvRequestRule(TvRequestRepo.Object);
        }

        [Test]
        public async Task RequestShow_DoesNotExistAtAll_IsSuccessful()
        {
            TvRequestRepo.Setup(x => x.GetChild()).Returns(new List<ChildRequests>().AsQueryable().BuildMock().Object);
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
            var childRequests = new List<ChildRequests>
            {
                new ChildRequests
                {
                    ParentRequest = new TvRequests
                    {
                        Id = 1,
                        ExternalProviderId = 1,
                    },
                    SeasonRequests = new List<SeasonRequests>
                    {
                        new SeasonRequests
                        {
                            Id = 1,
                            SeasonNumber = 1,
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
                                }
                            }
                        }
                    }
                }
            };
            TvRequestRepo.Setup(x => x.GetChild()).Returns(childRequests.AsQueryable().BuildMock().Object);
        }
    }
}
