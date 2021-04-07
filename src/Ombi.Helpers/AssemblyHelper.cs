using Microsoft.Extensions.PlatformAbstractions;
using System.Linq;
using System.Reflection;

namespace Ombi.Helpers
{
    public static class AssemblyHelper
    {
        public static string GetRuntimeVersion()
        {
            ApplicationEnvironment app = PlatformServices.Default.Application;
            var split = app.ApplicationVersion.Split('.');
            return string.Join('.', split.Take(3));
        }
    }
}