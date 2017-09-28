using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Ombi.Config;
using Ombi.Core.Authentication;
using Ombi.Core.Claims;
using Ombi.Core.Settings;
using Ombi.DependencyInjection;
using Ombi.Helpers;
using Ombi.Mapping;
using Ombi.Models.Identity;
using Ombi.Schedule;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;

namespace Ombi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Console.WriteLine(env.ContentRootPath);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            //if (env.IsDevelopment())
            //{
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "Logs", "log-{Date}.txt"))
                .WriteTo.SQLite("Ombi.db", "Logs", LogEventLevel.Debug)
                .CreateLogger();
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
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -._@+";
            });

            services.AddDataProtection();
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

            services.AddHangfire(x =>
            {
                x.UseMemoryStorage(new MemoryStorageOptions());
                //x.UseSQLiteStorage("Data Source=Ombi.db;");
                x.UseActivator(new IoCJobActivator(services.BuildServiceProvider()));
                x.UseConsole();
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
                    HotModuleReplacement = true
                });
            }

            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });

            // Setup the scheduler
            var jobSetup = (IJobSetup)app.ApplicationServices.GetService(typeof(IJobSetup));
            jobSetup.Setup();
            ctx.Seed();

            var provider = new FileExtensionContentTypeProvider { Mappings = { [".map"] = "application/octet-stream" } };

            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider
            });

            app.UseAuthentication();

            ApiKeyMiddlewear(app, serviceProvider);
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.ShowJsonEditor();
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

        private static void ApiKeyMiddlewear(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments(new PathString("/api")))
                {
                    // Let's check if this is an API Call
                    if (context.Request.Headers["ApiKey"].Any())
                    {
                        // validate the supplied API key
                        // Validate it
                        var headerKey = context.Request.Headers["ApiKey"].FirstOrDefault();
                        var settingsProvider = serviceProvider.GetService<ISettingsService<OmbiSettings>>();
                        var ombiSettings = settingsProvider.GetSettings();
                        var valid = ombiSettings.ApiKey.Equals(headerKey, StringComparison.CurrentCultureIgnoreCase);
                        if (!valid)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            await context.Response.WriteAsync("Invalid API Key");
                        }
                        else
                        {
                            var identity = new GenericIdentity("API");
                            var principal = new GenericPrincipal(identity, new[] {"Admin", "ApiUser"});
                            context.User = principal;
                            await next();
                        }
                    }
                    else
                    {
                        await next();
                    }
                }
                else
                {
                    await next();
                }
            });
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
