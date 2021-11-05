using NUnit.Framework;
using Ombi.Schedule.Jobs.Ombi;
using System.Collections.Generic;

namespace Ombi.Schedule.Tests
{
    [TestFixture]
    public class NewsletterUnsubscribeTests
    {
        [TestCaseSource(nameof(Data))]
        public string GenerateUnsubscribeLinkTest(string appUrl, string id)
        {
            return NewsletterJob.GenerateUnsubscribeLink(appUrl, id);
        }

        private static IEnumerable<TestCaseData> Data
        {
            get
            {
                yield return new TestCaseData("https://google.com/", "1").Returns("https://google.com:443/unsubscribe/1").SetName("Fully Qualified");
                yield return new TestCaseData("https://google.com", "1").Returns("https://google.com:443/unsubscribe/1").SetName("Missing Slash");
                yield return new TestCaseData("google.com", "1").Returns("http://google.com:80/unsubscribe/1").SetName("Missing scheme");
                yield return new TestCaseData("ombi.google.com", "1").Returns("http://ombi.google.com:80/unsubscribe/1").SetName("Sub domain missing scheme");
                yield return new TestCaseData("https://ombi.google.com", "1").Returns("https://ombi.google.com:443/unsubscribe/1").SetName("Sub domain");
                yield return new TestCaseData("https://ombi.google.com/", "1").Returns("https://ombi.google.com:443/unsubscribe/1").SetName("Sub domain with slash");
                yield return new TestCaseData("https://google.com/ombi/", "1").Returns("https://google.com:443/ombi/unsubscribe/1").SetName("RP");
                yield return new TestCaseData("https://google.com/ombi", "1").Returns("https://google.com:443/ombi/unsubscribe/1").SetName("RP missing slash");
                yield return new TestCaseData("https://google.com:3577", "1").Returns("https://google.com:3577/unsubscribe/1").SetName("Port");
                yield return new TestCaseData("https://google.com:3577/", "1").Returns("https://google.com:3577/unsubscribe/1").SetName("Port With Slash");
                yield return new TestCaseData("", "1").Returns(string.Empty).SetName("Missing App URL empty");
                yield return new TestCaseData(null, "1").Returns(string.Empty).SetName("Missing App URL null");
                yield return new TestCaseData("hty", string.Empty).Returns(string.Empty).SetName("Missing ID empty");
                yield return new TestCaseData("hty", null).Returns(string.Empty).SetName("Missing ID null");
            }
        }
    }
}
