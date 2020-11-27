using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class UriHelperTests
    {
        [TestCaseSource(nameof(UrlData))]
        public string ReturnUri(string uri)
        {
            return UriHelper.ReturnUri(uri).ToString();
        }
        public static IEnumerable<TestCaseData> UrlData
        {
            get
            {
                yield return new TestCaseData("https://google.com/").Returns($"https://google.com/").SetName("ReturnUri_With_HttpScheme");
                yield return new TestCaseData("google.com/").Returns($"http://google.com/").SetName("ReturnUri_HttpScheme_Not_Provided");
                yield return new TestCaseData("http://google.com:9090/").Returns($"http://google.com:9090/").SetName("ReturnUri_WithPort");
                yield return new TestCaseData("https://google.com/").Returns($"https://google.com/").SetName("ReturnUri_With_HttpsScheme");
                yield return new TestCaseData("https://hi.google.com/").Returns($"https://hi.google.com/").SetName("ReturnUri_With_SubDomain");
                yield return new TestCaseData("https://google.com/hi").Returns($"https://google.com/hi").SetName("ReturnUri_With_Path");
                yield return new TestCaseData("https://hi.google.com/hi").Returns($"https://hi.google.com/hi").SetName("ReturnUri_With_Path_And_SubDomain");
            }
        }

        [TestCaseSource(nameof(UrlWithPortData))]
        public string ReturnUriWithPort(string uri, int port)
        {
            return UriHelper.ReturnUri(uri, port).ToString();
        }
        public static IEnumerable<TestCaseData> UrlWithPortData
        {
            get
            {

                yield return new TestCaseData("https://google.com", 443).Returns($"https://google.com/").SetName("ReturnUri_With_HttpsPort");
                yield return new TestCaseData("https://google.com/", 123).Returns($"https://google.com:123/").SetName("ReturnUri_With_HttpScheme_With_Port");
                yield return new TestCaseData("google.com/", 80).Returns($"http://google.com/").SetName("ReturnUri_HttpScheme_Not_Provided_With_Port");
                yield return new TestCaseData("https://google.com/", 7000).Returns($"https://google.com:7000/").SetName("ReturnUri_With_HttpsScheme_With_Port");
                yield return new TestCaseData("https://hi.google.com/", 1).Returns($"https://hi.google.com:1/").SetName("ReturnUri_With_SubDomain_With_Port");
                yield return new TestCaseData("https://google.com/hi", 443).Returns($"https://google.com/hi").SetName("ReturnUri_With_Path_With_Port");
                yield return new TestCaseData("https://hi.google.com/hi", 443).Returns($"https://hi.google.com/hi").SetName("ReturnUri_With_Path_And_SubDomain_With_Port");
            }
        }

        [TestCaseSource(nameof(UrlWithPortWithSSLData))]
        public string ReturnUriWithPortAndSSL(string uri, int port, bool ssl)
        {
            return UriHelper.ReturnUri(uri, port, ssl).ToString();
        }
        public static IEnumerable<TestCaseData> UrlWithPortWithSSLData
        {
            get
            {
                foreach (var d in UrlWithPortData)
                {
                    var expected = (string)d.ExpectedResult;
                    var args = d.Arguments.ToList();
                    args.Add(true);
                    var newExpected = expected.ToHttpsUrl();
                    if (args.Contains(80))
                    {
                        newExpected = expected;
                    }
                    d.TestName += "_With_SSL";

                    yield return new TestCaseData(args.ToArray()).Returns(newExpected).SetName(d.TestName);
                }
            }
        }

        [TestCaseSource(nameof(UrlWithPortWithSSLDataCasing))]
        public string ReturnUriWithPortAndSSLCasing(string uri, int port, bool ssl)
        {
            return UriHelper.ReturnUri(uri, port, ssl).ToString();
        }
        public static IEnumerable<TestCaseData> UrlWithPortWithSSLDataCasing
        {
            get
            {
                foreach (var d in UrlWithPortData)
                {
                    if (d.TestName.Contains("_Path_"))
                    {
                        continue;
                    }
                    var expected = (string)d.ExpectedResult;
                    var args = d.Arguments.ToList();
                    for (int i = 0; i < args.Count; i++)
                    {
                        if(args[i] is string)
                        {
                            args[i] = ((string)args[i]).ToUpper();
                        }
                    }
                    args.Add(true);
                    var newExpected = expected.ToHttpsUrl();
                    if (args.Contains(80))
                    {
                        newExpected = expected;
                    }
                    d.TestName += "_With_SSL_ToUpper";

                    yield return new TestCaseData(args.ToArray()).Returns(newExpected).SetName(d.TestName);
                }
            }
        }

    }
}
