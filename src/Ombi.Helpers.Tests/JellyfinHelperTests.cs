using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class JellyfinHelperTests
    {
        [TestCaseSource(nameof(UrlData))]
        public string TestUrl(string mediaId, string url, string serverId)
        {
            // http://192.168.68.X:8097/web/index.html#!/details?id=7ffe222498445d5ebfddb31bc4fa9a6d&serverId=50cce67f0baa425093d189b3017331fb
            return JellyfinHelper.GetJellyfinMediaUrl(mediaId, serverId, url);
        }

        public static IEnumerable<TestCaseData> UrlData
        {
            get
            {
                var mediaId = 1;
                yield return new TestCaseData(mediaId.ToString(), "http://google.com", "1").Returns($"http://google.com/web/index.html#!/details?id={mediaId}&serverId=1").SetName("JellyfinHelper_GetMediaUrl_WithCustomDomain_WithoutTrailingSlash");
                yield return new TestCaseData(mediaId.ToString(), "http://google.com/", "1").Returns($"http://google.com/web/index.html#!/details?id={mediaId}&serverId=1").SetName("JellyfinHelper_GetMediaUrl_WithCustomDomain");
                yield return new TestCaseData(mediaId.ToString(), "https://google.com/", "1").Returns($"https://google.com/web/index.html#!/details?id={mediaId}&serverId=1").SetName("JellyfinHelper_GetMediaUrl_WithCustomDomain_Https");
            }
        }
    }
}
