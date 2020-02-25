using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ombi.Core.Authentication;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Settings.Settings.Models;

namespace Ombi
{
    public class ApiKeyMiddlewear
    {
        public ApiKeyMiddlewear(RequestDelegate next)
        {
            _next = next;
        }
        private readonly RequestDelegate _next;

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(new PathString("/api")))
            {
                //Let's check if this is an API Call
                if (context.Request.Headers.Keys.Contains("ApiKey", StringComparer.InvariantCultureIgnoreCase))
                {
                    // validate the supplied API key
                    // Validate it
                    var headerKey = context.Request.Headers["ApiKey"].FirstOrDefault();
                    await ValidateApiKey(context, _next, headerKey);
                }
                else if (context.Request.Query.ContainsKey("apikey"))
                {
                    if (context.Request.Query.TryGetValue("apikey", out var queryKey))
                    {
                        await ValidateApiKey(context, _next, queryKey);
                    }
                }
                // User access token used by the mobile app
                else if (context.Request.Headers["UserAccessToken"].Any())
                {
                    var headerKey = context.Request.Headers["UserAccessToken"].FirstOrDefault();
                    await ValidateUserAccessToken(context, _next, headerKey);
                }
                else
                {
                    await _next.Invoke(context);
                }
            }
            else
            {
                await _next.Invoke(context);
            }
        }

        private async Task ValidateUserAccessToken(HttpContext context, RequestDelegate next, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                await context.Response.WriteAsync("Invalid User Access Token");
                return;
            }

            var um = context.RequestServices.GetService<OmbiUserManager>();
            var user = await um.Users.FirstOrDefaultAsync(x => x.UserAccessToken == key);
            if (user == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid User Access Token");
            }
            else
            {

                var identity = new GenericIdentity(user.UserName);
                var roles = await um.GetRolesAsync(user);
                var principal = new GenericPrincipal(identity, roles.ToArray());
                context.User = principal;
                await next.Invoke(context);
            }
        }

        private async Task ValidateApiKey(HttpContext context, RequestDelegate next, string key)
        {
            var repo = context.RequestServices.GetService<ISettingsService<OmbiSettings>>();
            var ombiSettings = await repo.GetSettingsAsync();
            var valid = ombiSettings.ApiKey.Equals(key, StringComparison.CurrentCultureIgnoreCase);
            if (!valid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Invalid API Key");
            }
            else
            {
                // Check if we have a UserName header if so we can impersonate that user
                if (context.Request.Headers.Keys.Contains("UserName", StringComparer.InvariantCultureIgnoreCase))
                {
                    var username = context.Request.Headers["UserName"].FirstOrDefault();
                    if (username.IsNullOrEmpty())
                    {
                        UseApiUser(context);
                    } 
                    else
                    {
                        username = username.ToUpper();
                    }
                    
                    var um = context.RequestServices.GetService<OmbiUserManager>();

                    var user = await um.Users.FirstOrDefaultAsync(x =>
                        x.NormalizedUserName == username);
                    if (user == null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await context.Response.WriteAsync("Invalid User");
                        await next.Invoke(context);
                    }
                    var roles = await um.GetRolesAsync(user);
                    var identity = new GenericIdentity(user.UserName);
                    var principal = new GenericPrincipal(identity, roles.ToArray());
                    context.User = principal;
                }
                else
                {
                    UseApiUser(context);
                }

                await next.Invoke(context);
            }
        }

        private void UseApiUser(HttpContext context)
        {
            var identity = new GenericIdentity("API");
            var principal = new GenericPrincipal(identity, new[] { "Admin", "ApiUser" });
            context.User = principal;
        }
    }
}