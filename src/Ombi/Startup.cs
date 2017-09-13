using System;
using System.IO;
using System.Security.Principal;
using System.Text;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using Ombi.Core.Claims;
using Ombi.DependencyInjection;
using Ombi.Helpers;
using Ombi.Mapping;
using Ombi.Models.Identity;
using Ombi.Schedule;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Serilog;
using Serilog.Events;
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
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityDefinition("Authentication", new ApiKeyScheme());
                c.OperationFilter<SwaggerOperationFilter>();
                c.DescribeAllParametersInCamelCase();
            });
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPrincipal>(sp => sp.GetService<IHttpContextAccessor>().HttpContext.User);

            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            services.Configure<UserSettings>(Configuration.GetSection("UserSettings"));
            services.Configure<TokenAuthentication>(Configuration.GetSection("TokenAuthentication"));
            services.Configure<LandingPageBackground>(Configuration.GetSection("LandingPageBackground"));

            services.AddHangfire(x =>
            {
                x.UseMemoryStorage(new MemoryStorageOptions());
                //x.UseSQLiteStorage("Data Source=Ombi.db;");
                x.UseActivator(new IoCJobActivator(services.BuildServiceProvider()));
            });


            var tokenOptions = Configuration.GetSection("TokenAuthentication");

            

            var tokenValidationParameters = new TokenValidationParameters
            {

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.GetValue("SecretKey", string.Empty))),

                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidAudience = "Ombi",
                ValidIssuer = "Ombi",
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication().AddJwtBearer(x =>
            {
                x.Audience = "Ombi";
                x.TokenValidationParameters = tokenValidationParameters;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IMemoryCache cache)
        {

            var ctx = (IOmbiContext)app.ApplicationServices.GetService(typeof(IOmbiContext));



            app.UseAuthentication();

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
                Authorization = new [] { new HangfireAuthorizationFilter() }
            });

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
    
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return httpContext.User.IsInRole(OmbiRoles.Admin);
        }
    }
}
