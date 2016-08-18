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
using System.Collections.Generic;

using NUnit.Framework;

using PlexRequests.Core.Models;

namespace PlexRequests.Helpers.Tests
{
    [TestFixture]
    public class StringHelperTests
    {
        [TestCaseSource(nameof(StringData))]
        public string FirstCharToUpperTest(string input)
        {
            return input.FirstCharToUpper();
        }

        [TestCaseSource(nameof(StringCaseData))]
        public string ToCamelCaseWordsTest(string input)
        {
            return input.ToCamelCaseWords();
        }

        [TestCaseSource(nameof(PrefixData))]
        public string AddPrefix(string[] input, string prefix, string separator)
        {
            return input.AddPrefix(prefix, separator);
        }

        private static IEnumerable<TestCaseData> StringData
        {
            get
            {
                yield return new TestCaseData("abcCba").Returns("AbcCba").SetName("pascalCase");
                yield return new TestCaseData("").Returns("").SetName("Empty");
                yield return new TestCaseData("12DSAda").Returns("12DSAda").SetName("With numbers");
            }
        }

        private static IEnumerable<TestCaseData> StringCaseData
        {
            get
            {
                yield return new TestCaseData("abcCba").Returns("Abc Cba").SetName("spaces");
                yield return new TestCaseData("").Returns("").SetName("empty");
                yield return new TestCaseData("JamieRees").Returns("Jamie Rees").SetName("Name");
                yield return new TestCaseData("Jamierees").Returns("Jamierees").SetName("single word");
                yield return new TestCaseData("ThisIsANewString").Returns("This Is A New String").SetName("longer string");
                yield return new TestCaseData(IssueStatus.PendingIssue.ToString()).Returns("Pending Issue").SetName("enum pending");
                yield return new TestCaseData(IssueStatus.ResolvedIssue.ToString()).Returns("Resolved Issue").SetName("enum resolved");
            }
        }

        private static IEnumerable<TestCaseData> PrefixData
        {
            get
            {
                yield return new TestCaseData(new[] {"abc","def","ghi"}, "@", ",").Returns("@abc,@def,@ghi").SetName("Happy Path");
                yield return new TestCaseData(new[] {"abc","def","ghi"}, "!!", "").Returns("!!abc!!def!!ghi").SetName("Different Separator Path");
                yield return new TestCaseData(new[] {"abc"}, "", "").Returns("abc").SetName("Single Item");
                yield return new TestCaseData(new string[0], "", "").Returns(string.Empty).SetName("Empty Array");
                yield return new TestCaseData(new [] {"abc","aaaa"}, null, ",").Returns("abc,aaaa").SetName("Null prefix");
                yield return new TestCaseData(new [] {"abc","aaaa"}, "@", null).Returns("@abc@aaaa").SetName("Null separator test");
                yield return new TestCaseData(new [] {"abc","aaaa"}, null, null).Returns("abcaaaa").SetName("Null separator and prefix");
            }
        }
    }
}