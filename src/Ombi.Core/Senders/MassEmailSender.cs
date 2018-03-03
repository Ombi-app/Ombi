#region Copyright
// /************************************************************************
//    Copyright (c) 2018 Jamie Rees
//    File: MassEmailSender.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ombi.Core.Authentication;
using Ombi.Core.Models;
using Ombi.Core.Settings;
using Ombi.Helpers;
using Ombi.Notifications;
using Ombi.Notifications.Models;
using Ombi.Settings.Settings.Models;
using Ombi.Settings.Settings.Models.Notifications;
using Ombi.Store.Entities;

namespace Ombi.Core.Senders
{
    public class MassEmailSender : IMassEmailSender
    {
        public MassEmailSender(IEmailProvider emailProvider, ISettingsService<CustomizationSettings> custom, ISettingsService<EmailNotificationSettings> email,
            ILogger<MassEmailSender> log, OmbiUserManager manager)
        {
            _email = emailProvider;
            _customizationService = custom;
            _emailService = email;
            _log = log;
            _userManager = manager;
        }

        private readonly IEmailProvider _email;
        private readonly ISettingsService<CustomizationSettings> _customizationService;
        private readonly ISettingsService<EmailNotificationSettings> _emailService;
        private readonly ILogger<MassEmailSender> _log;
        private readonly OmbiUserManager _userManager;

        public async Task<bool> SendMassEmail(MassEmailModel model)
        {
            var customization = await _customizationService.GetSettingsAsync();
            var email = await _emailService.GetSettingsAsync();
            var messagesSent = new List<Task>();
            foreach (var user in model.Users)
            {
                var fullUser = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
                if (!fullUser.Email.HasValue())
                {
                    _log.LogInformation("User {0} has no email, cannot send mass email to this user", fullUser.UserName);
                    continue;
                }
                var resolver = new NotificationMessageResolver();
                var curlys = new NotificationMessageCurlys();
                curlys.Setup(fullUser, customization);
                var template = new NotificationTemplates() { Message = model.Body, Subject = model.Subject };
                var content = resolver.ParseMessage(template, curlys);
                var msg = new NotificationMessage
                {
                    Message = content.Message,
                    To = fullUser.Email,
                    Subject = content.Subject
                };
                messagesSent.Add(_email.SendAdHoc(msg, email));
                _log.LogInformation("Sent mass email to user {0} @ {1}", fullUser.UserName, fullUser.Email);
            }

            await Task.WhenAll(messagesSent);

            return true;
        }
    }
}