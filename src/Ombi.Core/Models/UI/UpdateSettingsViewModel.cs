using System.Runtime.InteropServices;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;

namespace Ombi.Core.Models.UI
{
    public class UpdateSettingsViewModel : UpdateSettings
    {
        public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// True when this install is managed by an external package manager (e.g. apt);
        /// the built-in self-updater is disabled and updates are applied via the package manager.
        /// </summary>
        public bool IsManagedByPackageManager => InstallMethodHelper.IsPackageManaged;
    }
}