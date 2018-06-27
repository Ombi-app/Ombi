using System;
using System.IO;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.DependencyInjection;
using Ombi.Helpers;
using Ombi.Mapping;
using Ombi.Schedule;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Serilog;

namespace Ombi
{
    public class Startup
    {
        public static StoragePathSingleton StoragePath => StoragePathSingleton.Instance;
        public Startup(IHostingEnvironment env)
        {
            Console.WriteLine(env.ContentRootPath);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            //if (env.IsDevelopment())
            //{
            Serilog.ILogger config;
            if (string.IsNullOrEmpty(StoragePath.StoragePath))
            {
                config = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "Logs", "log-{Date}.txt"))
                    .CreateLogger();
            }
            else
            {
                config = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.RollingFile(Path.Combine(StoragePath.StoragePath, "Logs", "log-{Date}.txt"))
                    .CreateLogger();
            }
            Log.Logger = config;


            //}
            //if (env.IsProduction())
            //{
            //    Log.Logger = new LoggerConfiguration()
            //        .MinimumLevel.Debug()
            //        .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "Logs", "log-{Date}.txt"))
            //        .WriteTo.SQLite("Ombi.db", "Logs", LogEventLevel.Debug)
            //        .CreateLogger();
            //}
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<OmbiContext>();

            services.AddIdentity<OmbiUser, IdentityRole>()
                .AddEntityFrameworkStores<OmbiContext>()
                .AddDefaultTokenProviders()
                .AddUserManager<OmbiUserManager>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.AllowedUserNameCharacters = string.Empty;
            });

            services.AddMemoryCache();

            services.AddJwtAuthentication(Configuration);

            services.AddMvc()
                .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddOmbiMappingProfile();
            services.AddAutoMapper(expression =>
            {
                expression.AddCollectionMappers();
            });

            services.RegisterApplicationDependencies(); // Ioc and EF
            services.AddSwagger();
            services.AddAppSettingsValues(Configuration);

            var i = StoragePathSingleton.Instance;
            if (string.IsNullOrEmpty(i.StoragePath))
            {
                i.StoragePath = string.Empty;
            }
            var sqliteStorage = $"Data Source={Path.Combine(i.StoragePath, "Schedules.db")};";

            services.AddHangfire(x =>
            {
                x.UseSQLiteStorage(sqliteStorage);
                x.UseActivator(new IoCJobActivator(services.BuildServiceProvider()));
                //x.UseConsole();
            });


            // Build the intermediate service provider
            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IMemoryCache cache, IServiceProvider serviceProvider)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            var ctx = serviceProvider.GetService<IOmbiContext>();
            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ConfigFile = "webpack.dev.js"
                });
            }

            var ombiService =
                app.ApplicationServices.GetService<ISettingsService<OmbiSettings>>();
            var settings = ombiService.GetSettings();
            if (settings.ApiKey.IsNullOrEmpty())
            {
                // Generate a API Key
                settings.ApiKey = Guid.NewGuid().ToString("N");
                ombiService.SaveSettings(settings);
            }

            // Check if it's in the startup args
            var appConfig = serviceProvider.GetService<IApplicationConfigRepository>();
            var baseUrl = appConfig.Get(ConfigurationTypes.BaseUrl).Result;
            if (baseUrl != null)
            {
                if (baseUrl.Value.HasValue())
                {
                    settings.BaseUrl = baseUrl.Value;
                    ombiService.SaveSettings(settings);
                }
            }
            if (settings.BaseUrl.HasValue())
            {
                app.UsePathBase(settings.BaseUrl);
            }

            app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = 1, ServerTimeout = TimeSpan.FromDays(1), ShutdownTimeout = TimeSpan.FromDays(1)});
            if (env.IsDevelopment())
            {
                app.UseHangfireDashboard(settings.BaseUrl.HasValue() ? $"{settings.BaseUrl}/hangfire" : "/hangfire",
                    new DashboardOptions
                    {
                        Authorization = new[] {new HangfireAuthorizationFilter()}
                    });
            }

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });
            
            // Setup the scheduler
            var jobSetup = app.ApplicationServices.GetService<IJobSetup>();
            jobSetup.Setup();
            ctx.Seed();
            
            var provider = new FileExtensionContentTypeProvider { Mappings = { [".map"] = "application/octet-stream" } };

            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider,
            });

            app.UseAuthentication();

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<ApiKeyMiddlewear>();

            //app.ApiKeyMiddlewear(app.ApplicationServices);
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });

            ombiService.Dispose();
        }
    }

    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}
