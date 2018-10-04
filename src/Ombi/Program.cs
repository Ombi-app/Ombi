using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using CommandLine;
using CommandLine.Text;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Ombi.Helpers;

namespace Ombi
{
    public class Program
    {
        private static string UrlArgs { get; set; }
        public static void Main(string[] args)
        {
            Console.Title = "Ombi";

            var host = string.Empty;
            var storagePath = string.Empty;
            var baseUrl = string.Empty;
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    host = o.Host;
                    storagePath = o.StoragePath;
                    baseUrl = o.BaseUrl;
                }).WithNotParsed(err =>
                {
                    foreach (var e in err)
                    {
                        Console.WriteLine(e);
                    }
                });

            Console.WriteLine(HelpOutput(result));

            UrlArgs = host;

            var urlValue = string.Empty;
            var instance = StoragePathSingleton.Instance;
            instance.StoragePath = storagePath ?? string.Empty;
            // Check if we need to migrate the settings
            CheckAndMigrate();
            using (var ctx = new SettingsContext())
            {
                var config = ctx.ApplicationConfigurations.ToList();
                var url = config.FirstOrDefault(x => x.Type == ConfigurationTypes.Url);
                var dbBaseUrl = config.FirstOrDefault(x => x.Type == ConfigurationTypes.BaseUrl);
                if (url == null)
                {
                    url = new ApplicationConfiguration
                    {
                        Type = ConfigurationTypes.Url,
                        Value = "http://*:5000"
                    };

                    ctx.ApplicationConfigurations.Add(url);
                    ctx.SaveChanges();
                    urlValue = url.Value;
                }
                if (!url.Value.Equals(host))
                {
                    url.Value = UrlArgs;
                    ctx.SaveChanges();
                    urlValue = url.Value;
                }

                if (dbBaseUrl == null)
                {
                    if (baseUrl.HasValue() && baseUrl.StartsWith("/"))
                    {
                        dbBaseUrl = new ApplicationConfiguration
                        {
                            Type = ConfigurationTypes.BaseUrl,
                            Value = baseUrl
                        };
                        ctx.ApplicationConfigurations.Add(dbBaseUrl);
                        ctx.SaveChanges();
                    }
                }
                else if (baseUrl.HasValue() && !baseUrl.Equals(dbBaseUrl.Value))
                {
                    dbBaseUrl.Value = baseUrl;
                    ctx.SaveChanges();
                }
            }

            DeleteSchedulesDb();

            Console.WriteLine($"We are running on {urlValue}");

            BuildWebHost(args).Run();
        }

        /// <summary>
        /// This is to remove the Settings from the Ombi.db to the "new" 
        /// OmbiSettings.db
        /// 
        /// Ombi is hitting a limitation with SQLite where there is a lot of database activity
        /// and SQLite does not handle concurrency at all, causing db locks.
        /// 
        /// Splitting it all out into it's own DB helps with this.
        /// </summary>
        private static void CheckAndMigrate()
        {
            var doneGlobal = false;
            var doneConfig = false;
            using (var ombi = new OmbiContext())
            using (var settings = new SettingsContext())
            {
                try
                {
                    if (ombi.Settings.Any())
                    {
                        // OK migrate it!
                        var allSettings = ombi.Settings.ToList();
                        settings.Settings.AddRange(allSettings);
                        doneGlobal = true;
                    }

                    // Check for any application settings

                    if (ombi.ApplicationConfigurations.Any())
                    {
                        // OK migrate it!
                        var allSettings = ombi.ApplicationConfigurations.ToList();
                        settings.ApplicationConfigurations.AddRange(allSettings);
                        doneConfig = true;
                    }

                    settings.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                // Now delete the old stuff
                if(doneGlobal)
                    ombi.Database.ExecuteSqlCommand("TRUNCATE TABLE GlobalSettings");
                if(doneConfig)
                    ombi.Database.ExecuteSqlCommand("TRUNCATE TABLE ApplicationConfiguration");

            }

        }

        private static void DeleteSchedulesDb()
        {
            try
            {
                if (File.Exists("Schedules.db"))
                {
                    File.Delete("Schedules.db");
                }
            }
            catch (Exception)
            {
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(UrlArgs)
                .PreferHostingUrls(true)
                .Build();

        private static string HelpOutput(ParserResult<Options> args)
        {
            var result = new StringBuilder();

            result.AppendLine("Hello, welcome to Ombi");
            result.AppendLine("Valid options are:");
            result.AppendLine(HelpText.AutoBuild(args, null, null));

            return result.ToString();
        }
    }

    public class Options
    {
        [Option("host", Required = false, HelpText =
            "Set to a semicolon-separated (;) list of URL prefixes to which the server should respond. For example, http://localhost:123." +
            " Use \"*\" to indicate that the server should listen for requests on any IP address or hostname using the specified port and protocol (for example, http://*:5000). " +
            "The protocol (http:// or https://) must be included with each URL. Supported formats vary between servers.", Default = "http://*:5000")]
        public string Host { get; set; }

        [Option("storage", Required = false, HelpText = "Storage path, where we save the logs and database")]
        public string StoragePath { get; set; }

        [Option("baseurl", Required = false, HelpText = "The base URL for reverse proxy scenarios")]
        public string BaseUrl { get; set; }

    }
}
