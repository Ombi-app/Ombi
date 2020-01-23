using Microsoft.Extensions.PlatformAbstractions;
using System.Reflection;

namespace Ombi.Helpers
{
    public static class AssemblyHelper
    {
        public static string GetRuntimeVersion()
        {
            ApplicationEnvironment app = PlatformServices.Default.Application;
            return app.ApplicationVersion;
        }
    }
}