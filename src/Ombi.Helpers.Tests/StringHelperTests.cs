using NUnit.Framework;

namespace Ombi.Helpers.Tests
{
    [TestFixture]
    public class StringHelperTests
    {
        [Test]
        public void ToHttpsUrl_ShouldReturnsHttpsUrl_HttpUrl()
        {
            var sourceUrl = "http://www.test.url";
            var expectedUrl = "https://www.test.url";

            Assert.AreEqual(expectedUrl, sourceUrl.ToHttpsUrl(), "Should return the source URL as https");
        }

        [Test]
        public void ToHttpsUrl_ShouldReturnsUnchangedUrl_HttpsUrl()
        {
            var sourceUrl = "https://www.test.url";
            var expectedUrl = "https://www.test.url";

            Assert.AreEqual(expectedUrl, sourceUrl.ToHttpsUrl(), "Should return the unchanged https URL");
        }

        [Test]
        public void ToHttpsUrl_ShouldReturnsUnchangedUrl_NonHttpUrl()
        {
            var sourceUrl = "ftp://www.test.url";
            var expectedUrl = "ftp://www.test.url";

            Assert.AreEqual(expectedUrl, sourceUrl.ToHttpsUrl(), "Should return the unchanged non-http URL");
        }

        [Test]
        public void ToHttpsUrl_ShouldReturnsUnchangedUrl_InvalidUrl()
        {
            var sourceUrl = "http:/www.test.url";
            var expectedUrl = "http:/www.test.url";

            Assert.AreEqual(expectedUrl, sourceUrl.ToHttpsUrl(), "Should return the unchanged invalid URL");
        }

        [Test]
        public void ToHttpsUrl_ShouldReturnNull_NullUrl()
        {
            const string sourceUrl = null;
            const string expectedUrl = null;

            Assert.AreEqual(expectedUrl, sourceUrl.ToHttpsUrl(), "Should return null for null URL");
        }
    }
}