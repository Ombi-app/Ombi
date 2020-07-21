using System;
using System.Threading.Tasks;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Schedule.Jobs.Ombi
{
    public class WelcomeEmail : IWelcomeEmail
    {
        public WelcomeEmail(ISettingsService<EmailNotificationSettings> email, INotificationTemplatesRepository template, ISettingsService<CustomizationSettings> c, 
            IEmailProvider provider)
        {
            _emailSettings = email;
            _email = provider;
            _templates = template;
            _customizationSettings = c;
        }

        private readonly ISettingsService<EmailNotificationSettings> _emailSettings;
        private readonly ISettingsService<CustomizationSettings> _customizationSettings;
        private readonly INotificationTemplatesRepository _templates;
        private readonly IEmailProvider _email;

        public async Task SendEmail(OmbiUser user)
        {
            var settings = await _emailSettings.GetSettingsAsync();
            if (!settings.Enabled)
            {
                return;
            }
            var template = await _templates.GetTemplate(NotificationAgent.Email, NotificationType.WelcomeEmail);
            if (!template.Enabled)
            {
                return;
            }

            var cs = await _customizationSettings.GetSettingsAsync();
            var parsed = Parse(user, template, cs);

            var message = new NotificationMessage
            {
                Message = parsed.Message,
                Subject = parsed.Subject,
                To = user.Email,
            };
            await _email.SendAdHoc(message, settings);
        }

        private NotificationMessageContent Parse(OmbiUser u, NotificationTemplates template, CustomizationSettings cs)
        {
            var resolver = new NotificationMessageResolver();
            var curlys = new NotificationMessageCurlys();
            curlys.Setup(u, cs);
            var parsed = resolver.ParseMessage(template, curlys);

            return parsed;
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                //_emailSettings?.Dispose();
                //_customizationSettings?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}