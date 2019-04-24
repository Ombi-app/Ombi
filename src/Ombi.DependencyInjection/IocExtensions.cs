﻿using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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
using Ombi.Api.DogNzb;
using Ombi.Api.FanartTv;
using Ombi.Api.Github;
using Ombi.Api.Gotify;
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
using Ombi.Store.Entities;
using Quartz.Spi;

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
            services.AddTransient<IMovieEngine, MovieSearchEngine>();
            services.AddTransient<IMovieRequestEngine, MovieRequestEngine>();
            services.AddTransient<ITvRequestEngine, TvRequestEngine>();
            services.AddTransient<ITvSearchEngine, TvSearchEngine>();
            services.AddTransient<IRuleEvaluator, RuleEvaluator>();
            services.AddTransient<IUserStatsEngine, UserStatsEngine>();
            services.AddTransient<IMovieSender, MovieSender>();
            services.AddTransient<IRecentlyAddedEngine, RecentlyAddedEngine>();
            services.AddTransient<IMusicSearchEngine, MusicSearchEngine>();
            services.AddTransient<IMusicRequestEngine, MusicRequestEngine>();
            services.AddTransient<ITvSender, TvSender>();
            services.AddTransient<IMusicSender, MusicSender>();
            services.AddTransient<IMassEmailSender, MassEmailSender>();
            services.AddTransient<IPlexOAuthManager, PlexOAuthManager>();
            services.AddTransient<IVoteEngine, VoteEngine>();
            services.AddTransient<IDemoMovieSearchEngine, DemoMovieSearchEngine>();
            services.AddTransient<IDemoTvSearchEngine, DemoTvSearchEngine>();
        }

        public static void RegisterEnginesV2(this IServiceCollection services)
        {
            services.AddTransient<IMultiSearchEngine, MultiSearchEngine>();
            services.AddTransient<IMovieEngineV2, MovieSearchEngineV2>();
            services.AddTransient<ITVSearchEngineV2, TvSearchEngineV2>();
            services.AddTransient<ICalendarEngine, CalendarEngine>();
        }

        public static void RegisterHttp(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPrincipal>(sp => sp.GetService<IHttpContextAccessor>().HttpContext.User);
        }

        public static void RegisterApi(this IServiceCollection services)
        {
            services.AddSingleton<IApi, Api.Api>();
            services.AddSingleton<IOmbiHttpClient, OmbiHttpClient>(); // https://blogs.msdn.microsoft.com/alazarev/2017/12/29/disposable-finalizers-and-httpclient/
            services.AddTransient<IMovieDbApi, Api.TheMovieDb.TheMovieDbApi>();
            services.AddTransient<IPlexApi, PlexApi>();
            services.AddTransient<IEmbyApi, EmbyApi>();
            services.AddTransient<ISonarrApi, SonarrApi>();
            services.AddTransient<ISonarrV3Api, SonarrV3Api>();
            services.AddTransient<ISlackApi, SlackApi>();
            services.AddTransient<ITvMazeApi, TvMazeApi>();
            services.AddTransient<ITraktApi, TraktApi>();
            services.AddTransient<IRadarrApi, RadarrApi>();
            services.AddTransient<IDiscordApi, DiscordApi>();
            services.AddTransient<IPushbulletApi, PushbulletApi>();
            services.AddTransient<IOmbiService, OmbiService>();
            services.AddTransient<IFanartTvApi, FanartTvApi>();
            services.AddTransient<IPushoverApi, PushoverApi>();
            services.AddTransient<IGotifyApi, GotifyApi>();
            services.AddTransient<IMattermostApi, MattermostApi>();
            services.AddTransient<ICouchPotatoApi, CouchPotatoApi>();
            services.AddTransient<IDogNzbApi, DogNzbApi>();
            services.AddTransient<ITelegramApi, TelegramApi>();
            services.AddTransient<IGithubApi, GithubApi>();
            services.AddTransient<ISickRageApi, SickRageApi>();
            services.AddTransient<IAppVeyorApi, AppVeyorApi>();
            services.AddTransient<IOneSignalApi, OneSignalApi>();
            services.AddTransient<ILidarrApi, LidarrApi>();
        }

        public static void RegisterStore(this IServiceCollection services) { 
            services.AddDbContext<OmbiContext>();
            services.AddDbContext<SettingsContext>();
            services.AddDbContext<ExternalContext>();
            
            services.AddScoped<IOmbiContext, OmbiContext>(); // https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6
            services.AddScoped<ISettingsContext, SettingsContext>(); // https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6
            services.AddScoped<IExternalContext, ExternalContext>(); // https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6
            services.AddScoped<ISettingsRepository, SettingsJsonRepository>();
            services.AddScoped<ISettingsResolver, SettingsResolver>();
            services.AddScoped<IPlexContentRepository, PlexServerContentRepository>();
            services.AddScoped<IEmbyContentRepository, EmbyContentRepository>();
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
            services.AddTransient<IRequestServiceMain, RequestService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IEmailProvider, GenericEmailProvider>();
            services.AddTransient<INotificationHelper, NotificationHelper>();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddTransient<IDiscordNotification, DiscordNotification>();
            services.AddTransient<IEmailNotification, EmailNotification>();
            services.AddTransient<IPushbulletNotification, PushbulletNotification>();
            services.AddTransient<ISlackNotification, SlackNotification>();
            services.AddTransient<ISlackNotification, SlackNotification>();
            services.AddTransient<IMattermostNotification, MattermostNotification>();
            services.AddTransient<IPushoverNotification, PushoverNotification>();
            services.AddTransient<IGotifyNotification, GotifyNotification>();
            services.AddTransient<ITelegramNotification, TelegramNotification>();
            services.AddTransient<IMobileNotification, MobileNotification>();
            services.AddTransient<IChangeLogProcessor, ChangeLogProcessor>();
        }

        public static void RegisterJobs(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, IoCJobFactory>(provider => new IoCJobFactory(provider));
            services.AddTransient<IBackgroundJobClient, BackgroundJobClient>();

            services.AddTransient<IPlexContentSync, PlexContentSync>();
            services.AddTransient<IEmbyContentSync, EmbyContentSync>();
            services.AddTransient<IEmbyEpisodeSync, EmbyEpisodeSync>();
            services.AddTransient<IEmbyAvaliabilityChecker, EmbyAvaliabilityChecker>();
            services.AddTransient<IPlexEpisodeSync, PlexEpisodeSync>();
            services.AddTransient<IPlexAvailabilityChecker, PlexAvailabilityChecker>();
            services.AddTransient<IJobSetup, JobSetup>();
            services.AddTransient<IRadarrSync, RadarrSync>();
            services.AddTransient<ISonarrSync, SonarrSync>();
            services.AddTransient<IOmbiAutomaticUpdater, OmbiAutomaticUpdater>();
            services.AddTransient<IPlexUserImporter, PlexUserImporter>();
            services.AddTransient<IEmbyUserImporter, EmbyUserImporter>();
            services.AddTransient<IWelcomeEmail, WelcomeEmail>();
            services.AddTransient<ICouchPotatoSync, CouchPotatoSync>();
            services.AddTransient<IProcessProvider, ProcessProvider>();
            services.AddTransient<ISickRageSync, SickRageSync>();
            services.AddTransient<IRefreshMetadata, RefreshMetadata>();
            services.AddTransient<INewsletterJob, NewsletterJob>();
            services.AddTransient<ILidarrAlbumSync, LidarrAlbumSync>();
            services.AddTransient<ILidarrArtistSync, LidarrArtistSync>();
            services.AddTransient<ILidarrAvailabilityChecker, LidarrAvailabilityChecker>();
            services.AddTransient<IIssuesPurge, IssuesPurge>();
            services.AddTransient<IResendFailedRequests, ResendFailedRequests>();
            services.AddTransient<IMediaDatabaseRefresh, MediaDatabaseRefresh>();
        }
    }
}
