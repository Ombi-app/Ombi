using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Ombi.Api.Emby;
using Ombi.Api.Plex;
using Ombi.Api.Radarr;
using Ombi.Api.Sonarr;
using Ombi.Api.TheMovieDb;
using Ombi.Api.Trakt;
using Ombi.Api.TvMaze;
using Ombi.Core;
using Ombi.Core.Engine;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.IdentityResolver;
using Ombi.Core.Models.Requests;
using Ombi.Core.Requests.Models;
using Ombi.Core.Rule;
using Ombi.Core.Settings;
using Ombi.Notifications;
using Ombi.Schedule;
using Ombi.Schedule.Jobs;
using Ombi.Settings.Settings;
using Ombi.Store.Context;
using Ombi.Store.Repository;
using Ombi.Core.Rules;

namespace Ombi.DependencyInjection
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class IocExtensions
    {
        public static void RegisterDependencies(this IServiceCollection services)
        {
            services.RegisterEngines();
            services.RegisterApi();
            services.RegisterServices();
            services.RegisterStore();
            services.RegisterIdentity();
            services.RegisterJobs();
        }

        public static void RegisterEngines(this IServiceCollection services)
        {
            services.AddTransient<IMovieEngine, MovieSearchEngine>();
            services.AddTransient<IMovieRequestEngine, MovieRequestEngine>();
            services.AddTransient<ITvRequestEngine, TvRequestEngine>();
            services.AddTransient<ITvSearchEngine, TvSearchEngine>();
            services.AddSingleton<IRuleEvaluator, RuleEvaluator>();
        }

        public static void RegisterApi(this IServiceCollection services)
        {
            services.AddTransient<IMovieDbApi, Api.TheMovieDb.TheMovieDbApi>();
            services.AddTransient<IPlexApi, PlexApi>();
            services.AddTransient<IEmbyApi, EmbyApi>();
            services.AddTransient<ISonarrApi, SonarrApi>();
            services.AddTransient<ITvMazeApi, TvMazeApi>();
            services.AddTransient<ITraktApi, TraktApi>();
            services.AddTransient<IRadarrApi, RadarrApi>();
        }

        public static void RegisterStore(this IServiceCollection services)
        {
            services.AddEntityFrameworkSqlite().AddDbContext<OmbiContext>();

            services.AddScoped<IOmbiContext, OmbiContext>();
            services.AddTransient<IRequestRepository, RequestJsonRepository>();
            services.AddTransient<ISettingsRepository, SettingsJsonRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ISettingsResolver, SettingsResolver>();
            services.AddTransient<IPlexContentRepository, PlexContentRepository>();
            services.AddTransient(typeof(ISettingsService<>), typeof(SettingsServiceV2<>));
        }
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IRequestServiceMain, RequestService>();
            services.AddTransient(typeof(IRequestService<>), typeof(JsonRequestService<>));
            services.AddSingleton<INotificationService, NotificationService>();
        }

        public static void RegisterJobs(this IServiceCollection services)
        {
            services.AddTransient<IPlexContentCacher, PlexContentCacher>();
            services.AddTransient<IJobSetup, JobSetup>();
        }

        public static void RegisterIdentity(this IServiceCollection services)
        {
            services.AddTransient<IUserIdentityManager, UserIdentityManager>();
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });
        }
    }
}
