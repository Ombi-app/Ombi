using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ombi.Config;
using Ombi.Helpers;
using Ombi.Models.Identity;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ombi
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();

            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "ApiKey",
                In = "header",
                Type = "apiKey",

            });
        }
    }
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
                    Contact = new Contact
                    {
                        Name = "Jamie Rees",
                        Url = "https://www.ombi.io/"
                    }
                });
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    //{"Bearer", new string[] { }},
                    {"ApiKey", new string[] { }},
                };

                //c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                //{
                //    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                //    Name = "Authorization",
                //    In = "header",
                //    Type = "apiKey"
                //});

                c.AddSecurityDefinition("ApiKey", new ApiKeyScheme
                {
                    Description = "API Key provided by Ombi. Example: \"ApiKey: {token}\"",
                    Name = "ApiKey",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);
                c.CustomSchemaIds(x => x.FullName);
                c.OperationFilter<AddRequiredHeaderParameter>();
                var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var xmlPath = Path.Combine(basePath, "Swagger.xml");
                try
                {
                    c.IncludeXmlComments(xmlPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }


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
            services.Configure<DemoLists>(configuration.GetSection("Demo"));
            var enabledDemo = Convert.ToBoolean(configuration.GetSection("Demo:Enabled").Value);
            DemoSingleton.Instance.Demo = enabledDemo;
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