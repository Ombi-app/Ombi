using System.Collections.Generic;

using NUnit.Framework;
using Ombi.Helpers;

namespace Ombi.Core.Tests
{
    [TestFixture]
    public class StringHelperTests
    {
        [TestCaseSource(nameof(StripCharsData))]
        public string StripCharacters(string str, char[] chars)
        {
            return str.StripCharacters(chars);
        }

        private static IEnumerable<TestCaseData> StripCharsData
        {
            get
            {
                yield return new TestCaseData("this!is^a*string",new []{'!','^','*'}).Returns("thisisastring").SetName("Basic Strip Multiple Chars");
                yield return new TestCaseData("What is this madness'",new []{'\'','^','*'}).Returns("What is this madness").SetName("Basic Strip Multipe Chars");
            }
        }
    }
}