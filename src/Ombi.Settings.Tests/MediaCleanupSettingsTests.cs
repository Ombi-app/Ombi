using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using Ombi.Settings.Settings.Models;

namespace Ombi.Settings.Tests
{
    [TestFixture]
    public class MediaCleanupSettingsTests
    {
        [TestCase(-1)]
        [TestCase(99)]
        public void InvalidOwnRequestRemovalMode_FailsValidation(int rawValue)
        {
            var settings = new MediaCleanupSettings
            {
                OwnRequestRemoval = (OwnRequestRemovalMode)rawValue
            };

            var validationResults = Validate(settings);

            Assert.That(validationResults.Any(x =>
                x.MemberNames.Contains(nameof(MediaCleanupSettings.OwnRequestRemoval))), Is.True);
        }

        [TestCase(-1)]
        [TestCase(99)]
        public void InvalidCommunityCleanupMode_FailsValidation(int rawValue)
        {
            var settings = new MediaCleanupSettings
            {
                CommunityCleanup = (CommunityCleanupMode)rawValue
            };

            var validationResults = Validate(settings);

            Assert.That(validationResults.Any(x =>
                x.MemberNames.Contains(nameof(MediaCleanupSettings.CommunityCleanup))), Is.True);
        }

        [Test]
        public void DefinedCleanupModes_PassEnumValidation()
        {
            var settings = new MediaCleanupSettings
            {
                OwnRequestRemoval = OwnRequestRemovalMode.ImmediateDeletion,
                CommunityCleanup = CommunityCleanupMode.AutomaticAfterThreshold
            };

            Assert.That(Validate(settings), Is.Empty);
        }

        private static List<ValidationResult> Validate(MediaCleanupSettings settings)
        {
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(
                settings,
                new ValidationContext(settings),
                validationResults,
                validateAllProperties: true);
            return validationResults;
        }
    }
}
