using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ombi.Config;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Models.Identity;
using Ombi.Settings.Settings.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace Ombi
{
    public static class StartupExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.DescribeAllEnumsAsStrings();
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Ombi Api",
                    Description = "The API for Ombi, most of these calls require an auth token that you can get from calling POST:\"/api/v1/token\" with the body of: \n {\n\"username\":\"YOURUSERNAME\",\n\"password\":\"YOURPASSWORD\"\n} \n" +
                                  "You can then use the returned token in the JWT Token field e.g. \"Bearer Token123xxff\"",
                    Contact = new Contact
                    {
                        Email = "tidusjar@gmail.com",
                        Name = "Jamie Rees",
                        Url = "https://www.ombi.io/"
                    }
                });
                c.CustomSchemaIds(x => x.FullName);
                var basePath = Directory.GetCurrentDirectory();
                var xmlPath = Path.Combine(basePath, "Swagger.xml");
                try
                {
                    c.IncludeXmlComments(xmlPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                c.AddSecurityDefinition("Bearer", new JwtBearer
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey",
                });

                c.OperationFilter<SwaggerOperationFilter>();
                c.DescribeAllParametersInCamelCase();
            });
        }

        public class JwtBearer : SecurityScheme
        {
            public string Name { get; set; }

            public string In { get; set; }

            public JwtBearer()
            {
                this.Type = "bearer";
            }
        }

        public static void AddAppSettingsValues(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
            services.Configure<UserSettings>(configuration.GetSection("UserSettings"));
            services.Configure<TokenAuthentication>(configuration.GetSection("TokenAuthentication"));
            services.Configure<LandingPageBackground>(configuration.GetSection("LandingPageBackground"));
        }

        public static void AddJwtAuthentication(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var tokenOptions = configuration.GetSection("TokenAuthentication");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.GetValue("SecretKey", string.Empty))),

                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidAudience = "Ombi",
                ValidIssuer = "Ombi",
                ClockSkew = TimeSpan.Zero,
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.Audience = "Ombi";
                x.TokenValidationParameters = tokenValidationParameters;
            });
        }
    }
}