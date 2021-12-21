﻿using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Engine;
using Ombi.Core.Models;
using Ombi.Core.Services;
using Ombi.Helpers;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Engine
{
    [TestFixture]
    public class MusicRequestLimitTests
    {

        private AutoMocker _mocker;
        private RequestLimitService _subject;

        [SetUp]
        public void SetUp()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            _mocker = new AutoMocker();
            var principleMock = new Mock<IPrincipal>();
            var identityMock = new Mock<IIdentity>();
            identityMock.SetupGet(x => x.Name).Returns("Test");
            principleMock.SetupGet(x => x.Identity).Returns(identityMock.Object);
            _mocker.Use(principleMock.Object);

            _subject = _mocker.CreateInstance<RequestLimitService>();
        }

        [Test]
        public async Task User_No_MusicLimit_Set()
        {
            var user = new OmbiUser();
            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result.HasLimit, Is.False);
        }

        [Test]
        public async Task No_UserPassedIn_UsernotExist_No_MusicLimit_Set()
        {
            var user = new OmbiUser();

            var um = _mocker.GetMock<OmbiUserManager>();
            um.SetupGet(x => x.Users).Returns(new List<OmbiUser> { user }.AsQueryable().BuildMock().Object);
            


            var result = await _subject.GetRemainingMusicRequests(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task No_UserPassedIn_No_MusicLimit_Set()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST"
            };

            var um = _mocker.GetMock<OmbiUserManager>();
            um.SetupGet(x => x.Users).Returns(new List<OmbiUser> { user }.AsQueryable().BuildMock().Object);



            var result = await _subject.GetRemainingMusicRequests(null);

            Assert.That(result.HasLimit, Is.False);
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_No_Requests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 1
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(new List<RequestLog>().AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                Id = "id1"
            };
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = yesterday, // Yesterday
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(yesterday.AddDays(7))
                );
        }

        [Test]
        [Ignore("Failing on CI")]
        public async Task UserPassedIn_MusicLimit_Set_Limit_MultipleRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                Id = "id1"
            };
            var yesterday = new DateTime(2020, 09, 05).AddDays(-1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = yesterday,
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = yesterday.AddDays(-2),
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate =yesterday.AddDays(-3), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate =yesterday.AddDays(-4), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate =yesterday.AddDays(-5), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate =yesterday.AddDays(-6), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate =yesterday.AddDays(-7), // Yesterday
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = yesterday.AddDays(-8), // Yesterday
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(0)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(yesterday.AddDays(1))
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Daily_NoRequestsToday()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Day,
                Id = "id1"
            };
            var yesterday = new DateTime(2020, 09, 05).AddDays(-1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = yesterday,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(2)
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Daily_OneRequestsToday()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Day,
                Id = "id1"
            };

            var today = DateTime.UtcNow;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today.AddHours(-1),
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddDays(1).Date)
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Daily_AllRequestsToday()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Day,
                Id = "id1"
            };
            var today = DateTime.UtcNow;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today.AddHours(-1),
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today.AddHours(-2),
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(0)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.AddDays(1).Date)
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Weekly_NoRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Week,
                Id = "id1"
            };
            var lastWeek = new DateTime(2020, 09, 05).FirstDateInWeek().AddDays(-1); // Day before reset
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = lastWeek,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(2)
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Weekly_OneRequestsWeek()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Week,
                Id = "id1"
            };
            var today = new DateTime(2021, 10, 05);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user, today);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.FirstDateInWeek().AddDays(7).Date)
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Weekly_AllRequestsWeek()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Week,
                Id = "id1"
            };
            var today = DateTime.UtcNow;
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today.AddMinutes(-2),
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today.AddMinutes(-1),
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(0)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(today.FirstDateInWeek().AddDays(7).Date)
                );
        }
        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Monthly_NoRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Month,
                Id = "id1"
            };
            var lastWeek = new DateTime(2020, 09, 05).AddMonths(-1).AddDays(-1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = lastWeek,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(2)
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Monthly_OneRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Month,
                Id = "id1"
            };
            var today = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today,
                }
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(1)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(firstDayOfMonth.AddMonths(1).Date)
                );
        }

        [Test]
        public async Task UserPassedIn_MusicLimit_Set_Limit_Monthly_AllRequests()
        {
            var user = new OmbiUser
            {
                NormalizedUserName = "TEST",
                MusicRequestLimit = 2,
                MusicRequestLimitType = RequestLimitType.Month,
                Id = "id1"
            };
            var today = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var log = new List<RequestLog>
            {
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today.AddDays(-1),
                },
                new RequestLog
                {
                    UserId = "id1",
                    RequestType = RequestType.Album,
                    RequestDate = today,
                },
            };
            var repoMock = _mocker.GetMock<IRepository<RequestLog>>();
            repoMock.Setup(x => x.GetAll()).Returns(log.AsQueryable().BuildMock().Object);

            var result = await _subject.GetRemainingMusicRequests(user);

            Assert.That(result, Is.InstanceOf<RequestQuotaCountModel>()
                .With.Property(nameof(RequestQuotaCountModel.HasLimit)).EqualTo(true)
                .And.Property(nameof(RequestQuotaCountModel.Limit)).EqualTo(2)
                .And.Property(nameof(RequestQuotaCountModel.Remaining)).EqualTo(0)
                .And.Property(nameof(RequestQuotaCountModel.NextRequest)).EqualTo(firstDayOfMonth.AddMonths(1).Date)
                );
        }
    }
}
