using System;

namespace Ombi.Helpers
{
    /// <summary>
    /// Detects how this Ombi instance was installed, via the OMBI_INSTALL_METHOD
    /// environment variable. Package-managed installs (e.g. apt) defer to the system
    /// package manager instead of running the built-in self-updater.
    /// </summary>
    public static class InstallMethodHelper
    {
        public const string EnvVarName = "OMBI_INSTALL_METHOD";
        public const string Apt = "apt";

        /// <summary>
        /// Normalised install method (lower-cased, trimmed). Empty when unset
        /// (a manual / self-managed install).
        /// </summary>
        public static string InstallMethod =>
            (Environment.GetEnvironmentVariable(EnvVarName) ?? string.Empty).Trim().ToLowerInvariant();

        public static bool IsApt => string.Equals(InstallMethod, Apt, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// True when an external package manager owns updates and Ombi must NOT
        /// run its built-in self-updater. Currently apt only.
        /// </summary>
        public static bool IsPackageManaged => IsApt;
    }
}
