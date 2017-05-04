using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ombi.Api.Emby;
using Ombi.Api.Plex;
using Ombi.Api.Sonarr;
using Ombi.Api.TheMovieDb;
using Ombi.Api.TvMaze;
using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models.Requests;
using Ombi.Core.Requests.Models;
using Ombi.Core.Settings;
using Ombi.Notifications;
using Ombi.Schedule;
using Ombi.Schedule.Jobs;
using Ombi.Settings.Settings;
using Ombi.Store.Context;
using Ombi.Store.Repository;
using Ombi.TheMovieDbApi;

namespace Ombi.DependencyInjection
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class IocExtensions
    {
        public static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            services.RegisterEngines();
            services.RegisterApi();
            services.RegisterServices();
            services.RegisterStore();
            services.RegisterIdentity();
            services.RegisterJobs();

            return services;
        }

        public static IServiceCollection RegisterEngines(this IServiceCollection services)
        {
            services.AddTransient<IMovieEngine, MovieSearchEngine>();
            services.AddTransient<IMovieRequestEngine, MovieRequestEngine>();
            services.AddTransient<ITvRequestEngine, TvRequestEngine>();
            services.AddTransient<ITvSearchEngine, TvSearchEngine>();
            return services;
        }

        public static IServiceCollection RegisterApi(this IServiceCollection services)
        {
            services.AddTransient<IMovieDbApi, Api.TheMovieDb.TheMovieDbApi>();
            services.AddTransient<IPlexApi, PlexApi>();
            services.AddTransient<IEmbyApi, EmbyApi>();
            services.AddTransient<ISonarrApi, SonarrApi>();
            services.AddTransient<ITvMazeApi, TvMazeApi>();
            return services;
        }

        public static IServiceCollection RegisterStore(this IServiceCollection services)
        {
            services.AddEntityFrameworkSqlite().AddDbContext<OmbiContext>();

            services.AddTransient<IOmbiContext, OmbiContext>();
            services.AddTransient<IRequestRepository, RequestJsonRepository>();
            services.AddTransient<ISettingsRepository, SettingsJsonRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ISettingsResolver, SettingsResolver>();
            services.AddTransient(typeof(ISettingsService<>), typeof(SettingsServiceV2<>));
            return services;
        }
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {

            services.AddTransient<IRequestServiceMain, RequestService>();
            services.AddTransient(typeof(IRequestService<>), typeof(JsonRequestService<>));
            services.AddSingleton<INotificationService, NotificationService>();

            return services;
        }

        public static IServiceCollection RegisterJobs(this IServiceCollection services)
        {
            services.AddTransient<IPlexContentCacher, PlexContentCacher>();
            services.AddTransient<IJobSetup, JobSetup>();

            return services;
        }

        public static IServiceCollection RegisterIdentity(this IServiceCollection services)
        {

            services.AddTransient<IUserIdentityManager, UserIdentityManager>();
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });
            return services;
        }
    }
}
