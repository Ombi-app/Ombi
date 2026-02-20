using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ombi.Core.Settings;
using Ombi.Core.Settings.Models.External;
using Ombi.Helpers;
using Ombi.Settings.Settings;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Settings.Tests
{
    [TestFixture]
    public class SettingsServiceTests
    {
        private Mock<ISettingsRepository> _mockRepo;
        private Mock<ICacheService> _mockCache;
        private SettingsService<TheMovieDbSettings> _service;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<ISettingsRepository>();
            _mockCache = new Mock<ICacheService>();
            _service = new SettingsService<TheMovieDbSettings>(_mockRepo.Object, _mockCache.Object);
        }

        [Test]
        public async Task GetSettingsAsync_WhenDatabaseRecordExistsWithEmptyContent_ReturnsDefaultInstance()
        {
            // Arrange - Database record exists but Content is empty
            var emptyRecord = new GlobalSettings
            {
                Id = 1,
                SettingsName = "TheMovieDbSettings",
                Content = "" // Empty content
            };

            _mockCache.Setup(x => x.GetOrAddAsync(
                It.IsAny<string>(),
                It.IsAny<System.Func<Task<TheMovieDbSettings>>>(),
                It.IsAny<System.DateTimeOffset>()))
                .Returns<string, System.Func<Task<TheMovieDbSettings>>, System.DateTimeOffset>(
                    async (key, factory, expiration) => await factory());

            _mockRepo.Setup(x => x.GetAsync("TheMovieDbSettings"))
                .ReturnsAsync(emptyRecord);

            // Act
            var result = await _service.GetSettingsAsync();

            // Assert
            Assert.That(result, Is.Not.Null, "Should return default instance, not null");
            Assert.That(result, Is.InstanceOf<TheMovieDbSettings>());
        }

        [Test]
        public async Task GetSettingsAsync_WhenDatabaseRecordExistsWithNullContent_ReturnsDefaultInstance()
        {
            // Arrange - Database record exists but Content is null
            var nullContentRecord = new GlobalSettings
            {
                Id = 1,
                SettingsName = "TheMovieDbSettings",
                Content = null // Null content
            };

            _mockCache.Setup(x => x.GetOrAddAsync(
                It.IsAny<string>(),
                It.IsAny<System.Func<Task<TheMovieDbSettings>>>(),
                It.IsAny<System.DateTimeOffset>()))
                .Returns<string, System.Func<Task<TheMovieDbSettings>>, System.DateTimeOffset>(
                    async (key, factory, expiration) => await factory());

            _mockRepo.Setup(x => x.GetAsync("TheMovieDbSettings"))
                .ReturnsAsync(nullContentRecord);

            // Act
            var result = await _service.GetSettingsAsync();

            // Assert
            Assert.That(result, Is.Not.Null, "Should return default instance, not null");
            Assert.That(result, Is.InstanceOf<TheMovieDbSettings>());
        }

        [Test]
        public async Task GetSettingsAsync_WhenNoDatabaseRecordExists_ReturnsDefaultInstance()
        {
            // Arrange - No database record exists
            _mockCache.Setup(x => x.GetOrAddAsync(
                It.IsAny<string>(),
                It.IsAny<System.Func<Task<TheMovieDbSettings>>>(),
                It.IsAny<System.DateTimeOffset>()))
                .Returns<string, System.Func<Task<TheMovieDbSettings>>, System.DateTimeOffset>(
                    async (key, factory, expiration) => await factory());

            _mockRepo.Setup(x => x.GetAsync("TheMovieDbSettings"))
                .ReturnsAsync((GlobalSettings)null);

            // Act
            var result = await _service.GetSettingsAsync();

            // Assert
            Assert.That(result, Is.Not.Null, "Should return default instance when no record exists");
            Assert.That(result, Is.InstanceOf<TheMovieDbSettings>());
        }
    }
}
