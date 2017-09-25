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

namespace Ombi
{
    public class Program
    {
        private static string UrlArgs { get; set; }
        private static string WebRoot { get; set; }
        public static void Main(string[] args)
        {
            Console.Title = "Ombi";

            var host = string.Empty;
            var storagePath = string.Empty;
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    host = o.Host;
                    WebRoot = o.WebRoot;
                    storagePath = o.StoragePath;
                });

            Console.WriteLine(HelpOutput(result));

            UrlArgs = host;

            var urlValue = string.Empty;
            using (var ctx = new OmbiContext())
            {
                var config = ctx.ApplicationConfigurations.ToList();
                var url = config.FirstOrDefault(x => x.Type == ConfigurationTypes.Url);
                if (url == null)
                {
                    url = new ApplicationConfiguration
                    {
                        Type = ConfigurationTypes.Url,
                        Value = "http://*"
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
            }

            Console.WriteLine($"We are running on {urlValue}");

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(UrlArgs)
                .PreferHostingUrls(true)
                .UseWebRoot(WebRoot)
                .Build();

        private static string HelpOutput(ParserResult<Options> args)
        {
            var result = new StringBuilder();

            result.AppendLine("Hello, and welcome to the  console application.");
            result.AppendLine("This application takes in a data file and attempts to import that data into our systems.");
            result.AppendLine("Valid options are:");
            result.AppendLine(HelpText.AutoBuild(args, null, null));
            result.AppendLine("Press any key to exit");

            return result.ToString();
        }
    }

    public class Options
    {
        [Option('h', "host", Required = false, HelpText =
            "Set to a semicolon-separated (;) list of URL prefixes to which the server should respond. For example, http://localhost:123." +
            " Use \"*\" to indicate that the server should listen for requests on any IP address or hostname using the specified port and protocol (for example, http://*:5000). " +
            "The protocol (http:// or https://) must be included with each URL. Supported formats vary between servers.", Default = "http://*:5000")]
        public string Host { get; set; }

        [Option('s', "storage", Required = false, HelpText = "Storage path, where we save the logs and database")]
        public string StoragePath { get; set; }

        [Option('w', "webroot", Required = false, HelpText = "(Root Path for Reverse Proxies) If not specified, the default is \"(Content Root)/wwwroot\", if the path exists. If the path doesn\'t exist, then a no-op file provider is used.")]
        public string WebRoot { get; set; }
        
    }
}
