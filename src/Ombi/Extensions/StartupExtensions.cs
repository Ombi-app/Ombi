using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ombi.Config;
using Ombi.Core.Authentication;
using Ombi.Helpers;
using Ombi.Models.Identity;

namespace Ombi
{
    public static class StartupExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Ombi Api V1",
                    Contact = new OpenApiContact
                    {
                        Name = "Jamie Rees",
                        Url = new Uri("https://www.ombi.io/")
                    }
                });

                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "API Key provided by Ombi. Example: \"ApiKey: {token}\"",
                    Name = "ApiKey",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.CustomSchemaIds(x => x.FullName);

                try
                {
                    string basePath = Path.GetDirectoryName(AppContext.BaseDirectory);
                    string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                    string xmlPath = Path.Combine(basePath ?? string.Empty, $"{assemblyName}.xml");
                    if (File.Exists(xmlPath))
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                    else
                    {
                        Console.WriteLine($"Swagger failed to find documentation file at '{xmlPath}'.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                c.DescribeAllParametersInCamelCase();
            });
        }

        public static void AddAppSettingsValues(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
            services.Configure<TokenAuthentication>(configuration.GetSection("TokenAuthentication"));
            services.Configure<LandingPageBackground>(configuration.GetSection("LandingPageBackground"));
            services.Configure<DemoLists>(configuration.GetSection("Demo"));
            var enabledDemo = Convert.ToBoolean(configuration.GetSection("Demo:Enabled").Value);
            DemoSingleton.Instance.Demo = enabledDemo;
        }

        public static void AddJwtAuthentication(this IServiceCollection services)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(StartupSingleton.Instance.SecurityKey)),
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidAudience = "Ombi",
                ValidIssuer = "Ombi",
                ClockSkew = TimeSpan.Zero,
            };

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.Audience = "Ombi";
                    x.TokenValidationParameters = tokenValidationParameters;
                    x.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/hubs")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = async context =>
                        {
                            var userid = context.Principal?.Claims?.Where(x => x.Type.Equals("id", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()?.Value ?? default;
                            var cache = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
                            var user = await cache.GetOrAdd(userid + "token", async () =>
                            {
                                var um = context.HttpContext.RequestServices.GetRequiredService<OmbiUserManager>();
                                return await um.FindByIdAsync(userid);
                            }, DateTime.UtcNow.AddMinutes(10));
                            if (user == null)
                            {
                                context.Fail("invaild token");
                            }

                        }
                    };
                });
        }
    }
}