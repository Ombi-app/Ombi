using System;
using System.Threading.Tasks;
using EnsureThat;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ombi.Core.Settings;
using Ombi.Notifications.Models;
using Ombi.Notifications.Templates;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Notifications
{
    public class GenericEmailProvider : IEmailProvider
    {
        public GenericEmailProvider(ISettingsService<CustomizationSettings> s, ILogger<GenericEmailProvider> log)
        {
            CustomizationSettings = s;
            _log = log;
        }
        private ISettingsService<CustomizationSettings> CustomizationSettings { get; }
        private readonly ILogger<GenericEmailProvider> _log;

        /// <summary>
        /// This will load up the Email template and generate the HTML
        /// </summary>
        /// <param name="model"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task SendAdHoc(NotificationMessage model, EmailNotificationSettings settings)
        {
            try
            {
                var email = new EmailBasicTemplate();

                var customization = await CustomizationSettings.GetSettingsAsync();
                var html = email.LoadTemplate(model.Subject, model.Message, null, customization.Logo);

                var body = new BodyBuilder
                {
                    HtmlBody = html,
                    TextBody = model.Other["PlainTextBody"]
                };

                var message = new MimeMessage
                {
                    Body = body.ToMessageBody(),
                    Subject = model.Subject
                };
                message.From.Add(new MailboxAddress(string.IsNullOrEmpty(settings.SenderName) ? settings.SenderAddress : settings.SenderName, settings.SenderAddress));
                message.To.Add(new MailboxAddress(model.To, model.To));
                
                using (var client = new SmtpClient())
                {
                    if (settings.DisableCertificateChecking)
                    {
                        // Disable validation of the certificate associated with the SMTP service
                        // Helpful when the TLS certificate is not in the certificate store of the server
                        // Does carry the risk of man in the middle snooping
                        client.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    }

                    if (settings.DisableTLS)
                    {
                        // Does not attempt to use either TLS or SSL
                        // Helpful when MailKit finds a TLS certificate, but it unable to use it
                        client.Connect(settings.Host, settings.Port, MailKit.Security.SecureSocketOptions.None); 
                    }
                    else
                    {
                        client.Connect(settings.Host, settings.Port); // Let MailKit figure out the correct SecureSocketOptions.
                    }
                    
                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    if (settings.Authentication)
                    {
                        client.Authenticate(settings.Username, settings.Password);
                    }
                    //Log.Info("sending message to {0} \r\n from: {1}\r\n Are we authenticated: {2}", message.To, message.From, client.IsAuthenticated);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception e)
            {
                //Log.Error(e);
                throw new InvalidOperationException(e.Message);
            }
        }

        public async Task Send(NotificationMessage model, EmailNotificationSettings settings)
        {
            try
            {
                EnsureArg.IsNotNullOrEmpty(settings.SenderAddress);
                EnsureArg.IsNotNullOrEmpty(model.To);
                EnsureArg.IsNotNullOrEmpty(model.Message);
                EnsureArg.IsNotNullOrEmpty(settings.Host);

                var body = new BodyBuilder
                {
                    HtmlBody = model.Message,
                    TextBody = model.Other["PlainTextBody"]
                };

                var message = new MimeMessage
                {
                    Body = body.ToMessageBody(),
                    Subject = model.Subject
                };

                message.From.Add(new MailboxAddress(string.IsNullOrEmpty(settings.SenderName) ? settings.SenderAddress : settings.SenderName, settings.SenderAddress));
                message.To.Add(new MailboxAddress(model.To, model.To));

                using (var client = new SmtpClient())
                {
                    if (settings.DisableCertificateChecking)
                    {
                        client.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    }

                    if (settings.DisableTLS)
                    {
                        client.Connect(settings.Host, settings.Port, MailKit.Security.SecureSocketOptions.None);
                    }
                    else
                    {
                        client.Connect(settings.Host, settings.Port); // Let MailKit figure out the correct SecureSocketOptions.
                    }

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    if (settings.Authentication)
                    {
                        client.Authenticate(settings.Username, settings.Password);
                    }
                    //Log.Info("sending message to {0} \r\n from: {1}\r\n Are we authenticated: {2}", message.To, message.From, client.IsAuthenticated);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "Exception when attempting to send email");
                throw;
            }
        }
    }
}