using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using Ombi.Api.Discord;
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
using Ombi.Core.Models.Requests;
using Ombi.Core.Notifications;
using Ombi.Core.Rule;
using Ombi.Core.Settings;
using Ombi.Notifications;
using Ombi.Schedule;
using Ombi.Schedule.Jobs;
using Ombi.Settings.Settings;
using Ombi.Store.Context;
using Ombi.Store.Repository;
using Ombi.Notifications.Agents;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Api;
using Ombi.Api.FanartTv;
using Ombi.Api.Pushbullet;
using Ombi.Api.Service;
using Ombi.Api.Slack;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Senders;
using Ombi.Schedule.Ombi;
using Ombi.Store.Repository.Requests;

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
            services.AddTransient<IMovieSender, MovieSender>();
            services.AddTransient<ITvSender, TvSender>();
        }

        public static void RegisterApi(this IServiceCollection services)
        {
            services.AddTransient<IApi, Api.Api>();
            services.AddTransient<IMovieDbApi, Api.TheMovieDb.TheMovieDbApi>();
            services.AddTransient<IPlexApi, PlexApi>();
            services.AddTransient<IEmbyApi, EmbyApi>();
            services.AddTransient<ISonarrApi, SonarrApi>();
            services.AddTransient<ISlackApi, SlackApi>();
            services.AddTransient<ITvMazeApi, TvMazeApi>();
            services.AddTransient<ITraktApi, TraktApi>();
            services.AddTransient<IRadarrApi, RadarrApi>();
            services.AddTransient<IDiscordApi, DiscordApi>();
            services.AddTransient<IPushbulletApi, PushbulletApi>();
            services.AddTransient<IOmbiService, OmbiService>();
            services.AddTransient<IFanartTvApi, FanartTvApi>();
        }

        public static void RegisterStore(this IServiceCollection services)
        {
            services.AddEntityFrameworkSqlite().AddDbContext<OmbiContext>();

            services.AddScoped<IOmbiContext, OmbiContext>();
            services.AddTransient<ISettingsRepository, SettingsJsonRepository>();
            services.AddTransient<ISettingsResolver, SettingsResolver>();
            services.AddTransient<IPlexContentRepository, PlexContentRepository>();
            services.AddTransient<INotificationTemplatesRepository, NotificationTemplatesRepository>();
            
            services.AddTransient<ITvRequestRepository, TvRequestRepository>();
            services.AddTransient<IMovieRequestRepository, MovieRequestRepository>();
            services.AddTransient<IAuditRepository, AuditRepository>();
            services.AddTransient<IApplicationConfigRepository, ApplicationConfigRepository>();
            services.AddTransient(typeof(ISettingsService<>), typeof(SettingsService<>));
        }
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IRequestServiceMain, RequestService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IEmailProvider, GenericEmailProvider>();
            services.AddTransient<INotificationHelper, NotificationHelper>();


            services.AddTransient<IDiscordNotification, DiscordNotification>();
            services.AddTransient<IEmailNotification, EmailNotification>();
            services.AddTransient<IPushbulletNotification, PushbulletNotification>();
            services.AddTransient<ISlackNotification, SlackNotification>();
        }

        public static void RegisterJobs(this IServiceCollection services)
        {
            services.AddTransient<IPlexContentCacher, PlexContentCacher>();
            services.AddTransient<IJobSetup, JobSetup>();
            services.AddTransient<IRadarrCacher, RadarrCacher>();
            services.AddTransient<IOmbiAutomaticUpdater, OmbiAutomaticUpdater>();
        }

        public static void RegisterIdentity(this IServiceCollection services)
        {
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build());
            });
        }
    }
}
