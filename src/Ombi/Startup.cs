using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SQLite;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
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
using Ombi.Core.IdentityResolver;
using Ombi.DependencyInjection;
using Ombi.Helpers;
using Ombi.Mapping;
using Ombi.Schedule;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Serilog;
using Serilog.Events;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using Swashbuckle.AspNetCore.Swagger;

namespace Ombi
{
    public partial class Startup
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

            if (env.IsDevelopment())
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "Logs", "log-{Date}.txt"))
                    .WriteTo.SQLite("Ombi.db", "Logs", LogEventLevel.Debug)
                    .CreateLogger();
            }
            if (env.IsProduction())
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "Logs", "log-{Date}.txt"))
                    .WriteTo.SQLite("Ombi.db", "Logs", LogEventLevel.Debug)
                    .CreateLogger();
            }
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<OmbiContext>(options =>
                options.UseSqlite("Data Source=Ombi.db"));
            
            services.AddIdentity<OmbiUser, IdentityRole>()
                .AddEntityFrameworkStores<OmbiContext>()
                .AddDefaultTokenProviders();

            services.AddIdentityServer()
                .AddTemporarySigningCredential()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(IdentityConfig.GetIdentityResources())
                .AddInMemoryApiResources(IdentityConfig.GetApiResources())
                .AddInMemoryClients(IdentityConfig.GetClients())
                .AddAspNetIdentity<OmbiUser>()
                .Services.AddTransient<IResourceOwnerPasswordValidator, OmbiOwnerPasswordValidator>()
                .AddTransient<IProfileService, OmbiProfileService>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

            services.AddMemoryCache();
            
            services.AddMvc()
                .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            
            services.AddOmbiMappingProfile();
            services.AddAutoMapper(expression =>
            {
                expression.AddCollectionMappers();
            });
            services.RegisterDependencies(); // Ioc and EF
            services.AddSwaggerGen(c =>
            {
                c.DescribeAllEnumsAsStrings();
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Ombi Api",
                    Description = "The API for Ombi, most of these calls require an auth token that you can get from calling POST:\"/connect/token/\" with the body of: \n {\n\"username\":\"YOURUSERNAME\",\n\"password\":\"YOURPASSWORD\"\n} \n" +
                                  "You can then use the returned token in the JWT Token field e.g. \"Bearer Token123xxff\"",
                    Contact = new Contact
                    {
                        Email = "tidusjar@gmail.com",
                        Name = "Jamie Rees",
                        Url = "https://www.ombi.io/"
                    }
                });
                c.CustomSchemaIds(x => x.FullName);
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, "Swagger.xml");
                try
                {
                    c.IncludeXmlComments(xmlPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                c.AddSecurityDefinition("Authentication", new ApiKeyScheme());
                c.OperationFilter<SwaggerOperationFilter>();
                c.DescribeAllParametersInCamelCase();
            });
            


            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPrincipal>(sp => sp.GetService<IHttpContextAccessor>().HttpContext.User);


            //services.Configure<TokenAuthenticationOptions>(Configuration.GetSection("TokenAuthentication"));
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            services.Configure<UserSettings>(Configuration.GetSection("UserSettings"));
            services.Configure<LandingPageBackground>(Configuration.GetSection("LandingPageBackground"));

            services.AddHangfire(x =>
            {

                x.UseMemoryStorage(new MemoryStorageOptions());

                //x.UseSQLiteStorage("Data Source=Ombi.db;");

                x.UseActivator(new IoCJobActivator(services.BuildServiceProvider()));
            });

#if DEBUG
            // Note .AddMiniProfiler() returns a IMiniProfilerBuilder for easy intellisense
            services.AddMiniProfiler();
#endif
            // Make sure you have memory cache available unless you're using another storage provider
            services.AddMemoryCache();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IMemoryCache cache)
        {
            var options = (IOptions<UserSettings>) app.ApplicationServices.GetService(
                typeof(IOptions<UserSettings>));

            var ctx = (IOmbiContext)app.ApplicationServices.GetService(typeof(IOmbiContext));

            // Get the url
            var url = ctx.ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.Url);
            var port = ctx.ApplicationConfigurations.FirstOrDefault(x => x.Type == ConfigurationTypes.Port);

            Console.WriteLine($"Using Url {url.Value}:{port.Value} for Identity Server");
            app.UseIdentity();
            app.UseIdentityServer();
            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
#if !DEBUG
                Authority = $"http://localhost:{port.Value}",
#else
                Authority = $"http://localhost:52038/",
#endif
                ApiName = "api",
                ApiSecret = "secret",

                EnableCaching = true,
                CacheDuration = TimeSpan.FromMinutes(10), // that's the default
                RequireHttpsMetadata = options.Value.UseHttps, // FOR DEV set to false
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                

            });

            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });

                app.UseMiniProfiler(new MiniProfilerOptions
                {
                    // Path to use for profiler URLs
                    RouteBasePath = "~/profiler",

                    // (Optional) Control which SQL formatter to use
                    // (default is no formatter)
                    SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter(),

                    // (Optional) Control storage
                    // (default is 30 minutes in MemoryCacheStorage)
                    Storage = new MemoryCacheStorage(cache, TimeSpan.FromMinutes(60)),
                });
            }
            
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.ShowJsonEditor();
            });


            // Setup the scheduler
            var jobSetup = (IJobSetup)app.ApplicationServices.GetService(typeof(IJobSetup));
            jobSetup.Setup();
            ctx.Seed();

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".map"] = "application/octet-stream";

            app.UseStaticFiles(new StaticFileOptions()
            {
                ContentTypeProvider = provider
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
