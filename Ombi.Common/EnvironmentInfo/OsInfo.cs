using System;
using System.IO;

namespace Ombi.Common.EnvironmentInfo
{
    public class OsInfo
    {
        public static Os Os { get; }

        public static bool IsNotWindows => !IsWindows;
        public static bool IsLinux => Os == Os.Linux;
        public static bool IsOsx => Os == Os.Osx;
        public static bool IsWindows => Os == Os.Windows;
        
        static OsInfo()
        {
            var platform = Environment.OSVersion.Platform;

            switch (platform)
            {
                case PlatformID.Win32NT:
                {
                    Os = Os.Windows;
                    break;
                }
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                {
                    // Sometimes Mac OS reports itself as Unix
                    if (Directory.Exists("/System/Library/CoreServices/") &&
                        (File.Exists("/System/Library/CoreServices/SystemVersion.plist") ||
                         File.Exists("/System/Library/CoreServices/ServerVersion.plist"))
                    )
                    {
                        Os = Os.Osx;
                    }
                    else
                    {
                        Os = Os.Linux;
                    }
                    break;
                }
            }
        }

    }

    public enum Os
    {
        Windows,
        Linux,
        Osx
    }
}