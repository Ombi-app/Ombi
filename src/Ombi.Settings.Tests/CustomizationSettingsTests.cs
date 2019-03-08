using NUnit.Framework;
using Ombi.Settings.Settings.Models;
using System.Collections.Generic;

namespace Tests
{
    [TestFixture]
    public class CustomizationSettingsTests
    {

        [TestCaseSource(nameof(TestData))]
        public string AddToUrlTests(string applicationUrl, string append)
        {
            var c = new CustomizationSettings
            {
                ApplicationUrl = applicationUrl
            };
            c.AddToUrl(append);

            return c.ApplicationUrl;
        }

        public static IEnumerable<TestCaseData> TestData
        {
            get
            {
                yield return new TestCaseData("https://google.com/", "token?").Returns("https://google.com/token?").SetName("ForwardSlash_On_AppUrl_NotOn_Append");
            }
        }
    }
}