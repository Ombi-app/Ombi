using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api.External.NotificationServices.Ntfy;
using Ombi.Helpers;
using Ombi.Notifications.Agents;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Notifications.Tests
{
    [TestFixture]
    public class NtfyNotificationTests
    {
        private AutoMocker _mocker;
        private NtfyNotification _subject;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<NtfyNotification>();
        }

        [Test]
        public void NotificationName_ReturnsCorrectName()
        {
            // Act
            var name = _subject.NotificationName;

            // Assert
            Assert.That(name, Is.EqualTo("NtfyNotification"));
        }

        [Test]
        public async Task NotifyAsync_WithValidSettings_CallsApiCorrectly()
        {
            // Arrange
            var settings = new NtfySettings
            {
                Enabled = true,
                BaseUrl = "https://ntfy.sh",
                Topic = "ombi-test",
                AuthorizationHeader = "Bearer test-token",
                Priority = 4
            };

            var options = new NotificationOptions
            {
                NotificationType = NotificationType.Test,
                RequestId = -1
            };

            var apiMock = _mocker.GetMock<INtfyApi>();
            apiMock
                .Setup(x => x.PushAsync(
                    It.Is<string>(s => s == "https://ntfy.sh"),
                    It.Is<string>(s => s == "ombi-test"),
                    It.Is<string>(s => s == "Bearer test-token"),
                    It.Is<string>(s => s == "Test Notification"),
                    It.IsAny<string>(),
                    It.Is<int>(i => i == 4)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _subject.NotifyAsync(options, settings);

            // Assert
            apiMock.Verify();
        }

        [Test]
        public async Task NotifyAsync_WithoutAuthHeader_CallsApiWithNullAuth()
        {
            // Arrange
            var settings = new NtfySettings
            {
                Enabled = true,
                BaseUrl = "https://ntfy.sh",
                Topic = "ombi-test",
                Priority = 3
            };

            var options = new NotificationOptions
            {
                NotificationType = NotificationType.Test,
                RequestId = -1
            };

            var apiMock = _mocker.GetMock<INtfyApi>();
            apiMock
                .Setup(x => x.PushAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<string>(s => s == null || string.IsNullOrEmpty(s)),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _subject.NotifyAsync(options, settings);

            // Assert
            apiMock.Verify();
        }

        [Test]
        public async Task NotifyAsync_WhenApiFails_LogsError()
        {
            // Arrange
            var settings = new NtfySettings
            {
                Enabled = true,
                BaseUrl = "https://ntfy.sh",
                Topic = "ombi-test",
                Priority = 3
            };

            var options = new NotificationOptions
            {
                NotificationType = NotificationType.Test,
                RequestId = -1
            };

            var apiMock = _mocker.GetMock<INtfyApi>();
            apiMock
                .Setup(x => x.PushAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(Task.FromException(new Exception("API Error")));

            var loggerMock = _mocker.GetMock<ILogger<NtfyNotification>>();

            // Act
            await _subject.NotifyAsync(options, settings);

            // Assert - Verify that logging occurred
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    LoggingEvents.NtfyNotification,
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task NotifyAsync_WithDisabledSettings_DoesNotCallApi()
        {
            // Arrange
            var settings = new NtfySettings
            {
                Enabled = false,
                BaseUrl = "https://ntfy.sh",
                Topic = "ombi-test"
            };

            var options = new NotificationOptions
            {
                NotificationType = NotificationType.Test,
                RequestId = -1
            };

            var apiMock = _mocker.GetMock<INtfyApi>();

            // Act
            await _subject.NotifyAsync(options, settings);

            // Assert
            apiMock.Verify(
                x => x.PushAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task NotifyAsync_WithEmptyBaseUrl_DoesNotCallApi()
        {
            // Arrange
            var settings = new NtfySettings
            {
                Enabled = true,
                BaseUrl = "",
                Topic = "ombi-test"
            };

            var options = new NotificationOptions
            {
                NotificationType = NotificationType.Test,
                RequestId = -1
            };

            var apiMock = _mocker.GetMock<INtfyApi>();

            // Act
            await _subject.NotifyAsync(options, settings);

            // Assert
            apiMock.Verify(
                x => x.PushAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task NotifyAsync_WithEmptyTopic_DoesNotCallApi()
        {
            // Arrange
            var settings = new NtfySettings
            {
                Enabled = true,
                BaseUrl = "https://ntfy.sh",
                Topic = ""
            };

            var options = new NotificationOptions
            {
                NotificationType = NotificationType.Test,
                RequestId = -1
            };

            var apiMock = _mocker.GetMock<INtfyApi>();

            // Act
            await _subject.NotifyAsync(options, settings);

            // Assert
            apiMock.Verify(
                x => x.PushAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task NotifyAsync_WithCustomPriority_UsesCorrectPriority()
        {
            // Arrange
            var settings = new NtfySettings
            {
                Enabled = true,
                BaseUrl = "https://ntfy.sh",
                Topic = "ombi-test",
                Priority = 5
            };

            var options = new NotificationOptions
            {
                NotificationType = NotificationType.Test,
                RequestId = -1
            };

            var apiMock = _mocker.GetMock<INtfyApi>();
            apiMock
                .Setup(x => x.PushAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<int>(i => i == 5)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _subject.NotifyAsync(options, settings);

            // Assert
            apiMock.Verify();
        }
    }
}
