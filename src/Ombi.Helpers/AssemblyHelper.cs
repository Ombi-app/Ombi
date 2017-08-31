using System;
using System.Reflection;

namespace Ombi.Helpers
{
    public static class AssemblyHelper
    {
        public static string GetRuntimeVersion() =>
            Assembly.GetEntryAssembly().GetType()
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyProductAttribute>()
                .Product;
    }
}