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
using Ombi.Api.TheMovieDb;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Ombi.Api.TheMovieDb.Models;

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
            using var provider = services.BuildServiceProvider();
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
                    using var tran = await settingsDb.Database.BeginTransactionAsync();
                    settingsDb.ApplicationConfigurations.Add(url);
                    await settingsDb.SaveChangesAsync();
                    await tran.CommitAsync();
                });

                urlValue = url.Value;
            }

            if (!url.Value.Equals(host))
            {
                url.Value = UrlArgs;
                var strat = settingsDb.Database.CreateExecutionStrategy();
                await strat.ExecuteAsync(async () =>
                {
                    using var tran = await settingsDb.Database.BeginTransactionAsync();
                    await settingsDb.SaveChangesAsync();
                    await tran.CommitAsync();
                });

                urlValue = url.Value;
            }
            else if (string.IsNullOrEmpty(urlValue))
            {
                urlValue = host;
            }

            await SortOutBaseUrl(baseUrl, settingsDb, ombiSettingsContent);
            var httpClient = new Ombi.Api.OmbiHttpClient(null, null);
            var api = new Ombi.Api.Api(new Logger<Api.Api>(NullLoggerFactory.Instance), httpClient);
            await MigrateOldTvDbIds(ombiDb, ombiSettingsContent, settingsDb, new Ombi.Api.TheMovieDb.TheMovieDbApi(null, (Api.IApi)api, null));

            Console.WriteLine($"We are running on {urlValue}");

            CreateHostBuilder(args).Build().Run();
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
                    using var tran = await ctx.Database.BeginTransactionAsync();
                    ctx.ApplicationConfigurations.Add(securityToken);
                    await ctx.SaveChangesAsync();
                    await tran.CommitAsync();
                });
            }

            instance.SecurityKey = securityToken.Value;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(_ =>
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
                    using var tran = await settingsDb.Database.BeginTransactionAsync();
                    settingsDb.Add(ombiSettingsContent);
                    await settingsDb.SaveChangesAsync();
                    await tran.CommitAsync();
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
            else
            {
                // The base url might have changed in the settings, so just rewrite
                setBaseUrl = true;
                baseUrl = ombiSettings.BaseUrl.HasValue() ? ombiSettings.BaseUrl : string.Empty;
            }


            if (setBaseUrl)
            {
                var trimmedBaseUrl = baseUrl.EndsWith('/') ? baseUrl.TrimEnd('/') : baseUrl;
                var process = AppContext.BaseDirectory;
                var ombiInstalledDir = Path.GetDirectoryName(process);
                var indexPath = Path.Combine(ombiInstalledDir, "ClientApp", "dist", "index.html");
                if (!File.Exists(indexPath))
                {
                    var error = $"Can't set the base URL because we cannot find the file at '{indexPath}', if you are trying to set a base url please report this on Github!";
                    Console.WriteLine(error);
                    return;
                }
                var indexHtml = await File.ReadAllTextAsync(indexPath);
                var sb = new StringBuilder(indexHtml);

                var headPosition = indexHtml.IndexOf("<head>");
                var firstLinkPosition = indexHtml.IndexOf("<link");

                sb.Remove(headPosition + 6, firstLinkPosition - headPosition - 6);

                sb.Insert(headPosition + 6,
                    $"<script type='text/javascript'>window[\"baseHref\"] = '{trimmedBaseUrl}';</script><base href=\"{trimmedBaseUrl}/\">");

                try
                {
                    await File.WriteAllTextAsync(indexPath, sb.ToString());
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Error attempting to write Base URL, see here: https://docs.ombi.app/info/known-faults/#unauthorized-access-to-indexhtml");
                }

                Console.WriteLine($"Wrote new baseurl at {indexPath}");
            }
        }

        private static async Task MigrateOldTvDbIds(OmbiContext ctx, GlobalSettings ombiSettingsContent, SettingsContext settingsContext, Api.TheMovieDb.TheMovieDbApi api)
        {
            var ombiSettings = JsonConvert.DeserializeObject<OmbiSettings>(ombiSettingsContent.Content);
            if (ombiSettings.HasMigratedOldTvDbData)
            {
                return;
            }

            var tvRequests = ctx.TvRequests;
            Console.WriteLine($"Total Requests to migrate {await tvRequests.CountAsync()}");
            foreach (var request in tvRequests)
            {
                var findResuilt = await api.Find(request.TvDbId.ToString(), ExternalSource.tvdb_id);

                var found = findResuilt.tv_results.FirstOrDefault();

                if (found == null)
                {
                    var seriesFound = findResuilt.tv_season_results.FirstOrDefault();
                    if (seriesFound == null)
                    {
                        Console.WriteLine($"Cannot find TheMovieDb Record for request {request.Title}");
                        continue;
                    }
                    request.ExternalProviderId = seriesFound.show_id;
                    Console.WriteLine($"Cannot find TheMovieDb Record for request, found potential match Title: {request.Title}, Match: {seriesFound.show_id}");
                }
                else
                {
                    request.ExternalProviderId = found.id;
                }
            }
            Console.WriteLine($"Finished Migration");

            var strat = ctx.Database.CreateExecutionStrategy();
            await strat.ExecuteAsync(async () =>
            {
                using (var tran = await ctx.Database.BeginTransactionAsync())
                {
                    ctx.TvRequests.UpdateRange(tvRequests);
                    await ctx.SaveChangesAsync();
                    await tran.CommitAsync();
                }
            });


            var settingsStrat = settingsContext.Database.CreateExecutionStrategy();
            await settingsStrat.ExecuteAsync(async () =>
            {
                using (var tran = await settingsContext.Database.BeginTransactionAsync())
                {
                    ombiSettings.HasMigratedOldTvDbData = true;

                    ombiSettingsContent.Content = JsonConvert.SerializeObject(ombiSettings);
                    settingsContext.Update(ombiSettingsContent);
                    await settingsContext.SaveChangesAsync();
                    await tran.CommitAsync();
                }
            });

            return;

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
