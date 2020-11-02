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
        public string TestUrl(string mediaId, string url, string serverId)
        {
            // http://192.168.68.X:8096/web/index.html#!/item?id=17980&serverId=4e7a85e6ed0b49b9a6d6d15e739a566b
            return EmbyHelper.GetEmbyMediaUrl(mediaId, serverId, url);
        }

        [TestCaseSource(nameof(JellyfinUrlData))]
        public string TestJellyfinUrl(string mediaId, string url, string serverId)
        {
            // http://192.168.68.X:8097/web/index.html#!/details?id=7ffe222498445d5ebfddb31bc4fa9a6d&serverId=50cce67f0baa425093d189b3017331fb
            return EmbyHelper.GetEmbyMediaUrl(mediaId, serverId, url, true);
        }

        public static IEnumerable<TestCaseData> UrlData
        {
            get
            {
                var mediaId = 1;
                yield return new TestCaseData(mediaId.ToString(), "http://google.com", "1").Returns($"http://google.com/web/index.html#!/item?id={mediaId}&serverId=1").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain_WithoutTrailingSlash");
                yield return new TestCaseData(mediaId.ToString(), "http://google.com/", "1").Returns($"http://google.com/web/index.html#!/item?id={mediaId}&serverId=1").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain");
                yield return new TestCaseData(mediaId.ToString(), "https://google.com/", "1").Returns($"https://google.com/web/index.html#!/item?id={mediaId}&serverId=1").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain_Https");
                yield return new TestCaseData(mediaId.ToString(), string.Empty, "1").Returns($"https://app.emby.media/web/index.html#!/item?id={mediaId}&serverId=1").SetName("EmbyHelper_GetMediaUrl_WithOutCustomDomain");
            }
        }

        public static IEnumerable<TestCaseData> JellyfinUrlData
        {
            get
            {
                var mediaId = 1;
                yield return new TestCaseData(mediaId.ToString(), "http://google.com", "1").Returns($"http://google.com/web/index.html#!/details?id={mediaId}&serverId=1").SetName("EmbyHelperJellyfin_GetMediaUrl_WithCustomDomain_WithoutTrailingSlash");
                yield return new TestCaseData(mediaId.ToString(), "http://google.com/", "1").Returns($"http://google.com/web/index.html#!/details?id={mediaId}&serverId=1").SetName("EmbyHelperJellyfin_GetMediaUrl_WithCustomDomain");
                yield return new TestCaseData(mediaId.ToString(), "https://google.com/", "1").Returns($"https://google.com/web/index.html#!/details?id={mediaId}&serverId=1").SetName("EmbyHelperJellyfin_GetMediaUrl_WithCustomDomain_Https");
            }
        }
    }
}
