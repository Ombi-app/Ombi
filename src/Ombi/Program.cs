using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using CommandLine;
using CommandLine.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Ombi.Extensions;
using Ombi.Helpers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Ombi.Settings.Settings.Models;
using System.Diagnostics;
using System.IO;

namespace Ombi
{
    public static class Program
    {
        private static string UrlArgs { get; set; }

        public static async Task Main(string[] args)
        {
            Console.Title = "Ombi";

            var host = string.Empty;
            var storagePath = string.Empty;
            var baseUrl = string.Empty;
            var demo = false;
            var migrate = false;
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    host = o.Host;
                    storagePath = o.StoragePath;
                    baseUrl = o.BaseUrl;
                    demo = o.Demo;
                    migrate = o.Migrate;
                }).WithNotParsed(err =>
                {
                    foreach (var e in err)
                    {
                        Console.WriteLine(e);
                    }
                });



            Console.WriteLine(HelpOutput(result));
            if (baseUrl.HasValue())
            {
                Console.WriteLine($"Base Url: {baseUrl}");
            }
            UrlArgs = host;

            var urlValue = string.Empty;
            var instance = StartupSingleton.Instance;
            var demoInstance = DemoSingleton.Instance;
            demoInstance.Demo = demo;
            instance.StoragePath = storagePath ?? string.Empty;


            var services = new ServiceCollection();
            services.ConfigureDatabases(null);
            using (var provider = services.BuildServiceProvider())
            {
                var settingsDb = provider.GetRequiredService<SettingsContext>();
                var ombiDb = provider.GetRequiredService<OmbiContext>();

                if (migrate)
                {
                    Console.WriteLine("Migrate in progress...");

                    var migrationTasks = new List<Task>();
                    var externalDb = provider.GetRequiredService<ExternalContext>();
                    migrationTasks.Add(settingsDb.Database.MigrateAsync());
                    migrationTasks.Add(ombiDb.Database.MigrateAsync());
                    migrationTasks.Add(externalDb.Database.MigrateAsync());

                    Task.WaitAll(migrationTasks.ToArray());

                    Console.WriteLine("Migrate complete.");
                    Environment.Exit(0);
                }

                var config = await settingsDb.ApplicationConfigurations.ToListAsync();
                var url = config.FirstOrDefault(x => x.Type == ConfigurationTypes.Url);
                var ombiSettingsContent = await settingsDb.Settings.FirstOrDefaultAsync(x => x.SettingsName == "OmbiSettings");
                var securityToken = config.FirstOrDefault(x => x.Type == ConfigurationTypes.SecurityToken);
                await CheckSecurityToken(securityToken, settingsDb, instance);
                if (url == null)
                {
                    url = new ApplicationConfiguration
                    {
                        Type = ConfigurationTypes.Url,
                        Value = "http://*:5000"
                    };
                    var strat = settingsDb.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        using (var tran = await settingsDb.Database.BeginTransactionAsync())
                        {
                            settingsDb.ApplicationConfigurations.Add(url);
                            await settingsDb.SaveChangesAsync();
                            await tran.CommitAsync();
                        }
                    });

