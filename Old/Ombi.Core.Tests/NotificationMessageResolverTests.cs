//#region Copyright
//// /************************************************************************
////    Copyright (c) 2016 Jamie Rees
////    File: AuthenticationSettingsTests.cs
////    Created By: Jamie Rees
////   
////    Permission is hereby granted, free of charge, to any person obtaining
////    a copy of this software and associated documentation files (the
////    "Software"), to deal in the Software without restriction, including
////    without limitation the rights to use, copy, modify, merge, publish,
////    distribute, sublicense, and/or sell copies of the Software, and to
////    permit persons to whom the Software is furnished to do so, subject to
////    the following conditions:
////   
////    The above copyright notice and this permission notice shall be
////    included in all copies or substantial portions of the Software.
////   
////    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
////    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
////    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
////    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
////    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
////    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
////    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////  ************************************************************************/
//#endregion
//using System;
//using System.Collections.Generic;

//using NUnit.Framework;

//using PlexRequests.Core.Models;
//using PlexRequests.Core.Notification;
//using PlexRequests.Core.SettingModels;

//namespace PlexRequests.Core.Tests
//{
//    [TestFixture]
//    public class NotificationMessageResolverTests
//    {
//        [TestCaseSource(nameof(MessageBodyResolver))]
//        public string ResolveBody(string body, NotificationMessageCurlys param)
//        {
//            var n = new NotificationMessageResolver();
//            var s = new NotificationSettings
//            {
//                Message = new List<Notification.NotificationMessage> { new Notification.NotificationMessage { NotificationType = NotificationType.NewRequest, Body = body } } 
//            };

//            var result = n.ParseMessage(s, NotificationType.NewRequest, param, TransportType.Email);
//            return result.Body;
//        }

//        [TestCaseSource(nameof(MessageSubjectResolver))]
//        public string ResolveSubject(string subject, NotificationMessageCurlys param)
//        {
//            var n = new NotificationMessageResolver();
//            var s = new NotificationSettings
//            {
//                Message = new List<Notification.NotificationMessage> { new Notification.NotificationMessage { NotificationType = NotificationType.NewRequest, Subject = subject }} 
//            };

//            var result = n.ParseMessage(s, NotificationType.NewRequest, param, TransportType.Email);
//            return result.Subject;
//        }

//        private static IEnumerable<TestCaseData> MessageSubjectResolver
//        {
//            get
//            {
//                yield return new TestCaseData(
//                    "{Username} has requested a {Type}",
//                    new NotificationMessageCurlys("Jamie", "Finding Dory", DateTime.Now.ToString(), "Movie", string.Empty))
//                    .Returns("Jamie has requested a Movie").SetName("Subject Curlys");

//                yield return new TestCaseData(
//                    null,
//                    new NotificationMessageCurlys("Jamie", "Finding Dory", DateTime.Now.ToString(), "Movie", string.Empty))
//                    .Returns(string.Empty).SetName("Empty Subject");

//                yield return new TestCaseData(
//                    "New Request Incoming!",
//                    new NotificationMessageCurlys("Jamie", "Finding Dory", DateTime.Now.ToString(), "Movie", string.Empty))
//                    .Returns("New Request Incoming!").SetName("No curlys");

//                yield return new TestCaseData(
//                    "%$R£%$£^%$&{Username}@{}:§",
//                    new NotificationMessageCurlys("Jamie", "Finding Dory", DateTime.Now.ToString(), "Movie", string.Empty))
//                    .Returns("%$R£%$£^%$&Jamie@{}:§").SetName("Special Chars");
//            }
//        }
//        private static IEnumerable<TestCaseData> MessageBodyResolver
//        {
//            get
//            {
//                yield return new TestCaseData(
//                    "There has been a new request from {Username}, Title: {Title} for {Type}",
//                    new NotificationMessageCurlys("Jamie", "Finding Dory", DateTime.Now.ToString(), "Movie", string.Empty))
//                    .Returns("There has been a new request from Jamie, Title: Finding Dory for Movie").SetName("FindingDory");

//                yield return new TestCaseData(
//                      null,
//                     new NotificationMessageCurlys(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty))
//                     .Returns(string.Empty)
//                     .SetName("Empty Message");

//                yield return new TestCaseData(
//                    "{{Wowwzer}} Damn}{{Username}}}}",
//                    new NotificationMessageCurlys("HEY!", string.Empty, string.Empty, string.Empty, string.Empty))
//                    .Returns("{{Wowwzer}} Damn}{HEY!}}}")
//                    .SetName("Multiple Curlys");


//                yield return new TestCaseData(
//                    "This is a message with no curlys",
//                    new NotificationMessageCurlys("Jamie", "Finding Dory", DateTime.Now.ToString(), "Movie", string.Empty))
//                    .Returns("This is a message with no curlys")
//                    .SetName("No Curlys");

//                yield return new TestCaseData(
//                    new string(')', 5000),
//                    new NotificationMessageCurlys(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty))
//                    .Returns(new string(')', 5000))
//                    .SetName("Long String");


//                yield return new TestCaseData(
//                    "This is a {Username} and {Username} Because {Issue}{Issue}",
//                     new NotificationMessageCurlys("HEY!", string.Empty, string.Empty, string.Empty, "Bob"))
//                    .Returns("This is a HEY! and HEY! Because BobBob")
//                    .SetName("Double Curly");

//                yield return new TestCaseData(
//                     "This is a {username} and {username} Because {Issue}{Issue}",
//                     new NotificationMessageCurlys("HEY!", string.Empty, string.Empty, string.Empty, "Bob"))
//                    .Returns("This is a {username} and {username} Because BobBob")
//                    .SetName("Case sensitive");

//                yield return new TestCaseData(
//                   "{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}{Date}",
//                     new NotificationMessageCurlys("HEY!", string.Empty, "b", string.Empty, "Bob"))
//                    .Returns("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")
//                    .SetName("Lots of curlys");
//            }
//        }
//    }
//}