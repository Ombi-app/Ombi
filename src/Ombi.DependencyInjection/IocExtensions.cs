using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Ombi.Api.Discord;
using Ombi.Api.Emby;
using Ombi.Api.Jellyfin;
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
using Ombi.Store.Repository;
using Ombi.Notifications.Agents;
using Ombi.Schedule.Jobs.Radarr;
using Ombi.Api;
using Ombi.Api.CouchPotato;
using Ombi.Api.DogNzb;
using Ombi.Api.FanartTv;
using Ombi.Api.Github;
using Ombi.Api.Gotify;
using Ombi.Api.GroupMe;
using Ombi.Api.Webhook;
using Ombi.Api.Lidarr;
using Ombi.Api.Mattermost;
using Ombi.Api.Notifications;
using Ombi.Api.Pushbullet;
using Ombi.Api.Pushover;
using Ombi.Api.Service;
using Ombi.Api.SickRage;
using Ombi.Api.Slack;
using Ombi.Core.Rule.Interfaces;
using Ombi.Core.Senders;
using Ombi.Helpers;
using Ombi.Schedule.Jobs.Couchpotato;
using Ombi.Schedule.Jobs.Emby;
using Ombi.Schedule.Jobs.Jellyfin;
using Ombi.Schedule.Jobs.Ombi;
using Ombi.Schedule.Jobs.Plex;
using Ombi.Schedule.Jobs.Sonarr;
using Ombi.Store.Repository.Requests;
using Ombi.Updater;
using Ombi.Api.Telegram;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Demo;
using Ombi.Core.Engine.V2;
using Ombi.Core.Processor;
using Ombi.Schedule.Jobs.Lidarr;
using Ombi.Schedule.Jobs.Plex.Interfaces;
using Ombi.Schedule.Jobs.SickRage;
using Ombi.Schedule.Processor;
using Quartz.Spi;
using Ombi.Api.MusicBrainz;
using Ombi.Api.Twilio;
using Ombi.Api.CloudService;
using Ombi.Api.RottenTomatoes;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Ombi.Core.Services;

