#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CustomNotificationService.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using Ombi.Core.Models;
using Ombi.Core.SettingModels;

namespace Ombi.Core.Notification
{
    public class NotificationMessageResolver
    {
        /// <summary>
        /// The start character '{'
        /// </summary>
        private const char StartChar = (char)123;
        /// <summary>
        /// The end character '}'
        /// </summary>
        private const char EndChar = (char)125;

        /// <summary>
        /// Parses the message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="notification">The notification.</param>
        /// <param name="type">The type.</param>
        /// <param name="c">The c.</param>
        /// <param name="transportType">Type of the transport.</param>
        /// <returns></returns>
        public NotificationMessageContent ParseMessage(NotificationSettingsV2 notification, NotificationType type, NotificationMessageCurlys c, TransportType transportType)
        {
            IEnumerable<NotificationMessage> content = null;
            switch (transportType)
            {
                case TransportType.Email:
                    content = notification.EmailNotification;
                    break;
                case TransportType.Pushbullet:
                    content = notification.PushbulletNotification;
                    break;
                case TransportType.Pushover:
                    content = notification.PushoverNotification;
                    break;
                case TransportType.Slack:
                    content = notification.SlackNotification;
                    break;
                case TransportType.Mattermost:
                    content = notification.MattermostNotification;
                    break;    
                default:
                    throw new ArgumentOutOfRangeException(nameof(transportType), transportType, null);
            }
           
            if (content == null)
            {
                return new NotificationMessageContent();
            }

            var message = content.FirstOrDefault(x => x.NotificationType == type) ?? new NotificationMessage();

            return Resolve(message.Body, message.Subject, c.Curlys);
        }

        /// <summary>
        /// Resolves the specified message curly fields.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private NotificationMessageContent Resolve(string body, string subject, IReadOnlyDictionary<string, string> parameters)
        {
            // Find the fields
            var bodyFields = FindCurlyFields(body);
            var subjectFields = FindCurlyFields(subject);

            body = ReplaceFields(bodyFields, parameters, body);
            subject = ReplaceFields(subjectFields, parameters, subject);

            return new NotificationMessageContent {Body = body ?? string.Empty, Subject = subject ?? string.Empty};
        }

        /// <summary>
        /// Finds the curly fields.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private IEnumerable<string> FindCurlyFields(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return new List<string>();
            }
            var insideCurly = false;
            var fields = new List<string>();
            var currentWord = string.Empty;
            var chars = message.ToCharArray();

            foreach (var c in chars)
            {
                if (char.IsWhiteSpace(c))
                {
                    currentWord = string.Empty;
                    continue;
                }

                if (c == StartChar) // Start of curly '{'
                {
                    insideCurly = true;
                    continue;
                }

                if (c == EndChar) // End of curly '}'
                {
                    fields.Add(currentWord); // We have finished the curly, add the word into the list
                    currentWord = string.Empty;
                    insideCurly = false;
                    continue;
                }

                if (insideCurly)
                {
                    currentWord += c.ToString(); // Add the character onto the word.
                }
            }

            return fields;
        }

        /// <summary>
        /// Replaces the fields.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="mainText">The main text.</param>
        /// <returns></returns>
        private string ReplaceFields(IEnumerable<string> fields, IReadOnlyDictionary<string, string> parameters, string mainText)
        {
            foreach (var field in fields)
            {
                string outString;
                if (parameters.TryGetValue(field, out outString))
                {
                    mainText = mainText.Replace($"{{{field}}}", outString);
                }
            }
            return mainText;
        }
    }
}