                    urlValue = url.Value;
                }

                if (!url.Value.Equals(host))
                {
                    url.Value = UrlArgs;
                    var strat = settingsDb.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        using (var tran = await settingsDb.Database.BeginTransactionAsync())
                        {
                            await settingsDb.SaveChangesAsync();
                            await tran.CommitAsync();
                        }
                    });

                    urlValue = url.Value;
                }
                else if (string.IsNullOrEmpty(urlValue))
                {
                    urlValue = host;
                }

                await SortOutBaseUrl(baseUrl, settingsDb, ombiSettingsContent);

                Console.WriteLine($"We are running on {urlValue}");

                CreateHostBuilder(args).Build().Run();
            }
        }

        private static async Task CheckSecurityToken(ApplicationConfiguration securityToken, SettingsContext ctx, StartupSingleton instance)
        {
            if (securityToken == null || string.IsNullOrEmpty(securityToken.Value))
            {
                securityToken = new ApplicationConfiguration
                {
                    Type = ConfigurationTypes.SecurityToken,
                    Value = Guid.NewGuid().ToString("N")
                };
                var strat = ctx.Database.CreateExecutionStrategy();
                await strat.ExecuteAsync(async () =>
                {
                    using (var tran = await ctx.Database.BeginTransactionAsync())
                    {
                        ctx.ApplicationConfigurations.Add(securityToken);
                        await ctx.SaveChangesAsync();
                        await tran.CommitAsync();
                    }
                });
            }

            instance.SecurityKey = securityToken.Value;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        // Set properties and call methods on options
                    });
                    webBuilder.PreferHostingUrls(true)
                    .UseUrls(UrlArgs)
                        .UseStartup<Startup>();
                });

        private static string HelpOutput(ParserResult<Options> args)
        {
            var result = new StringBuilder();

            result.AppendLine("Hello, welcome to Ombi");
            result.AppendLine("Valid options are:");
            result.AppendLine(HelpText.AutoBuild(args, null, null));

            return result.ToString();
        }

        private static async Task SortOutBaseUrl(string baseUrl, SettingsContext settingsDb, GlobalSettings ombiSettingsContent)
        {
            var setBaseUrl = false;
            if (ombiSettingsContent == null)
            {
                Console.WriteLine("Creating new Settings entity");
                ombiSettingsContent = new GlobalSettings
                {
                    SettingsName = "OmbiSettings",
                    Content = JsonConvert.SerializeObject(new OmbiSettings())
                };
                var strat = settingsDb.Database.CreateExecutionStrategy();
                await strat.ExecuteAsync(async () =>
                {
                    using (var tran = await settingsDb.Database.BeginTransactionAsync())
                    {
                        settingsDb.Add(ombiSettingsContent);
                        await settingsDb.SaveChangesAsync();
                        await tran.CommitAsync();
                    }
                });
            }
            var ombiSettings = JsonConvert.DeserializeObject<OmbiSettings>(ombiSettingsContent.Content);
            if (ombiSettings == null)
            {
                if (baseUrl.HasValue() && baseUrl.StartsWith("/"))
                {
                    setBaseUrl = true;
                    ombiSettings = new OmbiSettings
                    {
                        BaseUrl = baseUrl
                    };

                    ombiSettingsContent.Content = JsonConvert.SerializeObject(ombiSettings);
                    var strat = settingsDb.Database.CreateExecutionStrategy();
                    await strat.ExecuteAsync(async () =>
                    {
                        using (var tran = await settingsDb.Database.BeginTransactionAsync())
                        {
                            settingsDb.Update(ombiSettingsContent);
                            await settingsDb.SaveChangesAsync();
                            await tran.CommitAsync();
                        }
                    });
                }
            }
            else if (baseUrl.HasValue() && !baseUrl.Equals(ombiSettings.BaseUrl))
            {
                setBaseUrl = true;
                ombiSettings.BaseUrl = baseUrl;

                ombiSettingsContent.Content = JsonConvert.SerializeObject(ombiSettings);
                var strat = settingsDb.Database.CreateExecutionStrategy();
                await strat.ExecuteAsync(async () =>
                {
                    using (var tran = await settingsDb.Database.BeginTransactionAsync())
                    {
                        settingsDb.Update(ombiSettingsContent);
                        await settingsDb.SaveChangesAsync();
                        await tran.CommitAsync();
                    }
                });
            }


            if (setBaseUrl)
            {
                var trimmedBaseUrl = baseUrl.EndsWith('/') ? baseUrl.TrimEnd('/') : baseUrl;
                var process = Process.GetCurrentProcess().MainModule.FileName;
                var ombiInstalledDir = Path.GetDirectoryName(process);
                var indexPath = Path.Combine(ombiInstalledDir, "ClientApp", "dist", "index.html");
                if (!File.Exists(indexPath))
                {
                    var error = $"Can't set the base URL because we cannot find the file at {indexPath}, if you are trying to set a base url please report this on Github!";
                    Console.WriteLine(error);
                    throw new Exception(error);
                }
                var indexHtml = await File.ReadAllTextAsync(indexPath);
                indexHtml = indexHtml.Replace("<script type='text/javascript'>window[\"baseHref\"] = '/';</script>"
                   , $"<script type='text/javascript'>window[\"baseHref\"] = '{trimmedBaseUrl}';</script><base href=\"{baseUrl}\">", StringComparison.InvariantCultureIgnoreCase);

                await File.WriteAllTextAsync(indexPath, indexHtml);

                Console.WriteLine($"Wrote new baseurl at {indexPath}");
            }
        }
    }

    public class Options
    {
        [Option("host", Required = false, HelpText =
            "Set to a semicolon-separated (;) list of URL prefixes to which the server should respond. For example, http://localhost:123." +
            " Use \"localhost\" to indicate that the server should listen for requests on any IP address or hostname using the specified port and protocol (for example, http://localhost:5000). " +
            "The protocol (http:// or https://) must be included with each URL. Supported formats vary between servers.", Default = "http://*:5000")]
        public string Host { get; set; }

        [Option("storage", Required = false, HelpText = "Storage path, where we save the logs and database")]
        public string StoragePath { get; set; }

        [Option("baseurl", Required = false, HelpText = "The base URL for reverse proxy scenarios")]
        public string BaseUrl { get; set; }

        [Option("demo", Required = false, HelpText = "Demo mode, you will never need to use this, fuck that fruit company...")]
        public bool Demo { get; set; }

        [Option("migrate", Required = false, HelpText = "Will run the migrations then exit the application")]
        public bool Migrate { get; set; }

    }
}
