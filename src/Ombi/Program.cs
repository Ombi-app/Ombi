using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using CommandLine;
using Microsoft.AspNetCore;

namespace Ombi
{
    public class Program
    {
        private static string UrlArgs { get; set; }
        public static void Main(string[] args)
        {
            Console.Title = "Ombi";

            int port = 0;
            string host = string.Empty;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    port = o.Port;
                    host = o.Host;
                });
            
            UrlArgs = $"{host}:{port}";
            
            var urlValue = string.Empty;
            using (var ctx = new OmbiContext())
            {
                var url = ctx.ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.Url);
                var savedPort = ctx.ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.Port);
                if (url == null && savedPort == null)
                {
                    url = new ApplicationConfiguration
                    {
                        Type = ConfigurationTypes.Url,
                        Value = "http://*"
                    };

                    var dbPort = new ApplicationConfiguration
                    {
                        Type = ConfigurationTypes.Port,
                        Value = "5000"
                    };

                    ctx.ApplicationConfigurations.Add(url);
                    ctx.ApplicationConfigurations.Add(dbPort);
                    ctx.SaveChanges();
                    urlValue = url.Value;
                    port = int.Parse(dbPort.Value);
                }
                if (url != null && !url.Value.Equals(host))
                {
                    url.Value = UrlArgs;
                    ctx.SaveChanges();
                    urlValue = url.Value;
                }

                if (savedPort != null && !savedPort.Value.Equals(port.ToString()))
                {
                    savedPort.Value = port.ToString() ;
                    ctx.SaveChanges();
                }

            }

            Console.WriteLine($"We are running on {urlValue}");

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(UrlArgs)
                .Build();
    }

    public class Options
    {
        [Option('h', "host", Required = false, HelpText = "The Hostname default is http://*", Default ="http://*")]
        public string Host { get; set; }

        [Option('p', "port", Required = false, HelpText = "The port, default is 5000", Default = 5000)]
        public int Port { get; set; }
    }
}
