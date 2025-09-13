using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Services;
using Ombi.Core.Settings;
using Ombi.Settings.Settings.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ombi.Core.Tests.Services
{
    [TestFixture]
    public class FeatureServiceTests
    {
        private AutoMocker _mocker;
        private FeatureService _subject;
        private Mock<ISettingsService<FeatureSettings>> _featureSettingsMock;

        [SetUp]
        public void Setup()
        {
            _mocker = new AutoMocker();
            _featureSettingsMock = _mocker.GetMock<ISettingsService<FeatureSettings>>();
            _subject = _mocker.CreateInstance<FeatureService>();
        }

        [Test]
        public async Task FeatureEnabled_FeatureExistsAndEnabled_ReturnsTrue()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "TestFeature", Enabled = true },
                new FeatureEnablement { Name = "AnotherFeature", Enabled = false }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("TestFeature");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task FeatureEnabled_FeatureExistsAndDisabled_ReturnsFalse()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "TestFeature", Enabled = false },
                new FeatureEnablement { Name = "AnotherFeature", Enabled = true }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("TestFeature");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task FeatureEnabled_FeatureDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "ExistingFeature", Enabled = true }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("NonExistentFeature");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task FeatureEnabled_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "TestFeature", Enabled = true }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("testfeature");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task FeatureEnabled_CaseInsensitive_ReturnsFalse()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "TestFeature", Enabled = false }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("TESTFEATURE");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task FeatureEnabled_NoFeatures_ReturnsFalse()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new FeatureEnablement[0]);
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("AnyFeature");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task FeatureEnabled_NullFeatures_ReturnsFalse()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(null);
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("AnyFeature");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task FeatureEnabled_MultipleFeatures_ReturnsCorrectResults()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "Feature1", Enabled = true },
                new FeatureEnablement { Name = "Feature2", Enabled = false },
                new FeatureEnablement { Name = "Feature3", Enabled = true }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act & Assert
            Assert.That(await _subject.FeatureEnabled("Feature1"), Is.True);
            Assert.That(await _subject.FeatureEnabled("Feature2"), Is.False);
            Assert.That(await _subject.FeatureEnabled("Feature3"), Is.True);
            Assert.That(await _subject.FeatureEnabled("Feature4"), Is.False);
        }

        [Test]
        public async Task FeatureEnabled_EmptyFeatureName_ReturnsFalse()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "TestFeature", Enabled = true }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task FeatureEnabled_WhitespaceFeatureName_ReturnsFalse()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "TestFeature", Enabled = true }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled("   ");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task FeatureEnabled_NullFeatureName_ReturnsFalse()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "TestFeature", Enabled = true }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act
            var result = await _subject.FeatureEnabled(null);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task FeatureEnabled_FeatureWithSpecialCharacters_ReturnsTrue()
        {
            // Arrange
            var featureSettings = CreateFeatureSettings(new[]
            {
                new FeatureEnablement { Name = "Feature-With-Dashes", Enabled = true },
                new FeatureEnablement { Name = "Feature_With_Underscores", Enabled = false },
                new FeatureEnablement { Name = "Feature.With.Dots", Enabled = true }
            });
            _featureSettingsMock.Setup(x => x.GetSettingsAsync()).ReturnsAsync(featureSettings);

            // Act & Assert
            Assert.That(await _subject.FeatureEnabled("Feature-With-Dashes"), Is.True);
            Assert.That(await _subject.FeatureEnabled("Feature_With_Underscores"), Is.False);
            Assert.That(await _subject.FeatureEnabled("Feature.With.Dots"), Is.True);
        }

        private static FeatureSettings CreateFeatureSettings(FeatureEnablement[] features)
        {
            return new FeatureSettings
            {
                Features = features?.ToList() ?? new List<FeatureEnablement>()
            };
        }
    }
}
