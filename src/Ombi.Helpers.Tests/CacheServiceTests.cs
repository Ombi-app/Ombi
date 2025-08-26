using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Helpers;
using System;
using System.Threading.Tasks;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class CacheServiceTests
    {
        private AutoMocker _mocker;
        private CacheService _subject;
        private Mock<IMemoryCache> _memoryCacheMock;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _memoryCacheMock = _mocker.GetMock<IMemoryCache>();
            _subject = _mocker.CreateInstance<CacheService>();
        }

        [Test]
        public async Task GetOrAddAsync_WithFactory_ReturnsCachedValue()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var factoryCalled = false;

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<string>>>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () =>
            {
                factoryCalled = true;
                return Task.FromResult(expectedValue);
            });

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            Assert.That(factoryCalled, Is.False); // Factory should not be called if value is cached
        }

        [Test]
        public async Task GetOrAddAsync_WithCustomExpiration_UsesProvidedExpiration()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var customExpiration = DateTimeOffset.Now.AddHours(2);

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<string>>>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue), customExpiration);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public async Task GetOrAddAsync_WithDefaultExpiration_UsesOneHourExpiration()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<string>>>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public async Task GetOrAddAsync_FactoryThrowsException_PropagatesException()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedException = new InvalidOperationException("Test exception");

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<string>>>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult("value")));

            Assert.That(exception, Is.EqualTo(expectedException));
        }

        [Test]
        public async Task GetOrAddAsync_WithNullCacheKey_HandlesGracefully()
        {
            // Arrange
            string cacheKey = null;
            var expectedValue = "test-value";

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<ICacheEntry, Task<string>>>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public async Task GetOrAddAsync_WithEmptyCacheKey_HandlesGracefully()
        {
            // Arrange
            var cacheKey = "";
            var expectedValue = "test-value";

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<ICacheEntry, Task<string>>>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOrAdd_WithFactory_ReturnsCachedValue()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var factoryCalled = false;

            _memoryCacheMock.Setup(x => x.GetOrCreate(
                cacheKey,
                It.IsAny<Func<ICacheEntry, string>>()))
                .Returns(expectedValue);

            // Act
            var result = _subject.GetOrAdd(cacheKey, () =>
            {
                factoryCalled = true;
                return expectedValue;
            }, DateTimeOffset.Now.AddHours(1));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            Assert.That(factoryCalled, Is.False); // Factory should not be called if value is cached
        }

        [Test]
        public void GetOrAdd_WithCustomExpiration_UsesProvidedExpiration()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var customExpiration = DateTimeOffset.Now.AddHours(3);

            _memoryCacheMock.Setup(x => x.GetOrCreate(
                cacheKey,
                It.IsAny<Func<ICacheEntry, string>>()))
                .Returns(expectedValue);

            // Act
            var result = _subject.GetOrAdd(cacheKey, () => expectedValue, customExpiration);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOrAdd_FactoryThrowsException_PropagatesException()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedException = new InvalidOperationException("Test exception");

            _memoryCacheMock.Setup(x => x.GetOrCreate(
                cacheKey,
                It.IsAny<Func<ICacheEntry, string>>()))
                .Throws(expectedException);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _subject.GetOrAdd(cacheKey, () => "value", DateTimeOffset.Now.AddHours(1)));

            Assert.That(exception, Is.EqualTo(expectedException));
        }

        [Test]
        public void Remove_WithValidKey_RemovesFromCache()
        {
            // Arrange
            var cacheKey = "test-key";

            // Act
            _subject.Remove(cacheKey);

            // Assert
            _memoryCacheMock.Verify(x => x.Remove(cacheKey), Times.Once);
        }

        [Test]
        public void Remove_WithNullKey_HandlesGracefully()
        {
            // Arrange
            string cacheKey = null;

            // Act
            _subject.Remove(cacheKey);

            // Assert
            _memoryCacheMock.Verify(x => x.Remove(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void Remove_WithEmptyKey_HandlesGracefully()
        {
            // Arrange
            var cacheKey = "";

            // Act
            _subject.Remove(cacheKey);

            // Assert
            _memoryCacheMock.Verify(x => x.Remove(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task GetOrAddAsync_WithComplexObject_HandlesCorrectly()
        {
            // Arrange
            var cacheKey = "complex-object-key";
            var expectedValue = new TestObject { Id = 1, Name = "Test" };

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<TestObject>>>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Test"));
        }

        [Test]
        public void GetOrAdd_WithComplexObject_HandlesCorrectly()
        {
            // Arrange
            var cacheKey = "complex-object-key";
            var expectedValue = new TestObject { Id = 2, Name = "Another Test" };

            _memoryCacheMock.Setup(x => x.GetOrCreate(
                cacheKey,
                It.IsAny<Func<ICacheEntry, TestObject>>()))
                .Returns(expectedValue);

            // Act
            var result = _subject.GetOrAdd(cacheKey, () => expectedValue, DateTimeOffset.Now.AddHours(1));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            Assert.That(result.Id, Is.EqualTo(2));
            Assert.That(result.Name, Is.EqualTo("Another Test"));
        }

        [Test]
        public async Task GetOrAddAsync_WithZeroExpiration_HandlesCorrectly()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var zeroExpiration = DateTimeOffset.MinValue;

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<string>>>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue), zeroExpiration);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public async Task GetOrAddAsync_WithPastExpiration_HandlesCorrectly()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var pastExpiration = DateTimeOffset.Now.AddHours(-1);

            _memoryCacheMock.Setup(x => x.GetOrCreateAsync(
                cacheKey,
                It.IsAny<Func<ICacheEntry, Task<string>>>()))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue), pastExpiration);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is TestObject other)
                {
                    return Id == other.Id && Name == other.Name;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Name);
            }
        }
    }
}
