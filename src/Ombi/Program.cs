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
            var urlArgs = "http://*:5000";
            if (args.Length <= 0)
            {
                Console.WriteLine("No URL provided, we will run on \"http://localhost:5000\"");
                Console.WriteLine("Please provider the argument -url e.g. \"ombi.exe -url http://ombi.io:80/\"");
            }
            else
            {
                if (args[0].Contains("-url"))
                {
                    urlArgs = args[0].Replace("-url ", string.Empty);
                }
            }
            var urlValue = string.Empty;
            using (var ctx = new OmbiContext())
            {
                var url = ctx.ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.Url);
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
                else if (!url.Value.Equals(urlArgs))
                {
                    url.Value = urlArgs;
                    ctx.SaveChanges();
                    urlValue = url.Value;
                }
            }

            Console.WriteLine($"We are running on {urlValue}");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(urlValue)
                .UseStartup<Startup>()
                .Build();


            host.Run();
        }
    }
}
