using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Api;
using Ombi.Api.External.NotificationServices.Ntfy;

namespace Ombi.Notifications.Tests
{
    [TestFixture]
    public class NtfyApiTests
    {
        private AutoMocker _mocker;
        private NtfyApi _subject;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _subject = _mocker.CreateInstance<NtfyApi>();
        }

        [Test]
        public async Task PushAsync_WithAuthHeader_AddsAuthorizationHeader()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var authHeader = "Bearer tk_test123";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, authHeader, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            var hasAuthHeader = capturedRequest.Headers.Any(h => h.Key == "Authorization");
            Assert.That(hasAuthHeader, Is.True);
            var authHeaderValue = capturedRequest.Headers.First(h => h.Key == "Authorization").Value;
            Assert.That(authHeaderValue, Is.EqualTo(authHeader));
        }

        [Test]
        public async Task PushAsync_WithoutAuthHeader_DoesNotAddAuthorizationHeader()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            string authHeader = null;
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, authHeader, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            var hasAuthHeader = capturedRequest.Headers.Any(h => h.Key == "Authorization");
            Assert.That(hasAuthHeader, Is.False);
        }

        [Test]
        public async Task PushAsync_WithEmptyAuthHeader_DoesNotAddAuthorizationHeader()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            string authHeader = "";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, authHeader, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            var hasAuthHeader = capturedRequest.Headers.Any(h => h.Key == "Authorization");
            Assert.That(hasAuthHeader, Is.False);
        }

        [Test]
        public async Task PushAsync_UsesCorrectHttpMethod()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, null, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.HttpMethod, Is.EqualTo(HttpMethod.Post));
        }

        [Test]
        public async Task PushAsync_UsesCorrectBaseUrl()
        {
            // Arrange
            var baseUrl = "https://custom.ntfy.server";
            var topic = "test-topic";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, null, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.BaseUrl, Is.EqualTo(baseUrl));
        }

        [Test]
        public async Task PushAsync_SetsJsonContentType()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, null, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            var hasContentType = capturedRequest.ContentHeaders.Any(h => h.Key == "Content-Type");
            Assert.That(hasContentType, Is.True);
            var contentType = capturedRequest.ContentHeaders.First(h => h.Key == "Content-Type").Value;
            Assert.That(contentType, Is.EqualTo("application/json"));
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public async Task PushAsync_AcceptsPriorityValues(int priority)
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var subject = "Test Subject";
            var message = "Test Message";

            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act & Assert - Should not throw
            await _subject.PushAsync(baseUrl, topic, null, subject, message, priority);
            apiMock.Verify(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PushAsync_WithBearerToken_FormatsAuthHeaderCorrectly()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var authHeader = "Bearer tk_AgQdq7mVBoFD37zQVN29RhuMzNIz2";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, authHeader, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            var authHeaderValue = capturedRequest.Headers.First(h => h.Key == "Authorization").Value;
            Assert.That(authHeaderValue, Does.StartWith("Bearer "));
        }

        [Test]
        public async Task PushAsync_WithBasicAuth_FormatsAuthHeaderCorrectly()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var authHeader = "Basic dXNlcjpwYXNzd29yZA==";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, authHeader, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            var authHeaderValue = capturedRequest.Headers.First(h => h.Key == "Authorization").Value;
            Assert.That(authHeaderValue, Does.StartWith("Basic "));
        }

        [Test]
        public async Task PushAsync_WithSpecialCharactersInMessage_Succeeds()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var subject = "Test: Special <Characters>";
            var message = "Message with \"quotes\" and 'apostrophes' & ampersands";
            var priority = 3;

            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            var exception = default(Exception);
            try
            {
                await _subject.PushAsync(baseUrl, topic, null, subject, message, priority);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.That(exception, Is.Null, "Should handle special characters without throwing");
        }

        [Test]
        public async Task PushAsync_WithEmptyEndpoint_UsesBaseUrlDirectly()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, null, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.Endpoint, Is.EqualTo(string.Empty),
                "Ntfy API should POST to base URL directly, not to a specific endpoint");
        }

        [Test]
        public async Task PushAsync_WhenApiThrowsException_PropagatesException()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromException<HttpResponseMessage>(new HttpRequestException("Network error")));

            // Act
            var exception = default(HttpRequestException);
            try
            {
                await _subject.PushAsync(baseUrl, topic, null, subject, message, priority);
            }
            catch (HttpRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("Network error"));
        }

        [Test]
        public async Task PushAsync_IncludesJsonBody()
        {
            // Arrange
            var baseUrl = "https://ntfy.sh";
            var topic = "test-topic";
            var subject = "Test Subject";
            var message = "Test Message";
            var priority = 3;

            Request capturedRequest = null;
            var apiMock = _mocker.GetMock<IApi>();
            apiMock
                .Setup(x => x.Request(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                .Callback<Request, CancellationToken>((r, ct) => capturedRequest = r)
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            // Act
            await _subject.PushAsync(baseUrl, topic, null, subject, message, priority);

            // Assert
            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest.JsonBody, Is.Not.Null);
        }
    }
}
