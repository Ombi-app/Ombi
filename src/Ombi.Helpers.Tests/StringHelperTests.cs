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

        [TestCase("john@example.com", "john-example-com", TestName = "SanitizeTagLabel_EmailAddress")]
        [TestCase("user_name_123", "user-name-123", TestName = "SanitizeTagLabel_Underscores")]
        [TestCase("UserName123", "username123", TestName = "SanitizeTagLabel_Uppercase")]
        [TestCase("John Smith", "john-smith", TestName = "SanitizeTagLabel_Spaces")]
        [TestCase("user--name___test", "user-name-test", TestName = "SanitizeTagLabel_ConsecutiveHyphens")]
        [TestCase("__username__", "username", TestName = "SanitizeTagLabel_LeadingTrailingHyphens")]
        [TestCase("user!@#$%name&*()123", "user-name-123", TestName = "SanitizeTagLabel_SpecialCharacters")]
        [TestCase(null, "", TestName = "SanitizeTagLabel_NullInput")]
        [TestCase("", "", TestName = "SanitizeTagLabel_EmptyInput")]
        [TestCase("validtag123", "validtag123", TestName = "SanitizeTagLabel_AlreadyValid")]
        [TestCase("User.Name+Tag@Example-Domain.com", "user-name-tag-example-domain-com", TestName = "SanitizeTagLabel_ComplexEmail")]
        public void SanitizeTagLabel_ShouldSanitizeCorrectly(string input, string expected)
        {
            Assert.AreEqual(expected, StringHelper.SanitizeTagLabel(input));
        }
    }
}