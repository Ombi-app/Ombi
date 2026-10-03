using System;
using System.Threading.Tasks;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using Ombi.Core.Processor;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Settings.Settings.Models;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    [NonParallelizable] // mutates the process-wide OMBI_INSTALL_METHOD env var
    public class OmbiAutomaticUpdaterTests
    {
        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, null);
        }

        [TearDown]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, null);
        }

        [Test]
        public async Task Execute_Skips_When_PackageManaged_EvenIfAutoUpdateEnabled()
        {
            // apt-managed install: the package manager owns updates, so the self-updater must not run,
            // even when AutoUpdateEnabled is already persisted as true in the database.
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, "apt");

            var mocker = new AutoMocker();
            var settings = mocker.GetMock<ISettingsService<UpdateSettings>>();
            settings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new UpdateSettings { AutoUpdateEnabled = true });

            var job = mocker.CreateInstance<OmbiAutomaticUpdater>();

            await job.Execute(null);

            // Guard runs before the settings read and before any update check / download.
            settings.Verify(x => x.GetSettingsAsync(), Times.Never);
            mocker.GetMock<IChangeLogProcessor>().Verify(x => x.Process(), Times.Never);
        }

        [Test]
        public async Task Execute_ProceedsPastGuard_When_NotPackageManaged()
        {
            // Normal (manual) install: the guard must be transparent. With AutoUpdateEnabled = false the
            // job stops at the settings check, which proves we got past the guard without breaking anything.
            Environment.SetEnvironmentVariable(InstallMethodHelper.EnvVarName, null);

            var mocker = new AutoMocker();
            var settings = mocker.GetMock<ISettingsService<UpdateSettings>>();
            settings.Setup(x => x.GetSettingsAsync()).ReturnsAsync(new UpdateSettings { AutoUpdateEnabled = false });

            var job = mocker.CreateInstance<OmbiAutomaticUpdater>();

            await job.Execute(null);

            settings.Verify(x => x.GetSettingsAsync(), Times.Once);
            mocker.GetMock<IChangeLogProcessor>().Verify(x => x.Process(), Times.Never);
        }
    }
}
