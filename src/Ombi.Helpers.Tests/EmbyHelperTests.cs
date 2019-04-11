using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class EmbyHelperTests
    {
        [TestCaseSource(nameof(UrlData))]
        public string TestUrl(string mediaId, string url)
        {
            return EmbyHelper.GetEmbyMediaUrl(mediaId, url);
        }

        public static IEnumerable<TestCaseData> UrlData
        {
            get
            {
                var mediaId = 1;
                yield return new TestCaseData(mediaId.ToString(), "http://google.com").Returns($"http://google.com/#!/itemdetails.html?id={mediaId}").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain_WithoutTrailingSlash");
                yield return new TestCaseData(mediaId.ToString(), "http://google.com/").Returns($"http://google.com/#!/itemdetails.html?id={mediaId}").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain");
                yield return new TestCaseData(mediaId.ToString(), "https://google.com/").Returns($"https://google.com/#!/itemdetails.html?id={mediaId}").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain_Https");
            }
        }
    }
}
