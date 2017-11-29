using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Exceptions;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;

namespace Ombi.Notifications.Interfaces
{
    public abstract class BaseNotification<T> : INotification where T : Settings.Settings.Models.Settings, new()
    {
        protected BaseNotification(ISettingsService<T> settings, INotificationTemplatesRepository templateRepo, IMovieRequestRepository movie, ITvRequestRepository tv,
            ISettingsService<CustomizationSettings> customization)
        {
            Settings = settings;
            TemplateRepository = templateRepo;
            MovieRepository = movie;
            TvRepository = tv;
            CustomizationSettings = customization;
        }
        
        protected ISettingsService<T> Settings { get; }
        protected INotificationTemplatesRepository TemplateRepository { get; }
        protected IMovieRequestRepository MovieRepository { get; }
        protected ITvRequestRepository TvRepository { get; }
        protected CustomizationSettings Customization { get; set; }
        private ISettingsService<CustomizationSettings> CustomizationSettings { get; }


        protected ChildRequests TvRequest { get; set; }
        protected MovieRequests MovieRequest { get; set; }
        
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
            if (model.RequestId > 0)
            {
                await LoadRequest(model.RequestId, model.RequestType);
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
                        await Issue(model, notificationSettings);
                        break;
                    case NotificationType.RequestAvailable:
                        await AvailableRequest(model, notificationSettings);
                        break;
                    case NotificationType.RequestApproved:
                        await RequestApproved(model, notificationSettings);
                        break;
                    case NotificationType.AdminNote:
                        throw new NotImplementedException();

                    case NotificationType.Test:
                        await Test(model, notificationSettings);
                        break;
                    case NotificationType.RequestDeclined:
                        await RequestDeclined(model, notificationSettings);
                        break;
                    case NotificationType.ItemAddedToFaultQueue:
                        await AddedToRequestQueue(model, notificationSettings);
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
            else
            {
               TvRequest = await TvRepository.GetChild().FirstOrDefaultAsync(x => x.Id == requestId);
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
                return new NotificationMessageContent {Disabled = true};
            }
            var parsed = Parse(model, template);

            return parsed;
        }

        private NotificationMessageContent Parse(NotificationOptions model, NotificationTemplates template)
        {
            var resolver = new NotificationMessageResolver();
            var curlys = new NotificationMessageCurlys();
            if (model.RequestType == RequestType.Movie)
            {
                curlys.Setup(MovieRequest, Customization);
            }
            else
            {
                curlys.Setup(TvRequest, Customization);
            }
            var parsed = resolver.ParseMessage(template, curlys);

            return parsed;
        }


        protected abstract bool ValidateConfiguration(T settings);
        protected abstract Task NewRequest(NotificationOptions model, T settings);
        protected abstract Task Issue(NotificationOptions model, T settings);
        protected abstract Task AddedToRequestQueue(NotificationOptions model, T settings);
        protected abstract Task RequestDeclined(NotificationOptions model, T settings);
        protected abstract Task RequestApproved(NotificationOptions model, T settings);
        protected abstract Task AvailableRequest(NotificationOptions model, T settings);
        protected abstract Task Send(NotificationMessage model, T settings);
        protected abstract Task Test(NotificationOptions model, T settings);
    }
}