#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: NotificationService.cs
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
using System.Threading;

using NLog;

using PlexRequests.Helpers;

namespace PlexRequests.Services.Notification
{
    public static class NotificationService
    {

        private static Logger Log = LogManager.GetCurrentClassLogger();
        public static Dictionary<string, INotification> Observers { get; }

        static NotificationService()
        {
            Observers = new Dictionary<string, INotification>();
        }

        public static void Publish(string title, string requester)
        {
            Log.Trace("Notifying all observers: ");
            Log.Trace(Observers.DumpJson());
            foreach (var observer in Observers)
            {
                var notification = observer.Value;

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    notification.Notify(title, requester);
                }).Start();
            }
        }

        public static void Subscribe(INotification notification)
        {
            Log.Trace("Subscribing Observer {0}", notification.NotificationName);
            INotification notificationValue;
            if (Observers.TryGetValue(notification.NotificationName, out notificationValue))
            {
                Log.Trace("Observer {0} already exists", notification.NotificationName);
                // Observer already exists
                return;
            }

            Observers[notification.NotificationName] = notification;
        }

        public static void UnSubscribe(INotification notification)
        {
            Log.Trace("Unsubscribing Observer {0}", notification.NotificationName);
            INotification notificationValue;
            if (!Observers.TryGetValue(notification.NotificationName, out notificationValue))
            {
                Log.Trace("Observer {0} doesn't exist to Unsubscribe", notification.NotificationName);
                // Observer doesn't exists
                return;
            }
            Observers.Remove(notification.NotificationName);
        }
    }
}