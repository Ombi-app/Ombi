#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: NotificationSettingsV2.cs
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
using Ombi.Core.Models;
using Ombi.Core.Notification;

namespace Ombi.Core.SettingModels
{
    public class NotificationSettingsV2 : Settings
    {
        public NotificationSettingsV2()
        {
            EmailNotification = new List<NotificationMessage>
            {
                new NotificationMessage
                {
                    Body = "BODY",
                    NotificationType = NotificationType.NewRequest,
                    Subject = "SUB"
                },
                new NotificationMessage
                {
                    NotificationType = NotificationType.Issue,
                    Body = "issue",
                    Subject = "issuesub"
                }
            };
            SlackNotification = new List<NotificationMessage>();
            PushoverNotification = new List<NotificationMessage>();
            PushbulletNotification = new List<NotificationMessage>();
        }
        public List<NotificationMessage> EmailNotification { get; set; }
        public List<NotificationMessage> SlackNotification { get; set; }
        public List<NotificationMessage> PushbulletNotification { get; set; }
        public List<NotificationMessage> PushoverNotification { get; set; }
    }
}