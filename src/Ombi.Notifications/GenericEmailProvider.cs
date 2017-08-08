using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Ombi.Notifications.Models;
using Ombi.Notifications.Templates;
using Ombi.Settings.Settings.Models.Notifications;

namespace Ombi.Notifications
{
    public class GenericEmailProvider : IEmailProvider
    {
        public async Task SendAdHoc(NotificationMessage model, EmailNotificationSettings settings)
        {
            try
            {

                var email = new EmailBasicTemplate();
                var html = email.LoadTemplate(model.Subject, model.Message, null);

                var body = new BodyBuilder
                {
                    HtmlBody = html,
                    //TextBody = model.Other["PlainTextBody"]
                };

                var message = new MimeMessage
                {
                    Body = body.ToMessageBody(),
                    Subject = model.Subject
                };
                message.From.Add(new MailboxAddress(settings.SenderAddress, settings.SenderAddress));
                message.To.Add(new MailboxAddress(model.To, model.To));

                using (var client = new SmtpClient())
                {
                    client.Connect(settings.Host, settings.Port); // Let MailKit figure out the correct SecureSocketOptions.

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
                var body = new BodyBuilder
                {
                    HtmlBody = model.Message,
                    //TextBody = model.Other["PlainTextBody"]
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
                    client.Connect(settings.Host, settings.Port); // Let MailKit figure out the correct SecureSocketOptions.

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
    }
}