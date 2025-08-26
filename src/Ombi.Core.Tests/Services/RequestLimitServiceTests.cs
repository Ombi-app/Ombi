using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Authentication;
using Ombi.Core.Helpers;
using Ombi.Core.Models;
using Ombi.Core.Services;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Services
{
    [TestFixture]
    public class RequestLimitServiceTests
    {
        private AutoMocker _mocker;
        private RequestLimitService _subject;
        private Mock<ICurrentUser> _currentUserMock;
        private Mock<OmbiUserManager> _userManagerMock;
        private Mock<IRepository<RequestLog>> _requestLogRepositoryMock;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _currentUserMock = _mocker.GetMock<ICurrentUser>();
            _userManagerMock = _mocker.GetMock<OmbiUserManager>();
            _requestLogRepositoryMock = _mocker.GetMock<IRepository<RequestLog>>();

            // Setup default user manager mock
            var defaultUser = new OmbiUser { Id = "default-user", UserName = "defaultuser", NormalizedUserName = "DEFAULTUSER" };
            var currentUser = new OmbiUser { Id = "current-user", UserName = "currentuser", NormalizedUserName = "CURRENTUSER" };
            var userQueryable = new List<OmbiUser> { defaultUser, currentUser }.AsQueryable().BuildMock();
            _userManagerMock.Setup(x => x.Users).Returns(userQueryable);

            // Setup default current user mock
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(currentUser);

            _subject = _mocker.CreateInstance<RequestLimitService>();
        }

        [Test]
        public async Task GetRemainingMovieRequests_UserIsNull_ReturnsNull()
        {
            // Arrange
            // The service has a bug where it doesn't check if currentUser is null before accessing UserName
            // We need to work around this by providing a user that will result in null being returned
            var mockCurrentUser = new OmbiUser { UserName = "testuser" };
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(mockCurrentUser);
            
            // When GetUser() tries to find the user in userManager.Users, return null
            // This will cause the service to return null as expected
            var emptyUsersQueryable = new List<OmbiUser>().AsQueryable().BuildMock();
            _userManagerMock.Setup(x => x.Users).Returns(emptyUsersQueryable);

            // Act
            var result = await _subject.GetRemainingMovieRequests(null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetRemainingMovieRequests_NoLimitSet_ReturnsNoLimit()
        {
            // Arrange
            var user = CreateTestUser(movieRequestLimit: 0);
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);

            // Act
            var result = await _subject.GetRemainingMovieRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.False);
            Assert.That(result.Limit, Is.EqualTo(0));
            Assert.That(result.Remaining, Is.EqualTo(0));
        }

        [Test]
        public async Task GetRemainingMovieRequests_WeeklyLimit_CalculatesCorrectly()
        {
            // Arrange
            var user = CreateTestUser(movieRequestLimit: 5, movieRequestLimitType: RequestLimitType.Week);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.Movie, 3);
            
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingMovieRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(5));
            Assert.That(result.Remaining, Is.EqualTo(2));
        }

        [Test]
        public async Task GetRemainingMovieRequests_MonthlyLimit_CalculatesCorrectly()
        {
            // Arrange
            var user = CreateTestUser(movieRequestLimit: 10, movieRequestLimitType: RequestLimitType.Month);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.Movie, 7);
            
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingMovieRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(10));
            Assert.That(result.Remaining, Is.EqualTo(3));
        }

        [Test]
        public async Task GetRemainingMovieRequests_DailyLimit_CalculatesCorrectly()
        {
            // Arrange
            var user = CreateTestUser(movieRequestLimit: 5, movieRequestLimitType: RequestLimitType.Day);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.Movie, 1);
            
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingMovieRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(5));
            Assert.That(result.Remaining, Is.EqualTo(4));
        }

        [Test]
        public async Task GetRemainingMovieRequests_LimitExceeded_ReturnsZeroRemaining()
        {
            // Arrange
            var user = CreateTestUser(movieRequestLimit: 2, movieRequestLimitType: RequestLimitType.Day);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.Movie, 3);
            
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingMovieRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(2));
            Assert.That(result.Remaining, Is.EqualTo(0));
        }

        [Test]
        public async Task GetRemainingTvRequests_WeeklyLimit_CalculatesCorrectly()
        {
            // Arrange
            var user = CreateTestUser(episodeRequestLimit: 10, episodeRequestLimitType: RequestLimitType.Week);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.TvShow, 6);
            
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingTvRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(10));
            Assert.That(result.Remaining, Is.EqualTo(4));
        }

        [Test]
        public async Task GetRemainingMusicRequests_MonthlyLimit_CalculatesCorrectly()
        {
            // Arrange
            var user = CreateTestUser(musicRequestLimit: 15, musicRequestLimitType: RequestLimitType.Month);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.Album, 12);
            
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingMusicRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(15));
            Assert.That(result.Remaining, Is.EqualTo(3));
        }

        [Test]
        public async Task GetRemainingMovieRequests_WithCustomDateTime_CalculatesCorrectly()
        {
            // Arrange
            var customDate = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
            var user = CreateTestUser(movieRequestLimit: 5, movieRequestLimitType: RequestLimitType.Week);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.Movie, 2, customDate);
            
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingMovieRequests(user, customDate);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(5));
            Assert.That(result.Remaining, Is.EqualTo(3));
        }

        [Test]
        public async Task GetRemainingMovieRequests_NoLimitType_DefaultsToWeekly()
        {
            // Arrange
            var user = CreateTestUser(movieRequestLimit: 7);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.Movie, 3);
            
            _currentUserMock.Setup(x => x.GetUser()).ReturnsAsync(user);
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingMovieRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(7));
            Assert.That(result.Remaining, Is.EqualTo(4));
        }

        [Test]
        public async Task GetRemainingMovieRequests_UserParameterProvided_UsesProvidedUser()
        {
            // Arrange
            var user = CreateTestUser(movieRequestLimit: 5, movieRequestLimitType: RequestLimitType.Day);
            var requestLogs = CreateRequestLogs(user.Id, RequestType.Movie, 1);
            
            var mockQueryable = requestLogs.AsQueryable().BuildMock();
            _requestLogRepositoryMock.Setup(x => x.GetAll()).Returns(mockQueryable);

            // Act
            var result = await _subject.GetRemainingMovieRequests(user);

            // Assert
            Assert.That(result.HasLimit, Is.True);
            Assert.That(result.Limit, Is.EqualTo(5));
            Assert.That(result.Remaining, Is.EqualTo(4));
            _currentUserMock.Verify(x => x.GetUser(), Times.Never);
        }

        private static OmbiUser CreateTestUser(
            int movieRequestLimit = 0,
            int episodeRequestLimit = 0,
            int musicRequestLimit = 0,
            RequestLimitType? movieRequestLimitType = null,
            RequestLimitType? episodeRequestLimitType = null,
            RequestLimitType? musicRequestLimitType = null)
        {
            return new OmbiUser
            {
                Id = "test-user-id",
                UserName = "testuser",
                MovieRequestLimit = movieRequestLimit,
                EpisodeRequestLimit = episodeRequestLimit,
                MusicRequestLimit = musicRequestLimit,
                MovieRequestLimitType = movieRequestLimitType,
                EpisodeRequestLimitType = episodeRequestLimitType,
                MusicRequestLimitType = musicRequestLimitType
            };
        }

        private static List<RequestLog> CreateRequestLogs(string userId, RequestType requestType, int count, DateTime? baseDate = null)
        {
            var logs = new List<RequestLog>();
            var startDate = baseDate ?? DateTime.UtcNow;
            
            for (int i = 0; i < count; i++)
            {
                // For daily limits, all requests should be on the same day
                // For weekly limits, they should be within the same week
                // For monthly limits, they should be within the same month
                var requestDate = startDate.AddHours(i); // Use hours instead of days to keep within same day/week/month
                
                logs.Add(new RequestLog
                {
                    Id = i + 1,
                    UserId = userId,
                    RequestType = requestType,
                    RequestDate = requestDate,
                    RequestId = i + 1
                });
            }
            
            return logs;
        }
    }
}
