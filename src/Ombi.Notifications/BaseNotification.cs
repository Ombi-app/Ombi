using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Exceptions;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications
{
    public abstract class BaseNotification<T> : INotification where T : Settings.Settings.Models.Settings, new()
    {
        protected BaseNotification(ISettingsService<T> settings, INotificationTemplatesRepository templateRepo, IMovieRequestRepository movie, ITvRequestRepository tv,
            ISettingsService<CustomizationSettings> customization, ILogger<BaseNotification<T>> log, IRepository<RequestSubscription> sub, IMusicRequestRepository album,
            IRepository<UserNotificationPreferences> notificationUserPreferences)
        {
            Settings = settings;
            TemplateRepository = templateRepo;
            MovieRepository = movie;
            TvRepository = tv;
            CustomizationSettings = customization;
            RequestSubscription = sub;
            _log = log;
            AlbumRepository = album;
            UserNotificationPreferences = notificationUserPreferences;
            Settings.ClearCache();
        }

        protected ISettingsService<T> Settings { get; }
        protected INotificationTemplatesRepository TemplateRepository { get; }
        protected IMovieRequestRepository MovieRepository { get; }
        protected ITvRequestRepository TvRepository { get; }
        protected IMusicRequestRepository AlbumRepository { get; }
        protected CustomizationSettings Customization { get; set; }
        protected IRepository<RequestSubscription> RequestSubscription { get; set; }
        protected IRepository<UserNotificationPreferences> UserNotificationPreferences { get; set; }
        private ISettingsService<CustomizationSettings> CustomizationSettings { get; }
        private readonly ILogger<BaseNotification<T>> _log;


        protected ChildRequests TvRequest { get; set; }
        protected AlbumRequest AlbumRequest { get; set; }
        protected MovieRequests MovieRequest { get; set; }
        protected IQueryable<OmbiUser> SubsribedUsers { get; private set; }

        public abstract string NotificationName { get; }

        public async Task NotifyAsync(NotificationOptions model)
        {
            var configuration = await GetConfiguration();
            await NotifyAsync(model, configuration);
        }

        public async Task NotifyAsync(NotificationOptions model, Settings.Settings.Models.Settings settings)
        {
            if (settings == null) await NotifyAsync(model);

            var notificationSettings = (T)settings;

            if (!ValidateConfiguration(notificationSettings))
            {
                return;
            }

            // Is this a test?
            // The request id for tests is -1
            // Also issues are 0 since there might not be a request associated
            if (model.RequestId > 0)
            {
                await LoadRequest(model.RequestId, model.RequestType);
                SubsribedUsers = GetSubscriptions(model.RequestId, model.RequestType);
            }

            Customization = await CustomizationSettings.GetSettingsAsync();
            try
            {
                switch (model.NotificationType)
                {
                    case NotificationType.NewRequest:
                        await NewRequest(model, notificationSettings);
                        break;
                    case NotificationType.Issue:
                        await NewIssue(model, notificationSettings);
                        break;
                    case NotificationType.RequestAvailable:
                        await AvailableRequest(model, notificationSettings);
                        break;
                    case NotificationType.RequestApproved:
                        await RequestApproved(model, notificationSettings);
                        break;
                    case NotificationType.Test:
                        await Test(model, notificationSettings);
                        break;
                    case NotificationType.RequestDeclined:
                        await RequestDeclined(model, notificationSettings);
                        break;
                    case NotificationType.ItemAddedToFaultQueue:
                        await AddedToRequestQueue(model, notificationSettings);
                        break;
                    case NotificationType.IssueResolved:
                        await IssueResolved(model, notificationSettings);
                        break;
                    case NotificationType.IssueComment:
                        await IssueComment(model, notificationSettings);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (NotImplementedException)
            {
                // Do nothing, it's not implimented meaning it might not be ready or even used
            }
        }

        /// <summary>
        /// Loads the TV or Movie Request
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual async Task LoadRequest(int requestId, RequestType type)
        {
            if (type == RequestType.Movie)
            {
                MovieRequest = await MovieRepository.GetWithUser().FirstOrDefaultAsync(x => x.Id == requestId);
            }
            else if (type == RequestType.TvShow)
            {
                TvRequest = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == requestId);
            }
            else if (type == RequestType.Album)
            {
                AlbumRequest = await AlbumRepository.GetWithUser().FirstOrDefaultAsync(x => x.Id == requestId);
            }
        }

        private async Task<T> GetConfiguration()
        {
            var settings = await Settings.GetSettingsAsync();
            return settings;
        }

        /// <summary>
        /// Loads the correct template from the DB
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="type"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual async Task<NotificationMessageContent> LoadTemplate(NotificationAgent agent, NotificationType type, NotificationOptions model)
        {
            var template = await TemplateRepository.GetTemplate(agent, type);
            if (template == null)
            {
                throw new TemplateMissingException($"The template for {agent} and type {type} is missing");
            }
            if (!template.Enabled)
            {
                return new NotificationMessageContent { Disabled = true };
            }

            if (model.UserId.IsNullOrEmpty())
            {
                if (model.RequestType == RequestType.Movie)
                {
                    model.UserId = MovieRequest.RequestedUserId;
                }

                if (model.RequestType == RequestType.Album)
                {
                    model.UserId = AlbumRequest.RequestedUserId;
                }

                if (model.RequestType == RequestType.TvShow)
                {
                    model.UserId = TvRequest.RequestedUserId;
                }
            }
            var parsed = Parse(model, template, agent);

            return parsed;
        }

        protected IQueryable<OmbiUser> GetSubscriptions(int requestId, RequestType type)
        {
            var subs = RequestSubscription.GetAll().Include(x => x.User).ThenInclude(x => x.NotificationUserIds).Where(x => x.RequestId == requestId && type == x.RequestType);
            return subs.Select(x => x.User);
        }

        protected UserNotificationPreferences GetUserPreference(string userId, NotificationAgent agent)
        {
            return UserNotificationPreferences.GetAll()
                .FirstOrDefault(x => x.Agent == agent && x.UserId == userId);
        }

        private NotificationMessageContent Parse(NotificationOptions model, NotificationTemplates template, NotificationAgent agent)
        {
            var resolver = new NotificationMessageResolver();
            var curlys = new NotificationMessageCurlys();
            var preference = GetUserPreference(model.UserId, agent);
            if (model.RequestType == RequestType.Movie)
            {
                _log.LogDebug("Notification options: {@model}, Req: {@MovieRequest}, Settings: {@Customization}", model, MovieRequest, Customization);

                curlys.Setup(model, MovieRequest, Customization, preference);
            }
            else if (model.RequestType == RequestType.TvShow)
            {
                _log.LogDebug("Notification options: {@model}, Req: {@TvRequest}, Settings: {@Customization}", model, TvRequest, Customization);
                curlys.Setup(model, TvRequest, Customization, preference);
            }
            else if (model.RequestType == RequestType.Album)
            {
                _log.LogDebug("Notification options: {@model}, Req: {@AlbumRequest}, Settings: {@Customization}", model, AlbumRequest, Customization);
                curlys.Setup(model, AlbumRequest, Customization, preference);
            }
            var parsed = resolver.ParseMessage(template, curlys);

            return parsed;
        }


        protected abstract bool ValidateConfiguration(T settings);
        protected abstract Task NewRequest(NotificationOptions model, T settings);
        protected abstract Task NewIssue(NotificationOptions model, T settings);
        protected abstract Task IssueComment(NotificationOptions model, T settings);
        protected abstract Task IssueResolved(NotificationOptions model, T settings);
        protected abstract Task AddedToRequestQueue(NotificationOptions model, T settings);
        protected abstract Task RequestDeclined(NotificationOptions model, T settings);
        protected abstract Task RequestApproved(NotificationOptions model, T settings);
        protected abstract Task AvailableRequest(NotificationOptions model, T settings);
        protected abstract Task Send(NotificationMessage model, T settings);
        protected abstract Task Test(NotificationOptions model, T settings);
    }
}