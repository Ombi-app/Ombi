using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
using Ombi.Api.CouchPotato;
using Ombi.Api.FanartTv;
using Ombi.Api.Mattermost;
using Ombi.Api.Pushbullet;
using Ombi.Api.Pushover;
using Ombi.Api.Service;
using Ombi.Api.Slack;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Senders;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Jobs.Sonarr;
using Ombi.Schedule.Ombi;
using Ombi.Store.Repository.Requests;
using PlexContentCacher = Ombi.Schedule.Jobs.Plex.PlexContentCacher;

namespace Ombi.DependencyInjection
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class IocExtensions
    {
        public static void RegisterApplicationDependencies(this IServiceCollection services)
        {
            services.RegisterEngines();
            services.RegisterApi();
            services.RegisterServices();
            services.RegisterStore();
            services.RegisterJobs();
            services.RegisterHttp();
        }

        public static void RegisterEngines(this IServiceCollection services)
        {
            services.AddTransient<IMovieEngine, MovieSearchEngine>();
            services.AddTransient<IMovieRequestEngine, MovieRequestEngine>();
            services.AddTransient<ITvRequestEngine, TvRequestEngine>();
            services.AddTransient<ITvSearchEngine, TvSearchEngine>();
            services.AddTransient<IRuleEvaluator, RuleEvaluator>();
            services.AddTransient<IMovieSender, MovieSender>();
            services.AddTransient<ITvSender, TvSender>();
        }
        public static void RegisterHttp(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPrincipal>(sp => sp.GetService<IHttpContextAccessor>().HttpContext.User);
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
            services.AddTransient<IPushoverApi, PushoverApi>();
            services.AddTransient<IMattermostApi, MattermostApi>();
            services.AddTransient<ICouchPotatoApi, CouchPotatoApi>();
        }

        public static void RegisterStore(this IServiceCollection services)
        {
            services.AddEntityFrameworkSqlite().AddDbContext<OmbiContext>();

            services.AddScoped<IOmbiContext, OmbiContext>(); // https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6
            services.AddTransient<ISettingsRepository, SettingsJsonRepository>();
            services.AddTransient<ISettingsResolver, SettingsResolver>();
            services.AddTransient<IPlexContentRepository, PlexContentRepository>();
            services.AddTransient<IEmbyContentRepository, EmbyContentRepository>();
            services.AddTransient<INotificationTemplatesRepository, NotificationTemplatesRepository>();
            
            services.AddTransient<ITvRequestRepository, TvRequestRepository>();
            services.AddTransient<IMovieRequestRepository, MovieRequestRepository>();
            services.AddTransient<IAuditRepository, AuditRepository>();
            services.AddTransient<IApplicationConfigRepository, ApplicationConfigRepository>();
            services.AddTransient<ITokenRepository, TokenRepository>();
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
            services.AddTransient<ISlackNotification, SlackNotification>();
            services.AddTransient<IMattermostNotification, MattermostNotification>();
            services.AddTransient<IPushoverNotification, PushoverNotification>();

            services.AddTransient<IBackgroundJobClient, BackgroundJobClient>();
        }

        public static void RegisterJobs(this IServiceCollection services)
        {
            services.AddTransient<IPlexContentCacher, PlexContentCacher>();
            services.AddTransient<IEmbyContentCacher, EmbyContentCacher>();
            services.AddTransient<IEmbyEpisodeCacher, EmbyEpisodeCacher>();
            services.AddTransient<IEmbyAvaliabilityChecker, EmbyAvaliabilityChecker>();
            services.AddTransient<IPlexEpisodeCacher, PlexEpisodeCacher>();
            services.AddTransient<IPlexAvailabilityChecker, PlexAvailabilityChecker>();
            services.AddTransient<IJobSetup, JobSetup>();
            services.AddTransient<IRadarrCacher, RadarrCacher>();
            services.AddTransient<ISonarrCacher, SonarrCacher>();
            services.AddTransient<IOmbiAutomaticUpdater, OmbiAutomaticUpdater>();
            services.AddTransient<IPlexUserImporter, PlexUserImporter>();
            services.AddTransient<IEmbyUserImporter, EmbyUserImporter>();
            services.AddTransient<IWelcomeEmail, WelcomeEmail>();
        }
    }
}
