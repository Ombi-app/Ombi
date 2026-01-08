using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using Ombi.Helpers;
using System;
using System.Threading.Tasks;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class CacheServiceTests
    {
        private MemoryCache _memoryCache;
        private CacheService _subject;

        [SetUp]
        public void Setup()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _subject = new CacheService(_memoryCache);
        }

        [TearDown]
        public void TearDown()
        {
            _memoryCache?.Dispose();
        }

        [Test]
        public async Task GetOrAddAsync_WithFactory_ReturnsCachedValue()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var factoryCalled = false;

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () =>
            {
                factoryCalled = true;
                return Task.FromResult(expectedValue);
            });

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            Assert.That(factoryCalled, Is.True); // Factory should be called for first call

            // Second call should return cached value without calling factory
            factoryCalled = false;
            var cachedResult = await _subject.GetOrAddAsync(cacheKey, () =>
            {
                factoryCalled = true;
                return Task.FromResult("different-value");
            });

            Assert.That(cachedResult, Is.EqualTo(expectedValue)); // Should return cached value
            Assert.That(factoryCalled, Is.False); // Factory should not be called
        }

        [Test]
        public async Task GetOrAddAsync_WithCustomExpiration_UsesProvidedExpiration()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var customExpiration = DateTimeOffset.Now.AddHours(2);

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue), customExpiration);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            
            // Verify the value is cached with the custom expiration
            var cachedValue = _memoryCache.Get(cacheKey);
            Assert.That(cachedValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public async Task GetOrAddAsync_WithDefaultExpiration_UsesOneHourExpiration()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            
            // Verify the value is cached
            var cachedValue = _memoryCache.Get(cacheKey);
            Assert.That(cachedValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public async Task GetOrAddAsync_FactoryThrowsException_PropagatesException()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedException = new InvalidOperationException("Test exception");

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _subject.GetOrAddAsync<string>(cacheKey, () => throw expectedException));

            Assert.That(exception, Is.EqualTo(expectedException));
        }

        [Test]
        public async Task GetOrAddAsync_WithNullCacheKey_ThrowsArgumentNullException()
        {
            // Arrange
            string cacheKey = null;
            var expectedValue = "test-value";

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue)));

            Assert.That(exception.ParamName, Is.EqualTo("key"));
        }

        [Test]
        public async Task GetOrAddAsync_WithEmptyCacheKey_WorksCorrectly()
        {
            // Arrange
            var cacheKey = "";
            var expectedValue = "test-value";

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            
            // Verify the value is cached
            var cachedValue = _memoryCache.Get(cacheKey);
            Assert.That(cachedValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOrAdd_WithFactory_ReturnsCachedValue()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var factoryCalled = false;

            // Act
            var result = _subject.GetOrAdd(cacheKey, () =>
            {
                factoryCalled = true;
                return expectedValue;
            }, DateTimeOffset.Now.AddHours(1));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            Assert.That(factoryCalled, Is.True); // Factory should be called for first call

            // Second call should return cached value without calling factory
            factoryCalled = false;
            var cachedResult = _subject.GetOrAdd(cacheKey, () =>
            {
                factoryCalled = true;
                return "different-value";
            }, DateTimeOffset.Now.AddHours(1));

            Assert.That(cachedResult, Is.EqualTo(expectedValue)); // Should return cached value
            Assert.That(factoryCalled, Is.False); // Factory should not be called
        }

        [Test]
        public void GetOrAdd_WithCustomExpiration_UsesProvidedExpiration()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var customExpiration = DateTimeOffset.Now.AddHours(3);

            // Act
            var result = _subject.GetOrAdd(cacheKey, () => expectedValue, customExpiration);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            
            // Verify the value is cached
            var cachedValue = _memoryCache.Get(cacheKey);
            Assert.That(cachedValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetOrAdd_FactoryThrowsException_PropagatesException()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedException = new InvalidOperationException("Test exception");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                _subject.GetOrAdd<string>(cacheKey, () => throw expectedException, DateTimeOffset.Now.AddHours(1)));

            Assert.That(exception, Is.EqualTo(expectedException));
        }

        [Test]
        public void Remove_WithValidKey_RemovesFromCache()
        {
            // Arrange
            var cacheKey = "test-key";
            var value = "test-value";
            _memoryCache.Set(cacheKey, value);

            // Verify value is in cache
            Assert.That(_memoryCache.Get(cacheKey), Is.EqualTo(value));

            // Act
            _subject.Remove(cacheKey);

            // Assert
            Assert.That(_memoryCache.Get(cacheKey), Is.Null);
        }

        [Test]
        public void Remove_WithNullKey_ThrowsArgumentNullException()
        {
            // Arrange
            string cacheKey = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _subject.Remove(cacheKey));
            Assert.That(exception.ParamName, Is.EqualTo("key"));
        }

        [Test]
        public void Remove_WithEmptyKey_WorksCorrectly()
        {
            // Arrange
            var cacheKey = "";
            var value = "test-value";
            _memoryCache.Set(cacheKey, value);

            // Verify value is in cache
            Assert.That(_memoryCache.Get(cacheKey), Is.EqualTo(value));

            // Act
            _subject.Remove(cacheKey);

            // Assert
            Assert.That(_memoryCache.Get(cacheKey), Is.Null);
        }

        [Test]
        public async Task GetOrAddAsync_WithComplexObject_HandlesCorrectly()
        {
            // Arrange
            var cacheKey = "complex-object-key";
            var expectedValue = new TestObject { Id = 1, Name = "Test" };

            // Act
            var result = await _subject.GetOrAddAsync(cacheKey, () => Task.FromResult(expectedValue));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Test"));
            
            // Verify the value is cached
            var cachedValue = _memoryCache.Get(cacheKey) as TestObject;
            Assert.That(cachedValue, Is.Not.Null);
            Assert.That(cachedValue.Id, Is.EqualTo(1));
            Assert.That(cachedValue.Name, Is.EqualTo("Test"));
        }

        [Test]
        public void GetOrAdd_WithComplexObject_HandlesCorrectly()
        {
            // Arrange
            var cacheKey = "complex-object-key";
            var expectedValue = new TestObject { Id = 2, Name = "Another Test" };

            // Act
            var result = _subject.GetOrAdd(cacheKey, () => expectedValue, DateTimeOffset.Now.AddHours(1));

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
            Assert.That(result.Id, Is.EqualTo(2));
            Assert.That(result.Name, Is.EqualTo("Another Test"));
            
            // Verify the value is cached
            var cachedValue = _memoryCache.Get(cacheKey) as TestObject;
            Assert.That(cachedValue, Is.Not.Null);
            Assert.That(cachedValue.Id, Is.EqualTo(2));
            Assert.That(cachedValue.Name, Is.EqualTo("Another Test"));
        }

        [Test]
        public async Task GetOrAddAsync_WithZeroExpiration_HandlesCorrectly()
        {
            // Arrange
            var cacheKey = "test-key";
            var expectedValue = "test-value";
            var zeroExpiration = DateTimeOffset.MinValue;

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
