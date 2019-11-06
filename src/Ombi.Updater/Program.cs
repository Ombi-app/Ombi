using System;
using System.IO;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Ombi.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" =======================================");
            Console.WriteLine("        Starting the Ombi Updater"       );
            Console.WriteLine(" =======================================");


            var options = CheckArgs(args);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Run app
            serviceProvider.GetService<IInstaller>().Start(options);
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Add logging
            //serviceCollection.AddSingleton(new LoggerFactory()
            //    .AddConsole()
            //    .AddSerilog()
            //    .AddDebug());
            serviceCollection.AddLogging();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine("Logs", "log-{Date}.txt"))
                .Enrich.FromLogContext()
                .CreateLogger();
            
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton(configuration);

            //// Add services
            serviceCollection.AddTransient<IInstaller, Installer>();
            serviceCollection.AddTransient<IProcessProvider, ProcessProvider>();
        }

        private static StartupOptions CheckArgs(string[] args)
        {
            var result = Parser.Default.ParseArguments<StartupOptions>(args);
            StartupOptions opts = null;
            result.WithParsed(options => opts = options);
            return opts;
        }
    }

    public class StartupOptions
    {
        [Option("processname", Required = false, Default = "Ombi")]
        public string ProcessName { get; set; }
        [Option("applicationPath", Required = false)]
        public string ApplicationPath { get; set; }
        [Option("processId", Required = false)]
        public int OmbiProcessId { get; set; }
        [Option("host", Required = false)]
        public string Host { get; set; }
        [Option("storage", Required = false)]
        public string Storage { get; set; }
        [Option("windowsServiceName", Required = false)]
        public string WindowsServiceName { get; set; }

        public bool IsWindowsService => !string.IsNullOrEmpty(WindowsServiceName);
    }
}