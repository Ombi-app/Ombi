#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: StringHelperTests.cs
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
using NUnit.Framework;

using PlexRequests.Store;

namespace PlexRequests.Helpers.Tests
{
    [TestFixture]
    public class TypeHelperTests
    {
        [TestCaseSource(nameof(TypeData))]
        public string[] FirstCharToUpperTest(Type input)
        {
            return input.GetPropertyNames();
        }

        [Test]
        public void GetConstantsTest()
        {
            var consts = typeof(UserClaims).GetConstantsValues<string>();
            Assert.That(consts.Contains("Admin"),Is.True);
            Assert.That(consts.Contains("PowerUser"),Is.True);
            Assert.That(consts.Contains("User"),Is.True);
        }

        private static IEnumerable<TestCaseData> TypeData
        {
            get
            {
                yield return new TestCaseData(typeof(TestClass1)).Returns(new[] { "Test1", "Test2", "Test3" }).SetName("Simple Class");
                yield return new TestCaseData(typeof(int)).Returns(new string[0]).SetName("NoPropeties Class");
                yield return new TestCaseData(typeof(IEnumerable<>)).Returns(new string[0]).SetName("Interface");
                yield return new TestCaseData(typeof(string)).Returns(new[] { "Chars", "Length" }).SetName("String");
                yield return new TestCaseData(typeof(RequestedModel)).Returns(
                    new[]
                    {
                        "ProviderId", "ImdbId", "TvDbId", "Overview", "Title", "PosterPath", "ReleaseDate", "Type",
                         "Status", "Approved", "RequestedBy", "RequestedDate", "Available", "Issues", "OtherMessage", "AdminNote",
                         "SeasonList", "SeasonCount", "SeasonsRequested", "MusicBrainzId", "RequestedUsers","ArtistName",
                         "ArtistId","IssueId","Episodes","AllUsers","CanApprove","Id"
                    }).SetName("Requested Model");
            }
        }


        private sealed class TestClass1
        {
            public string Test1 { get; set; }
            public int Test2 { get; set; }
            public long[] Test3 { get; set; }
        }
    }
}