namespace Ombi.DependencyInjection
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class IocExtensions
    {
        public static void RegisterApplicationDependencies(this IServiceCollection services)
        {
            services.RegisterEngines();
            services.RegisterEnginesV2();
            services.RegisterApi();
            services.RegisterServices();
            services.RegisterStore();
            services.RegisterJobs();
            services.RegisterHttp();
        }

        public static void RegisterEngines(this IServiceCollection services)
        {
            services.AddScoped<IMovieEngine, MovieSearchEngine>();
            services.AddScoped<IMovieRequestEngine, MovieRequestEngine>();
            services.AddScoped<ITvRequestEngine, TvRequestEngine>();
            services.AddScoped<ITvSearchEngine, TvSearchEngine>();
            services.AddScoped<IRuleEvaluator, RuleEvaluator>();
            services.AddScoped<IUserStatsEngine, UserStatsEngine>();
            services.AddScoped<IMovieSender, MovieSender>();
            services.AddScoped<IRecentlyAddedEngine, RecentlyAddedEngine>();
            services.AddScoped<IMusicSearchEngine, MusicSearchEngine>();
            services.AddScoped<IMusicRequestEngine, MusicRequestEngine>();
            services.AddScoped<ITvSender, TvSender>();
            services.AddScoped<IMusicSender, MusicSender>();
            services.AddScoped<IMassEmailSender, MassEmailSender>();
            services.AddScoped<IPlexOAuthManager, PlexOAuthManager>();
            services.AddScoped<IVoteEngine, VoteEngine>();
            services.AddScoped<IDemoMovieSearchEngine, DemoMovieSearchEngine>();
            services.AddScoped<IDemoTvSearchEngine, DemoTvSearchEngine>();
            services.AddScoped<IUserDeletionEngine, UserDeletionEngine>();
        }

        public static void RegisterEnginesV2(this IServiceCollection services)
        {
            services.AddScoped<IMultiSearchEngine, MultiSearchEngine>();
            services.AddScoped<IMovieEngineV2, MovieSearchEngineV2>();
            services.AddScoped<ITVSearchEngineV2, TvSearchEngineV2>();
            services.AddScoped<ICalendarEngine, CalendarEngine>();
            services.AddScoped<IMusicSearchEngineV2, MusicSearchEngineV2>();
            services.AddScoped<IIssuesEngine, IssuesEngine>();
        }

        public static void RegisterHttp(this IServiceCollection services)
        {
            var runtimeVersion = AssemblyHelper.GetRuntimeVersion();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPrincipal>(sp => sp.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddHttpClient("OmbiClient", client =>
            {                
                client.DefaultRequestHeaders.Add("User-Agent", $"Ombi/{runtimeVersion} (https://ombi.io/)");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true;

                return httpClientHandler;
            });
        }

        public static void RegisterApi(this IServiceCollection services)
        {
            services.AddScoped<IApi, Api.Api>(s => new Api.Api(s.GetRequiredService<ILogger<Api.Api>>(), s.GetRequiredService<IHttpClientFactory>().CreateClient("OmbiClient")));
            services.AddScoped<IMovieDbApi, Api.TheMovieDb.TheMovieDbApi>();
            services.AddScoped<IPlexApi, PlexApi>();
            services.AddScoped<IEmbyApi, EmbyApi>();
            services.AddScoped<IJellyfinApi, JellyfinApi>();
            services.AddScoped<ISonarrApi, SonarrApi>();
            services.AddScoped<ISonarrV3Api, SonarrV3Api>();
            services.AddScoped<ISlackApi, SlackApi>();
            services.AddScoped<ITvMazeApi, TvMazeApi>();
            services.AddScoped<ITraktApi, TraktApi>();
            services.AddScoped<IRadarrApi, RadarrApi>();
            services.AddScoped<IRadarrV3Api, RadarrV3Api>();
            services.AddScoped<IDiscordApi, DiscordApi>();
            services.AddScoped<IPushbulletApi, PushbulletApi>();
            services.AddScoped<IOmbiService, OmbiService>();
            services.AddScoped<IFanartTvApi, FanartTvApi>();
            services.AddScoped<IPushoverApi, PushoverApi>();
            services.AddScoped<IGotifyApi, GotifyApi>();
            services.AddScoped<IWebhookApi, WebhookApi>();
            services.AddScoped<IMattermostApi, MattermostApi>();
            services.AddScoped<ICouchPotatoApi, CouchPotatoApi>();
            services.AddScoped<IDogNzbApi, DogNzbApi>();
            services.AddScoped<ITelegramApi, TelegramApi>();
            services.AddScoped<IGithubApi, GithubApi>();
            services.AddScoped<ISickRageApi, SickRageApi>();
            services.AddScoped<IAppVeyorApi, AppVeyorApi>();
            services.AddScoped<IOneSignalApi, OneSignalApi>();
            services.AddScoped<ILidarrApi, LidarrApi>();
            services.AddScoped<IGroupMeApi, GroupMeApi>();
            services.AddScoped<IMusicBrainzApi, MusicBrainzApi>();
            services.AddScoped<IWhatsAppApi, WhatsAppApi>();
            services.AddScoped<ICloudMobileNotification, CloudMobileNotification>();
            services.AddScoped<IEmbyApiFactory, EmbyApiFactory>();
            services.AddScoped<IJellyfinApiFactory, JellyfinApiFactory>();
            services.AddScoped<IRottenTomatoesApi, RottenTomatoesApi>();
        }

        public static void RegisterStore(this IServiceCollection services) { 
            //services.AddDbContext<OmbiContext>();
            //services.AddDbContext<SettingsContext>();
            //services.AddDbContext<ExternalContext>();
            
            //services.AddScoped<OmbiContext, OmbiContext>(); // https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6
            //services.AddScoped<ISettingsContext, SettingsContext>(); // https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6
            //services.AddScoped<ExternalContext, ExternalContext>(); // https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6
            services.AddScoped<ISettingsRepository, SettingsJsonRepository>();
            services.AddScoped<ISettingsResolver, SettingsResolver>();
            services.AddScoped<IPlexContentRepository, PlexServerContentRepository>();
            services.AddScoped<IEmbyContentRepository, EmbyContentRepository>();
            services.AddScoped<IJellyfinContentRepository, JellyfinContentRepository>();
            services.AddScoped<INotificationTemplatesRepository, NotificationTemplatesRepository>();
            
            services.AddScoped<ITvRequestRepository, TvRequestRepository>();
            services.AddScoped<IMovieRequestRepository, MovieRequestRepository>();
            services.AddScoped<IMusicRequestRepository, MusicRequestRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IApplicationConfigRepository, ApplicationConfigRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped(typeof(ISettingsService<>), typeof(SettingsService<>));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IExternalRepository<>), typeof(ExternalRepository<>));
        }
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IRequestServiceMain, RequestService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailProvider, GenericEmailProvider>();
            services.AddScoped<INotificationHelper, NotificationHelper>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<IMediaCacheService, MediaCacheService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddSingleton<IRequestLimitService, RequestLimitService>();

            services.AddScoped<IDiscordNotification, DiscordNotification>();
            services.AddScoped<IEmailNotification, EmailNotification>();
            services.AddScoped<IPushbulletNotification, PushbulletNotification>();
            services.AddScoped<ISlackNotification, SlackNotification>();
            services.AddScoped<ISlackNotification, SlackNotification>();
            services.AddScoped<IMattermostNotification, MattermostNotification>();
            services.AddScoped<IPushoverNotification, PushoverNotification>();
            services.AddScoped<IGotifyNotification, GotifyNotification>();
            services.AddScoped<IWebhookNotification, WebhookNotification>();
            services.AddScoped<ITelegramNotification, TelegramNotification>();
            services.AddScoped<ILegacyMobileNotification, LegacyMobileNotification>();
            services.AddScoped<IChangeLogProcessor, ChangeLogProcessor>();
        }

        public static void RegisterJobs(this IServiceCollection services)
        {
            services.AddSingleton<QuartzJobRunner>();
            services.AddSingleton<IJobFactory, IoCJobFactory>();

            services.AddSingleton<IPlexContentSync, PlexContentSync>();
            services.AddSingleton<IEmbyContentSync, EmbyContentSync>();
            services.AddSingleton<IEmbyEpisodeSync, EmbyEpisodeSync>();
            services.AddSingleton<IEmbyAvaliabilityChecker, EmbyAvaliabilityChecker>();
            services.AddSingleton<IJellyfinContentSync, JellyfinContentSync>();
            services.AddSingleton<IJellyfinEpisodeSync, JellyfinEpisodeSync>();
            services.AddSingleton<IJellyfinAvaliabilityChecker, JellyfinAvaliabilityChecker>();
            services.AddSingleton<IPlexEpisodeSync, PlexEpisodeSync>();
            services.AddSingleton<IPlexAvailabilityChecker, PlexAvailabilityChecker>();
            services.AddSingleton<IRadarrSync, RadarrSync>();
            services.AddSingleton<ISonarrSync, SonarrSync>();
            services.AddSingleton<IOmbiAutomaticUpdater, OmbiAutomaticUpdater>();
            services.AddSingleton<IPlexUserImporter, PlexUserImporter>();
            services.AddSingleton<IEmbyUserImporter, EmbyUserImporter>();
            services.AddSingleton<IJellyfinUserImporter, JellyfinUserImporter>();
            services.AddSingleton<IWelcomeEmail, WelcomeEmail>();
            services.AddSingleton<ICouchPotatoSync, CouchPotatoSync>();
            services.AddSingleton<IProcessProvider, ProcessProvider>();
            services.AddSingleton<ISickRageSync, SickRageSync>();
            services.AddSingleton<IRefreshMetadata, RefreshMetadata>();
            services.AddSingleton<INewsletterJob, NewsletterJob>();
            services.AddSingleton<ILidarrAlbumSync, LidarrAlbumSync>();
            services.AddSingleton<ILidarrArtistSync, LidarrArtistSync>();
            services.AddSingleton<ILidarrAvailabilityChecker, LidarrAvailabilityChecker>();
            services.AddSingleton<IIssuesPurge, IssuesPurge>();
            services.AddSingleton<IResendFailedRequests, ResendFailedRequests>();
            services.AddSingleton<IMediaDatabaseRefresh, MediaDatabaseRefresh>();
            services.AddSingleton<IArrAvailabilityChecker, ArrAvailabilityChecker>();
            services.AddSingleton<IAutoDeleteRequests, AutoDeleteRequests>();
        }
    }
}
