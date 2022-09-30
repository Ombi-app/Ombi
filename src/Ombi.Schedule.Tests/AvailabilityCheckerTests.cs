using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core;
using Ombi.Hubs;
using Ombi.Notifications.Models;
using Ombi.Schedule.Jobs;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class AvailabilityCheckerTests
    {
        private AutoMocker _mocker;
        private TestAvailabilityChecker _subject;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMocker();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.Clients(It.IsAny<IReadOnlyList<string>>())).Returns(mockClientProxy.Object);

            var hubContext = new Mock<IHubContext<NotificationHub>>();
            hubContext.Setup(x => x.Clients).Returns(() => mockClients.Object);
            _mocker.Use(hubContext);


            _subject = _mocker.CreateInstance<TestAvailabilityChecker>();
        }

        [Test]
        public async Task All_Episodes_Are_Available_In_Request()
        {
            var request = new ChildRequests
            {
                Title = "Test",
                Id = 1,
                RequestedUser = new OmbiUser { Email = "" },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                Available = false,
                                EpisodeNumber = 1,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            },
                            new EpisodeRequests
                            {
                                Available = false,
                                EpisodeNumber = 2,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            }
                        }
                    }
                }
            };

            var databaseEpisodes = new List<IBaseMediaServerEpisode>
            {
                new PlexEpisode
                {
                    EpisodeNumber = 1,
                    SeasonNumber = 1,
                },
                new PlexEpisode
                {
                    EpisodeNumber = 2,
                    SeasonNumber = 1,
                },
            }.AsQueryable().BuildMock().Object;

            await _subject.ProcessTvShow(databaseEpisodes, request);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available, Is.True);
                Assert.That(request.MarkedAsAvailable, Is.Not.Null);
                Assert.That(request.SeasonRequests[0].Episodes[0].Available, Is.True);
                Assert.That(request.SeasonRequests[0].Episodes[1].Available, Is.True);
            });

            Assert.Multiple(() =>
            {
                _mocker.Verify<ITvRequestRepository>(x => x.Save(), Times.Exactly(2));
                _mocker.Verify<INotificationHelper>(x => x.Notify(It.Is<NotificationOptions>(x => x.NotificationType == Helpers.NotificationType.RequestAvailable && x.RequestId == 1)), Times.Once);
            });
        }

        [Test]
        public async Task All_One_Episode_Is_Available_In_Request()
        {
            var request = new ChildRequests
            {
                Title = "Test",
                Id = 1,
                RequestedUser = new OmbiUser { Email = "" },
                SeasonRequests = new List<SeasonRequests>
                {
                    new SeasonRequests
                    {
                        Episodes = new List<EpisodeRequests>
                        {
                            new EpisodeRequests
                            {
                                Available = false,
                                EpisodeNumber = 1,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            },
                            new EpisodeRequests
                            {
                                Available = false,
                                EpisodeNumber = 2,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            },
                            new EpisodeRequests
                            {
                                Available = true,
                                EpisodeNumber = 3,
                                Season = new SeasonRequests
                                {
                                    SeasonNumber = 1
                                }
                            }
                        }
                    }
                }
            };

            var databaseEpisodes = new List<IBaseMediaServerEpisode>
            {
                new PlexEpisode
                {
                    EpisodeNumber = 1,
                    SeasonNumber = 1,
                },
                new PlexEpisode
                {
                    EpisodeNumber = 3,
                    SeasonNumber = 1,
                },
            }.AsQueryable().BuildMock().Object;

            await _subject.ProcessTvShow(databaseEpisodes, request);

            Assert.Multiple(() =>
            {
                Assert.That(request.Available, Is.False);
                Assert.That(request.MarkedAsAvailable, Is.Null);
                Assert.That(request.SeasonRequests[0].Episodes[0].Available, Is.True);
                Assert.That(request.SeasonRequests[0].Episodes[1].Available, Is.False);
            });

            Assert.Multiple(() =>
            {
                _mocker.Verify<ITvRequestRepository>(x => x.Save(), Times.Once);
                _mocker.Verify<INotificationHelper>(x => x.Notify(It.Is<NotificationOptions>(x => x.NotificationType == Helpers.NotificationType.PartiallyAvailable && x.RequestId == 1)), Times.Once);
            });
        }
    }


    public class TestAvailabilityChecker : AvailabilityChecker
    {
        public TestAvailabilityChecker(ITvRequestRepository tvRequest, INotificationHelper notification, ILogger log, IHubContext<NotificationHub> hub) : base(tvRequest, notification, log, hub)
        {
        }

        public new Task ProcessTvShow(IQueryable<IBaseMediaServerEpisode> seriesEpisodes, ChildRequests child) => base.ProcessTvShow(seriesEpisodes, child);
    }
}
