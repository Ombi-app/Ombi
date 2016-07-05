#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AuthenticationSettingsTests.cs
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

using NUnit.Framework;

using PlexRequests.Core.Models;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.Core.Tests
{
    [TestFixture]
    public class NotificationMessageResolverTests
    {
        [TestCaseSource(nameof(MessageResolver))]
        [Ignore("WIP")]
        public NotificationMessageResolution Resolve(Dictionary<NotificationType, string> body, Dictionary<string,string> param)
        {
            var n = new NotificationMessageResolver();
            var s = new NotificationSettings
            {
                Message = body,
                CustomParamaters = param
            };

            return n.ParseMessage(s, NotificationType.NewRequest);
        }

        private static IEnumerable<TestCaseData> MessageResolver
        {
            get
            {
                yield return new TestCaseData(
                    new Dictionary<NotificationType, string> { { NotificationType.NewRequest, "There has been a new request from {Username}, Title: {Title} for {Type}" } },
                    new Dictionary<string, string>{{"Username", "Jamie" },{"Title", "Finding Dory" },{"Type", "Movie" }})
                    .Returns("There has been a new request from Jamie, Title: Finding Dory for Movie")
                    .SetName("FindingDory");

                yield return new TestCaseData(
                     new Dictionary<NotificationType, string> { { NotificationType.NewRequest, string.Empty } },
                     new Dictionary<string, string>())
                     .Returns(string.Empty)
                     .SetName("Empty Message");

                yield return new TestCaseData(
                    new Dictionary<NotificationType, string> { { NotificationType.NewRequest, "{{Wowwzer}} Damn}{{son}}}}" } },
                    new Dictionary<string, string> { {"son","HEY!"} })
                    .Returns("{{Wowwzer}} Damn}{HEY!}}}")
                    .SetName("Multiple Curlys");


                yield return new TestCaseData(
                    new Dictionary<NotificationType, string> { { NotificationType.NewRequest, "This is a message with no curlys" } },
                    new Dictionary<string, string> { { "son", "HEY!" } })
                    .Returns("This is a message with no curlys")
                    .SetName("No Curlys");

                yield return new TestCaseData(
                    new Dictionary<NotificationType, string> { { NotificationType.NewRequest, new string(')', 5000)} },
                    new Dictionary<string, string> { { "son", "HEY!" } })
                    .Returns(new string(')', 5000))
                    .SetName("Long String");


                yield return new TestCaseData(
                    new Dictionary<NotificationType, string> { { NotificationType.NewRequest, "This is a {Username} and {Username} Because {Curly}{Curly}" } },
                    new Dictionary<string, string> { { "Username", "HEY!" }, {"Curly","Bob"} })
                    .Returns("This is a HEY! and HEY! Because BobBob")
                    .SetName("Double Curly");

                yield return new TestCaseData(
                    new Dictionary<NotificationType, string> { { NotificationType.NewRequest, "This is a {Username} and {Username} Because {Curly}{Curly}" } },
                    new Dictionary<string, string> { { "username", "HEY!" }, { "Curly", "Bob" } })
                    .Returns("This is a {Username} and {Username} Because BobBob")
                    .SetName("Case sensitive");

                yield return new TestCaseData(
                    new Dictionary<NotificationType, string> { { NotificationType.NewRequest, "{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}{a}" } },
                    new Dictionary<string, string> { { "a", "b" } })
                    .Returns("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb")
                    .SetName("Lots of curlys");

                yield return new TestCaseData(
                    new Dictionary<NotificationType, string> { { NotificationType.NewRequest, $"{{{new string('b', 10000)}}}" } },
                    new Dictionary<string, string> { { new string('b', 10000), "Hello" } })
                    .Returns("Hello")
                    .SetName("Very long Curly");
            }
        }

    }
}