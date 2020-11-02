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
            return EmbyHelper.GetEmbyMediaUrl(mediaId, serverId, url);
        }

        [TestCaseSource(nameof(JellyfinUrlData))]
        public string TestJellyfinUrl(string mediaId, string url, string serverId)
        {
            return EmbyHelper.GetEmbyMediaUrl(mediaId, serverId, url, true);
        }

        public static IEnumerable<TestCaseData> UrlData
        {
            get
            {
                var mediaId = 1;
                yield return new TestCaseData(mediaId.ToString(), "http://google.com", "1").Returns($"http://google.com/#!/item?id={mediaId}&serverId=1").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain_WithoutTrailingSlash");
                yield return new TestCaseData(mediaId.ToString(), "http://google.com/", "1").Returns($"http://google.com/#!/item?id={mediaId}&serverId=1").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain");
                yield return new TestCaseData(mediaId.ToString(), "https://google.com/", "1").Returns($"https://google.com/#!/item?id={mediaId}&serverId=1").SetName("EmbyHelper_GetMediaUrl_WithCustomDomain_Https");
                yield return new TestCaseData(mediaId.ToString(), string.Empty, "1").Returns($"https://app.emby.media/#!/item?id={mediaId}&serverId=1").SetName("EmbyHelper_GetMediaUrl_WithOutCustomDomain");
            }
        }

        public static IEnumerable<TestCaseData> JellyfinUrlData
        {
            get
            {
                var mediaId = 1;
                yield return new TestCaseData(mediaId.ToString(), "http://google.com", "1").Returns($"http://google.com/#!/itemdetails.html?id={mediaId}&serverId=1").SetName("EmbyHelperJellyfin_GetMediaUrl_WithCustomDomain_WithoutTrailingSlash");
                yield return new TestCaseData(mediaId.ToString(), "http://google.com/", "1").Returns($"http://google.com/#!/itemdetails.html?id={mediaId}&serverId=1").SetName("EmbyHelperJellyfin_GetMediaUrl_WithCustomDomain");
                yield return new TestCaseData(mediaId.ToString(), "https://google.com/", "1").Returns($"https://google.com/#!/itemdetails.html?id={mediaId}&serverId=1").SetName("EmbyHelperJellyfin_GetMediaUrl_WithCustomDomain_Https");
            }
        }
    }
}
