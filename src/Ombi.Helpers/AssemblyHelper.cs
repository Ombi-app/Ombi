using System;
using System.IO;
using System.Text.Json;

namespace Ombi.Helpers
{
    public static class AssemblyHelper
    {
        public static string GetRuntimeVersion()
        {
            Version version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            var assemblyVersion = version == null ? null : $"{version.Major}.{version.Minor}.{version.Build}";

            // Assembly version defaults to 0.0.0 or 1.0.0 when SemVer isn't set during build.
            // Fall back to version.json which contains the canonical version.
            if (string.IsNullOrEmpty(assemblyVersion) || assemblyVersion == "0.0.0" || assemblyVersion == "1.0.0")
            {
                var versionFromFile = ReadVersionFromFile();
                if (!string.IsNullOrEmpty(versionFromFile))
                {
                    return versionFromFile;
                }
            }

            return assemblyVersion ?? "1.0.0";
        }

        private static string ReadVersionFromFile()
        {
            try
            {
                // Look for version.json next to the running executable
                var basePath = AppContext.BaseDirectory;
                var versionFile = Path.Combine(basePath, "version.json");
                if (!File.Exists(versionFile))
                {
                    return null;
                }

                var json = File.ReadAllText(versionFile);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("version", out var versionElement))
                {
                    return versionElement.GetString();
                }
            }
            catch
            {
                // Swallow errors — fall back to assembly version
            }

            return null;
        }
    }
}