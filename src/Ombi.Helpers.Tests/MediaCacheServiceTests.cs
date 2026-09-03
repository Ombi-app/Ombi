using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class MediaCacheServiceTests
    {
        private const string TrackedKeys = "MediaCacheServiceKeys";

        private MemoryCache _memoryCache;
        private MediaCacheService _subject;

        [SetUp]
        public void Setup()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _subject = new MediaCacheService(_memoryCache);
        }

        [TearDown]
        public void TearDown()
        {
            _memoryCache?.Dispose();
        }

        [Test]
        public async Task GetOrAddAsync_OnlyCallsTheFactoryOnce()
        {
            var factoryCalls = 0;

            var first = await _subject.GetOrAddAsync("key", () =>
            {
                factoryCalls++;
                return Task.FromResult("value");
            });
            var second = await _subject.GetOrAddAsync("key", () =>
            {
                factoryCalls++;
                return Task.FromResult("other value");
            });

            Assert.That(first, Is.EqualTo("value"));
            Assert.That(second, Is.EqualTo("value"));
            Assert.That(factoryCalls, Is.EqualTo(1));
        }

        [Test]
        public async Task GetOrAddAsync_OnlyTracksTheKeyOnce()
        {
            await _subject.GetOrAddAsync("key", () => Task.FromResult("value"));
            await _subject.GetOrAddAsync("key", () => Task.FromResult("value"));
            await _subject.GetOrAddAsync("key", () => Task.FromResult("value"));

            Assert.That(_memoryCache.Get<HashSet<string>>(TrackedKeys), Has.Count.EqualTo(1));
        }

        [Test]
        public async Task Purge_RemovesEverythingWeHaveCached()
        {
            await _subject.GetOrAddAsync("first", () => Task.FromResult("1"));
            await _subject.GetOrAddAsync("second", () => Task.FromResult("2"));

            await _subject.Purge();

            Assert.That(_memoryCache.Get<string>("first"), Is.Null);
            Assert.That(_memoryCache.Get<string>("second"), Is.Null);
        }

        [Test]
        public async Task Purge_ForgetsTheKeysItHasAlreadyPurged()
        {
            await _subject.GetOrAddAsync("first", () => Task.FromResult("1"));
            await _subject.Purge();

            Assert.That(_memoryCache.Get<HashSet<string>>(TrackedKeys), Is.Null);
        }
    }
}
