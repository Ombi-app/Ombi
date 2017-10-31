using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

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
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddConsole()
                .AddSerilog()
                .AddDebug());
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
            if(args.Length <= 1)
            {
                Console.WriteLine("No Args Provided... Exiting");
                Environment.Exit(1);
            }
            var startup = new StartupOptions
            {
                ApplicationPath = args[0],
                ProcessName = args[1],
            };
            if (args.Length == 4)
            {
                startup.StartupArgs = args[2] + " " + args[3];
            }
            else if (args.Length == 3)
            {
                startup.StartupArgs = args[2];
            }

            var p = new ProcessProvider();
            var ombiProc = p.FindProcessByName(startup.ProcessName).FirstOrDefault();

            startup.OmbiProcessId = ombiProc?.Id ?? -1;

            return startup;
        }
    }

    public class StartupOptions
    {
        public string ProcessName { get; set; }
        public string ApplicationPath { get; set; }
        public int OmbiProcessId { get; set; }
        public string StartupArgs { get; set; }
    }
}