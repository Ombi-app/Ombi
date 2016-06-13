#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: LandingPageTests.cs
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

using PlexRequests.UI.Models;

namespace PlexRequests.UI.Tests
{
    [TestFixture]
    public class LandingPageTests
    {
        [TestCaseSource(nameof(NoticeEnabledData))]
        public bool TestNoticeEnabled(DateTime start, DateTime end)
        {
            return new LandingPageViewModel {NoticeEnd = end, NoticeStart = start}.NoticeActive;
        }

        private static IEnumerable<TestCaseData> NoticeEnabledData
        {
            get
            {
                yield return new TestCaseData(DateTime.Now, DateTime.Now.AddDays(1)).Returns(true);
                yield return new TestCaseData(DateTime.Now, DateTime.Now.AddDays(99)).Returns(true);
                yield return new TestCaseData(DateTime.Now.AddDays(2), DateTime.Now).Returns(false); // End in past
                yield return new TestCaseData(DateTime.Now.AddDays(2), DateTime.Now.AddDays(3)).Returns(false); // Not started yet
                yield return new TestCaseData(DateTime.Now.AddDays(-5), DateTime.Now.AddDays(-1)).Returns(false); // Finished yesterday
            }
        }
    }
}