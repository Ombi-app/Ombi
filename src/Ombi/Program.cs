using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
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
            var demo = false;
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    host = o.Host;
                    storagePath = o.StoragePath;
                    baseUrl = o.BaseUrl;
                    demo = o.Demo;
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
            var demoInstance = DemoSingleton.Instance;
            demoInstance.Demo = demo;
            instance.StoragePath = storagePath ?? string.Empty;
            // Check if we need to migrate the settings
            CheckAndMigrate();
            var ctx = new SettingsContext();
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

            DeleteSchedulesDb();

            Console.WriteLine($"We are running on {urlValue}");

            CreateWebHostBuilder(args).Build().Run();
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
            var ombi = new OmbiContext();
            var settings = new SettingsContext();

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
            if (doneGlobal)
                ombi.Database.ExecuteSqlCommand("DELETE FROM GlobalSettings");
            if (doneConfig)
                ombi.Database.ExecuteSqlCommand("DELETE FROM ApplicationConfiguration");

            // Now migrate all the external stuff
            var external = new ExternalContext();

            try
            {
                if (ombi.PlexEpisode.Any())
                {
                    external.PlexEpisode.AddRange(ombi.PlexEpisode.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM PlexEpisode");
                }

                if (ombi.PlexSeasonsContent.Any())
                {
                    external.PlexSeasonsContent.AddRange(ombi.PlexSeasonsContent.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM PlexSeasonsContent");
                }
                if (ombi.PlexServerContent.Any())
                {
                    external.PlexServerContent.AddRange(ombi.PlexServerContent.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM PlexServerContent");
                }
                if (ombi.EmbyEpisode.Any())
                {
                    external.EmbyEpisode.AddRange(ombi.EmbyEpisode.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM EmbyEpisode");
                }

                if (ombi.EmbyContent.Any())
                {
                    external.EmbyContent.AddRange(ombi.EmbyContent.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM EmbyContent");
                }
                if (ombi.RadarrCache.Any())
                {
                    external.RadarrCache.AddRange(ombi.RadarrCache.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM RadarrCache");
                }
                if (ombi.SonarrCache.Any())
                {
                    external.SonarrCache.AddRange(ombi.SonarrCache.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM SonarrCache");
                }
                if (ombi.LidarrAlbumCache.Any())
                {
                    external.LidarrAlbumCache.AddRange(ombi.LidarrAlbumCache.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM LidarrAlbumCache");
                }
                if (ombi.LidarrArtistCache.Any())
                {
                    external.LidarrArtistCache.AddRange(ombi.LidarrArtistCache.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM LidarrArtistCache");
                }
                if (ombi.SickRageEpisodeCache.Any())
                {
                    external.SickRageEpisodeCache.AddRange(ombi.SickRageEpisodeCache.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM SickRageEpisodeCache");
                }
                if (ombi.SickRageCache.Any())
                {
                    external.SickRageCache.AddRange(ombi.SickRageCache.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM SickRageCache");
                }
                if (ombi.CouchPotatoCache.Any())
                {
                    external.CouchPotatoCache.AddRange(ombi.CouchPotatoCache.ToList());
                    ombi.Database.ExecuteSqlCommand("DELETE FROM CouchPotatoCache");
                }

                external.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(UrlArgs)
                .PreferHostingUrls(true);

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

        [Option("demo", Required = false, HelpText = "Demo mode, you will never need to use this, fuck that fruit company...")]
        public bool Demo { get; set; }

    }
}
