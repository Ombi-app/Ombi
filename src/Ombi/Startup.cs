using System;
using System.IO;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
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
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Settings;
using Ombi.DependencyInjection;
using Ombi.Extensions;
using Ombi.Helpers;
using Ombi.Mapping;
using Ombi.Schedule;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Serilog;
using Ombi.Api.Telegram;
using ILogger = Serilog.ILogger;

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

            ILogger config = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(StoragePath.StoragePath.IsNullOrEmpty() ? env.ContentRootPath : StoragePath.StoragePath, "Logs", "log-{Date}.txt"))
                .CreateLogger();

            Log.Logger = config;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
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

            services.ConfigureDatabases();
            services.AddHealthChecks();
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

            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            services.AddTelegramCallbacks(
                provider => provider.GetRequiredService<IMovieRequestEngine>(),
                async (engine, id) => await engine.ApproveMovieById(id),
                async (engine, id) => await engine.DenyMovieById(id, string.Empty),

                provider => provider.GetRequiredService<ITvRequestEngine>(),
                async (engine, id) => await engine.ApproveChildRequest(id),
                async (engine, id) => await engine.DenyChildRequest(id, string.Empty),

                result => result.Message ?? result.ErrorMessage
            );

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

            app.UseQuartz().GetAwaiter().GetResult();

            var ctx = serviceProvider.GetService<OmbiContext>();
            loggerFactory.AddSerilog();

            app.UseHealthChecks("/health");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ConfigFile = "webpack.config.ts",
                    
                    //EnvParam = new
                    //{
                    //    aot = true // can't use AOT with HMR currently https://github.com/angular/angular-cli/issues/6347
                    //}
                });
            }

            var ombiService =
                app.ApplicationServices.GetService<ISettingsService<OmbiSettings>>();
            var settings = ombiService.GetSettings();
            if (settings.ApiKey.IsNullOrEmpty())
            {
                // Generate a API Key
                settings.ApiKey = Guid.NewGuid().ToString("N");
                settings.CollectAnalyticData = true; // Since this is a first setup, enable analytical data collection
                settings.Set = true;
                ombiService.SaveSettings(settings);
            }

            if (!settings.Set)
            {
                settings.Set = true;
                settings.CollectAnalyticData = true;
                ombiService.SaveSettings(settings);
            }

            // Check if it's in the startup args
            var appConfig = serviceProvider.GetService<IApplicationConfigRepository>();
            var baseUrl = appConfig.Get(ConfigurationTypes.BaseUrl);
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

          // Setup the scheduler
            //var jobSetup = app.ApplicationServices.GetService<IJobSetup>();
            //jobSetup.Setup();
            ctx.Seed();
            var settingsctx = serviceProvider.GetService<SettingsContext>();
            settingsctx.Seed();

            var provider = new FileExtensionContentTypeProvider { Mappings = { [".map"] = "application/octet-stream" } };

            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider,
            });

            app.UseAuthentication();

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<ApiKeyMiddlewear>();

            app.UseCors("MyPolicy");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                if (settings.BaseUrl.HasValue())
                {
                    c.SwaggerEndpoint($"{settings.BaseUrl}/swagger/v1/swagger.json", "My API V1");
                }
                else
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                }
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
        }
    }
}
