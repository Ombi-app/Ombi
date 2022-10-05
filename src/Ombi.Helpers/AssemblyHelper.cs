using System;

namespace Ombi.Helpers
{
    public static class AssemblyHelper
    {
        public static string GetRuntimeVersion()
        {
            Version version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            return version == null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}