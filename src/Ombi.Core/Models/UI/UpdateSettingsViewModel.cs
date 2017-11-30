using System.Runtime.InteropServices;
using Ombi.Settings.Settings.Models;

namespace Ombi.Core.Models.UI
{
    public class UpdateSettingsViewModel : UpdateSettings
    {
        public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}