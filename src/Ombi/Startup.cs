using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.DependencyInjection;
using Ombi.Extensions;
using Ombi.Helpers;
using Ombi.Hubs;
using Ombi.Mapping;
using Ombi.Schedule;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Serilog;
using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ombi.HealthChecks;

namespace Ombi
{
    public class Startup
    {
        public static StartupSingleton StoragePath => StartupSingleton.Instance;

        public Startup(IWebHostEnvironment env)
        {
            Console.WriteLine(env.ContentRootPath);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<OmbiUser, IdentityRole>()
                .AddEntityFrameworkStores<OmbiContext>()
                .AddRoles<IdentityRole>()
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

            var hcBuilder = services.AddHealthChecks();
            hcBuilder.AddOmbiHealthChecks();
            services.ConfigureDatabases(hcBuilder);
            //services.AddHealthChecksUI(setupSettings: setup =>
            //{
            //    setup.AddHealthCheckEndpoint("Ombi", "/health");
            //});
            services.AddMemoryCache();
            services.AddHttpClient();

            services.AddJwtAuthentication();

            services.AddMvc()
                .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.AddOmbiMappingProfile();
            services.AddAutoMapper(expression => { expression.AddCollectionMappers(); });

            services.RegisterApplicationDependencies(); // Ioc and EF
            services.AddSwagger();
            services.AddAppSettingsValues(Configuration);

            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(isOriginAllowed: _ => true)
                    .AllowCredentials();
            }));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
            services.AddSignalR();
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            serviceProvider.UseQuartz().GetAwaiter().GetResult();

            var ctx = serviceProvider.GetService<OmbiContext>();
            loggerFactory.AddSerilog();
            var ombiService =
                serviceProvider.GetService<ISettingsService<OmbiSettings>>();
            var settings = ombiService.GetSettings();

            var sharedOptions = new SharedOptions();
            if (settings.BaseUrl.HasValue())
            {
                if (settings.BaseUrl.EndsWith("/"))
                {
                    settings.BaseUrl = settings.BaseUrl.Remove(settings.BaseUrl.Length - 1, 1);
                }
                sharedOptions.RequestPath = settings.BaseUrl;
            }

            app.UseSpaStaticFiles(new StaticFileOptions(sharedOptions));


            if (settings.ApiKey.IsNullOrEmpty())
            {
                // Generate a API Key
                settings.ApiKey = Guid.NewGuid().ToString("N");
                settings.CollectAnalyticData = true; // Since this is a first setup, enable analytical data collection
                settings.Set = true;
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

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<ApiKeyMiddlewear>();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/hubs/notification");
                if (!settings.DisableHealthChecks)
                {
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        //ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    //endpoints.MapHealthChecksUI(opts =>
                    //{
                    //    opts.AddCustomStylesheet("HealthCheck.css");
                    //});
                }
            });

            app.UseSpa(spa =>
            {
#if DEBUG
                spa.Options.SourcePath = "ClientApp";
                spa.UseProxyToSpaDevelopmentServer("http://localhost:3578");
#endif
            });

        }
    }
}
