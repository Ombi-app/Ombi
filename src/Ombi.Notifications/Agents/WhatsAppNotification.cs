using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;
using Ombi.Store.Repository.Requests;
using Ombi.Api.Twilio;

namespace Ombi.Notifications.Agents
{
    public class WhatsAppNotification : BaseNotification<TwilioSettings>
    {
        public WhatsAppNotification(IWhatsAppApi api, ISettingsService<TwilioSettings> sn, ILogger<WhatsAppNotification> log, 
                                    INotificationTemplatesRepository r, IMovieRequestRepository m, 
                                    ITvRequestRepository t, ISettingsService<CustomizationSettings> s
            , IRepository<RequestSubscription> sub, IMusicRequestRepository music,
            IRepository<UserNotificationPreferences> userPref) : base(sn, r, m, t,s,log, sub, music, userPref)
        {
            Api = api;
            Logger = log;
        }

        public override string NotificationName => "WhatsAppNotification";

        private IWhatsAppApi Api { get; }
        private ILogger Logger { get; }

        protected override bool ValidateConfiguration(TwilioSettings settings)
        {
            if (!settings?.WhatsAppSettings?.Enabled ?? false)
            {
                return false;
            }
            return (!settings.WhatsAppSettings?.AccountSid?.IsNullOrEmpty() ?? false) 
                && (!settings.WhatsAppSettings?.AuthToken?.IsNullOrEmpty() ?? false) 
                && (!settings.WhatsAppSettings?.From?.IsNullOrEmpty() ?? false);
        }

        protected override async Task NewRequest(NotificationOptions model, TwilioSettings settings)
        {
            await Run(model, settings, NotificationType.NewRequest);
        }

        protected override async Task NewIssue(NotificationOptions model, TwilioSettings settings)
        {
            await Run(model, settings, NotificationType.Issue);
        }

        protected override async Task IssueComment(NotificationOptions model, TwilioSettings settings)
        {
            await Run(model, settings, NotificationType.IssueComment);
        }

        protected override async Task IssueResolved(NotificationOptions model, TwilioSettings settings)
        {
            await Run(model, settings, NotificationType.IssueResolved);
        }

        protected override async Task AddedToRequestQueue(NotificationOptions model, TwilioSettings settings)
        {
            await Run(model, settings, NotificationType.ItemAddedToFaultQueue);
        }

        protected override async Task RequestDeclined(NotificationOptions model, TwilioSettings settings)
        {
            await Run(model, settings, NotificationType.RequestDeclined);
        }

        protected override async Task RequestApproved(NotificationOptions model, TwilioSettings settings)
        {
            await Run(model, settings, NotificationType.RequestApproved);
        }

        protected override async Task AvailableRequest(NotificationOptions model, TwilioSettings settings)
        {
            await Run(model, settings, NotificationType.RequestAvailable);
        }

        protected override async Task Send(NotificationMessage model, TwilioSettings settings)
        {
            try
            {
                var whatsApp = new WhatsAppModel
                {
                    Message = model.Message,
                    From = settings.WhatsAppSettings.From,
                    To = ""// TODO
                };
                await Api.SendMessage(whatsApp, settings.WhatsAppSettings.AccountSid, settings.WhatsAppSettings.AuthToken);
            }
            catch (Exception e)
            {
                Logger.LogError(LoggingEvents.WhatsApp, e, "Failed to send WhatsApp Notification");
            }
        }

        protected override async Task Test(NotificationOptions model, TwilioSettings settings)
        {
            var message = $"This is a test from Ombi, if you can see this then we have successfully pushed a notification!";
            var notification = new NotificationMessage
            {
                Message = message,
            };
            await Send(notification, settings);
        }

        private async Task Run(NotificationOptions model, TwilioSettings settings, NotificationType type)
        {
            var parsed = await LoadTemplate(NotificationAgent.WhatsApp, type, model);
            if (parsed.Disabled)
            {
                Logger.LogInformation($"Template {type} is disabled for {NotificationAgent.WhatsApp}");
                return;
            }
            var notification = new NotificationMessage
            {
                Message = parsed.Message,
            };
            await Send(notification, settings);
        }
    }
}
