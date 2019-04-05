using NUnit.Framework;
using System.Collections.Generic;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class HtmlHelperTests
    {
        [TestCaseSource(nameof(HtmlData))]
        public string RemoveHtmlTests(string input)
        {
            return HtmlHelper.RemoveHtml(input);
        }

        public static IEnumerable<TestCaseData> HtmlData
        {
            get
            {
                yield return new TestCaseData("<h1>hi</h1>").Returns("hi").SetName("Simple Html");
                yield return new TestCaseData("<html><body><head></head><h1>hi</h1></body></html>").Returns("hi").SetName("Nested text inside Html");
                yield return new TestCaseData("there is no html here").Returns("there is no html here").SetName("No Html");
                yield return new TestCaseData("there is <b>some</b> html here").Returns("there is some html here").SetName("Html in middle");
                yield return new TestCaseData("<a>there</a> <u>is</u> <b>lots</b> <i>html</i> <span>here</span>").Returns("there is lots html here").SetName("Html in everywhere");
                yield return new TestCaseData("there is <span class=\"abc\">some</span> html here").Returns("there is some html here").SetName("Html in with classes");
                yield return new TestCaseData("there is <span id=\"sometag\">some</span> html here").Returns("there is some html here").SetName("Html in with attribute");
                yield return new TestCaseData("there is <span data-tag=\"sometag\" class=\"abc\">some</span> html here").Returns("there is some html here").SetName("Html in with attribute and class");
            }
        }
        public static IEnumerable<TestCaseData> OtherData
        {
            get
            {
                foreach (var data in HtmlData)
                {
                    yield return data;
                }
                yield return new TestCaseData("xyz").Returns("xyz").SetName("More Tests");
            }
        }
    }
}
