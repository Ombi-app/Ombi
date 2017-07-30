using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Ombi";
            var port = 5000;
            var urlArgs = $"http://*:{port}";
            if (args.Length <= 0)
            {
                Console.WriteLine("No URL provided, we will run on \"http://localhost:5000\"");
                //Console.WriteLine("Please provider the argument -url e.g. \"ombi.exe -url http://ombi.io:80/\"");
            }
            else
            {
                if (args[0].Contains("-url"))
                {
                    try
                    {
                        urlArgs = args[0].Replace("-url ", string.Empty);
                        var index = urlArgs.IndexOf(':', urlArgs.IndexOf(':') + 1);
                        var portString = urlArgs.Substring(index + 1, urlArgs.Length - index - 1);
                        port = int.Parse(portString);

                        urlArgs = urlArgs.Substring(0, urlArgs.Length - portString.Length - 1);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Port is not defined or correctly formatted");
                        Console.WriteLine(e.Message);
                        Console.ReadLine();
                        Environment.Exit(1);
                    }
                }
            }
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
                else if (!url.Value.Equals(urlArgs))
                {
                    url.Value = urlArgs;
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

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls($"{urlValue}:{port}")
                .UseStartup<Startup>()
                .Build();


            host.Run();
        }
    }
}
