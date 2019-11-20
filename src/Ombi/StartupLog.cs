using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ILogger = Serilog.ILogger;
using Ombi.Helpers;


namespace Ombi
{
    public class StartupLog
    {
        public static StoragePathSingleton StoragePath => StoragePathSingleton.Instance;

        private string ContentRootPath
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.IndexOf("bin\\"))); }
        }

        public StartupLog()
        {
            //this.config();
        }

        public void Config()
        {
            ILogger log_config = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.RollingFile(Path.Combine(StoragePath.StoragePath.IsNullOrEmpty() ? this.ContentRootPath : StoragePath.StoragePath, "Logs", "startup-{Date}.txt"))
               .CreateLogger();
            Log.Logger = log_config;
        }

        public void LogError(string str)
        {
            Console.WriteLine(str);
            Log.Error(str);
        }

        public void LogInformation(string str)
        {
            Console.WriteLine(str);
            Log.Information(str);
        }
    }
}
