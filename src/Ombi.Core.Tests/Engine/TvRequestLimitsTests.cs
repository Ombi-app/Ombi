using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Models;
using Ombi.Core.Services;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class TvRequestLimitsTests
    {

        private AutoMocker _mocker;
        private RequestLimitService _subject;

        [SetUp]
        public void SetUp()
        {
            _mocker = new AutoMocker();
            var principleMock = new Mock<IPrincipal>();
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(x => x.Name).Returns("Test");
            principleMock.SetupGet(x => x.Identity).Returns(identityMock.Object);
            _mocker.Use(principleMock.Object);

            _subject = _mocker.CreateInstance<RequestLimitService>();
        }

        [Test]
        public async Task User_No_TvLimit_Set()
        {
            var user = new OmbiUser();
            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result.HasLimit, Is.False);
        }

        [Test]
        public async Task No_UserPassedIn_UsernotExist_No_TvLimit_Set()
        {
            var user = new OmbiUser();

            var um = _mocker.GetMock<OmbiUserManager>();
            um.SetupGet(x => x.Users).Returns(new List<OmbiUser> { user }.AsQueryable().BuildMock().Object);



            var result = await _subject.GetRemainingTvRequests(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task No_UserPassedIn_No_TvLimit_Set()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST"
            };

            var um = _mocker.GetMock<OmbiUserManager>();
            um.SetupGet(x => x.Users).Returns(new List<OmbiUser> { user }.AsQueryable().BuildMock().Object);



            var result = await _subject.GetRemainingTvRequests(null);

            Assert.That(result.HasLimit, Is.False);
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_No_Requests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 1
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(new List<RequestLog>().AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                Id = "id1"
            };
            var yesterday = DateTime.Now.AddDays(-1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    RequestDate = yesterday, // Yesterday
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(yesterday.AddDays(7))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_MultipleRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                Id = "id1"
            };
            var yesterday = DateTime.Now.AddDays(-1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate = yesterday,
                },
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate = yesterday.AddDays(-2),
                },
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate =yesterday.AddDays(-3), // Yesterday
                },
                new RequestLog
                {
                    EpisodeCount = 1,
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    RequestDate =yesterday.AddDays(-4), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate =yesterday.AddDays(-5), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate =yesterday.AddDays(-6), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate =yesterday.AddDays(-7), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate = yesterday.AddDays(-8), // Yesterday
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(0)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(yesterday.AddDays(1))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Daily_NoRequestsToday()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Day,
                Id = "id1"
            };
            var yesterday = DateTime.Now.AddDays(-1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 1,
                    RequestDate = yesterday,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(2)
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Daily_OneRequestsToday()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Day,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate = today.AddHours(-1),
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddDays(1).AddHours(-1))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Daily_AllRequestsToday()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Day,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    RequestDate = today.AddHours(-1),
                    EpisodeCount = 1,
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 1,
                    RequestDate = today.AddHours(-2),
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(0)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddDays(1).AddHours(-2))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Daily_MultipleEpisodeRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 10,
                EpisodeRequestLimitType = RequestLimitType.Day,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    RequestDate = today.AddHours(-1),
                    EpisodeCount = 5,
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 4,
                    RequestDate = today.AddHours(-2),
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(10)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddDays(1).AddHours(-2))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Weekly_NoRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Week,
                Id = "id1"
            };
            var lastWeek = DateTime.Now.AddDays(-8);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 1,
                    RequestDate = lastWeek,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(2)
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Weekly_OneRequestsWeek()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Week,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate = today,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddDays(7))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Weekly_AllRequestsWeek()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Week,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 1,
                    RequestDate = today.AddDays(-1),
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    RequestDate = today,
                    EpisodeCount = 1,
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(0)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddDays(6))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Weekly_MultipleEpisodeRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 10,
                EpisodeRequestLimitType = RequestLimitType.Week,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 5,
                    RequestDate = today.AddDays(-1),
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    RequestDate = today,
                    EpisodeCount = 4,
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(10)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddDays(6))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Monthly_NoRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Month,
                Id = "id1"
            };
            var lastWeek = DateTime.Now.AddMonths(-1).AddDays(-1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 1,
                    RequestDate = lastWeek,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(2)
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Monthly_OneRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Month,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    EpisodeCount = 1,
                    RequestType = RequestType.TvShow,
                    RequestDate = today,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddMonths(1))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Monthly_AllRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 2,
                EpisodeRequestLimitType = RequestLimitType.Month,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 1,
                    RequestDate = today.AddDays(-1),
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 1,
                    RequestDate = today,
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(0)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddMonths(1).AddDays(-1))
                );
        }

        [Test]
        public async Task UserPassedIn_TvLimit_Set_Limit_Monthly_MultipleEpisodeREeuests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                EpisodeRequestLimit = 10,
                EpisodeRequestLimitType = RequestLimitType.Month,
                Id = "id1"
            };
            var today = DateTime.Now;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount =5,
                    RequestDate = today.AddDays(-1),
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.TvShow,
                    EpisodeCount = 4,
                    RequestDate = today,
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingTvRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(10)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddMonths(1).AddDays(-1))
                );

        }
    }
}
