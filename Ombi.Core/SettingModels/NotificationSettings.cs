#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: NotificationSettings.cs
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
    public class NotificationSettings : Settings
    {
        public NotificationSettings()
        {
            Message = new List<NotificationMessage>
            {
                new NotificationMessage { NotificationType = NotificationType.NewRequest },
                new NotificationMessage { NotificationType = NotificationType.Issue },
                new NotificationMessage { NotificationType = NotificationType.AdminNote },
                new NotificationMessage { NotificationType = NotificationType.RequestApproved },
                new NotificationMessage { NotificationType = NotificationType.RequestAvailable }
            };
        }


        public bool Enabled { get; set; }

        public List<NotificationMessage> Message { get; set; }
    }

    public static class NotificationCurly
    {
        public static readonly List<string> Curlys = new List<string> { "Username", "Title", "Date", "Issue", "Type" };
    }
}