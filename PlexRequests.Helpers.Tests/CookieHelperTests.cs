#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: DateTimeHelperTests.cs
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

using NUnit.Framework;

namespace PlexRequests.Helpers.Tests
{
    [TestFixture]
    public class CookieHelperTests
    {
        [TestCaseSource(nameof(GetAnalyticsClientId))]
        public string TestGetAnalyticsClientId(Dictionary<string,string> cookies)
        {
            return CookieHelper.GetAnalyticClientId(cookies);
        }

        private static IEnumerable<TestCaseData> GetAnalyticsClientId
        {
            get
            {
                yield return new TestCaseData(new Dictionary<string, string>()).Returns(string.Empty);
                yield return new TestCaseData(new Dictionary<string, string> { { "_ga", "GA1.1.306549087.1464005217" } }).Returns("306549087.1464005217");
                yield return new TestCaseData(new Dictionary<string,string> { {"_ga", "GA1.1.306549087" } }).Returns(string.Empty);
            }
        }
    }
}