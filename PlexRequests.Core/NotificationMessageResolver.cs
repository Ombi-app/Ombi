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
using System.Collections.Generic;
using System.Linq;

using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.Core
{
    public class NotificationMessageResolver
    {
        public string ParseMessage<T>(T notification, NotificationType type) where T : NotificationSettings
        {
            var notificationToParse = notification.Message.FirstOrDefault(x => x.Key == type).Value;
            if (string.IsNullOrEmpty(notificationToParse))
                return string.Empty;

            return Resolve(notificationToParse, notification.CustomParamaters);
        }

        private string Resolve(string message, Dictionary<string,string> paramaters)
        {
            var fields = FindCurlyFields(message);
            

            foreach (var f in fields)
            {
                string outString;
                if (paramaters.TryGetValue(f, out outString))
                {
                    message = message.Replace($"{{{f}}}", outString);
                }
            }

            return message;
        }

        private IEnumerable<string> FindCurlyFields(string message)
        {
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

                if (c == 123) // Start of curly '{'
                {
                    insideCurly = true;
                    continue;
                }

                if (c == 125) // End of curly '}'
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
    }
}