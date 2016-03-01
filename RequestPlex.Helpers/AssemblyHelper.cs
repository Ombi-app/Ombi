using System.Diagnostics;
using System.Reflection;

namespace RequestPlex.Helpers
{
    public class AssemblyHelper
    {
        public static string GetAssemblyVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }
    }
}