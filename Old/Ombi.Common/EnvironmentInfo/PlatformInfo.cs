using System;

namespace Ombi.Common.EnvironmentInfo
{
    public enum PlatformType
    {
        DotNet = 0,
        Mono = 1
    }

    public interface IPlatformInfo
    {
        Version Version { get; }
    }

    public abstract class PlatformInfo : IPlatformInfo
    {
        static PlatformInfo()
        {
            Platform = Type.GetType("Mono.Runtime") != null ? PlatformType.Mono : PlatformType.DotNet;
        }

        public static PlatformType Platform { get; }
        public static bool IsMono => Platform == PlatformType.Mono;
        public static bool IsDotNet => Platform == PlatformType.DotNet;

        public static string PlatformName
        {
            get
            {
                if (IsDotNet)
                {
                    return ".NET";
                }

                return "Mono";
            }
        }

        public abstract Version Version { get; }
    }
}