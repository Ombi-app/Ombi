using System;
using NUnit.Framework;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class InstallMethodHelperTests
    {
        [TearDown]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, null);
        }

        [Test]
        public void Unset_IsNotPackageManaged()
        {
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, null);

            Assert.That(InstallMethodHelper.InstallMethod, Is.Empty);
            Assert.That(InstallMethodHelper.IsApt, Is.False);
            Assert.That(InstallMethodHelper.IsPackageManaged, Is.False);
        }

        [Test]
        public void Apt_IsPackageManaged()
        {
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, "apt");

            Assert.That(InstallMethodHelper.InstallMethod, Is.EqualTo("apt"));
            Assert.That(InstallMethodHelper.IsApt, Is.True);
            Assert.That(InstallMethodHelper.IsPackageManaged, Is.True);
        }

        [TestCase("APT")]
        [TestCase("Apt")]
        [TestCase("  apt  ")]
        public void Apt_IsDetected_CaseInsensitive_AndTrimmed(string value)
        {
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, value);

            Assert.That(InstallMethodHelper.IsApt, Is.True);
            Assert.That(InstallMethodHelper.IsPackageManaged, Is.True);
        }

        [TestCase("docker")]
        [TestCase("manual")]
        [TestCase("garbage")]
        public void UnknownValues_AreNotPackageManaged(string value)
        {
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, value);

            Assert.That(InstallMethodHelper.IsApt, Is.False);
            Assert.That(InstallMethodHelper.IsPackageManaged, Is.False);
        }
    }
}
