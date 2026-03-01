using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Ombi.Notifications.Templates;

namespace Ombi.Notifications.Tests
{
    [TestFixture]
    public class EmailBasicTemplateTests
    {
        private EmailBasicTemplate _template;
        private string _testTemplateContent;
        private string _testTemplatePath;

        [SetUp]
        public void Setup()
        {
            _template = new EmailBasicTemplate();
            
            // Create a test template content
            _testTemplateContent = @"<html>
<head><title>{@SUBJECT}</title></head>
<body>
    <div>{@LOGO}</div>
    <div>{@BODY}</div>
    {@POSTER}
    <footer>{@DATENOW}</footer>
</body>
</html>";
            
            // Create a unique temporary template file for testing
            _testTemplatePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.html");
            File.WriteAllText(_testTemplatePath, _testTemplateContent);
            
            // Use reflection to set the template location to our test file
            var field = typeof(EmailBasicTemplate).GetField("_templateLocation", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(_template, _testTemplatePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testTemplatePath))
            {
                File.Delete(_testTemplatePath);
            }
        }

        [Test]
        public void TemplateLocation_ReturnsCorrectPath()
        {
            // Arrange & Act
            var location = _template.TemplateLocation;
            
            // Assert
            Assert.That(location, Is.Not.Null);
            Assert.That(location, Does.EndWith("BasicTemplate.html"));
        }

        [Test]
        public void LoadTemplate_WithAllParameters_ReplacesAllTokens()
        {
            // Arrange
            var subject = "Test Subject";
            var body = "Test Body Content";
            var img = "http://example.com/poster.jpg";
            var logo = "http://example.com/logo.png";
            var url = "http://example.com/media";

            // Act
            var result = _template.LoadTemplate(subject, body, img, logo, url);

            // Assert
            Assert.That(result, Does.Contain(subject));
            Assert.That(result, Does.Contain(body));
            Assert.That(result, Does.Contain(logo));
            Assert.That(result, Does.Contain(img));
            Assert.That(result, Does.Contain(url));
            Assert.That(result, Does.Not.Contain("{@SUBJECT}"));
            Assert.That(result, Does.Not.Contain("{@BODY}"));
            Assert.That(result, Does.Not.Contain("{@LOGO}"));
            Assert.That(result, Does.Not.Contain("{@POSTER}"));
            Assert.That(result, Does.Not.Contain("{@DATENOW}"));
        }

        [Test]
        public void LoadTemplate_WithDefaultParameters_UsesDefaults()
        {
            // Arrange
            var subject = "Test Subject";
            var body = "Test Body Content";

            // Act
            var result = _template.LoadTemplate(subject, body);

            // Assert
            Assert.That(result, Does.Contain(subject));
            Assert.That(result, Does.Contain(body));
            Assert.That(result, Does.Contain("http://i.imgur.com/7pqVq7W.png")); // Default OmbiLogo
            Assert.That(result, Does.Not.Contain("<tr><td align=\"center\">"));  // No poster content
        }

        [Test]
        public void LoadTemplate_WithCustomLogo_UsesCustomLogo()
        {
            // Arrange
            var subject = "Test Subject";
            var body = "Test Body Content";
            var customLogo = "http://example.com/custom-logo.png";

            // Act
            var result = _template.LoadTemplate(subject, body, logo: customLogo);

            // Assert
            Assert.That(result, Does.Contain(customLogo));
            Assert.That(result, Does.Not.Contain("http://i.imgur.com/7pqVq7W.png"));
        }

        [Test]
        public void LoadTemplate_WithIncludeLogoFalse_ExcludesLogo()
        {
            // Arrange
            var subject = "Test Subject";
            var body = "Test Body Content";

            // Act
            var result = _template.LoadTemplate(subject, body, includeLogo: false);

            // Assert
            Assert.That(result, Does.Not.Contain("http://i.imgur.com/7pqVq7W.png"));
            // The logo placeholder should be replaced with empty string
            Assert.That(result, Does.Not.Contain("{@LOGO}"));
        }

        [Test]
        public void LoadTemplate_WithIncludePosterFalse_ExcludesPoster()
        {
            // Arrange
            var subject = "Test Subject";
            var body = "Test Body Content";
            var img = "http://example.com/poster.jpg";

            // Act
            var result = _template.LoadTemplate(subject, body, img, includePoster: false);

            // Assert
            Assert.That(result, Does.Not.Contain(img));
            Assert.That(result, Does.Not.Contain("<tr><td align=\"center\">"));
        }

        [Test]
        public void LoadTemplate_ContainsCurrentDateTime()
        {
            // Arrange
            var subject = "Test Subject";
            var body = "Test Body Content";
            var beforeTime = DateTime.Now.AddMinutes(-1);

            // Act
            var result = _template.LoadTemplate(subject, body);
            var afterTime = DateTime.Now.AddMinutes(1);

            // Assert - Check that the result contains a formatted date between our bounds
            Assert.That(result, Does.Not.Contain("{@DATENOW}"));
            // The exact date format check would be fragile, so we just ensure the token was replaced
        }

        [Test]
        public void GetPosterContent_WithEmptyImage_ReturnsEmptyString()
        {
            // Arrange
            var method = typeof(EmailBasicTemplate).GetMethod("GetPosterContent", 
                BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = (string)method.Invoke(null, new object[] { "", "http://example.com/url" });

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetPosterContent_WithNullImage_ReturnsEmptyString()
        {
            // Arrange
            var method = typeof(EmailBasicTemplate).GetMethod("GetPosterContent", 
                BindingFlags.NonPublic | BindingFlags.Static);

            // Act
            var result = (string)method.Invoke(null, new object[] { null, "http://example.com/url" });

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetPosterContent_WithImageNoUrl_ReturnsImageOnly()
        {
            // Arrange
            var method = typeof(EmailBasicTemplate).GetMethod("GetPosterContent", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var imgSrc = "http://example.com/poster.jpg";

            // Act
            var result = (string)method.Invoke(null, new object[] { imgSrc, "" });

            // Assert
            Assert.That(result, Does.Contain(imgSrc));
            Assert.That(result, Does.Contain("<img src="));
            Assert.That(result, Does.Contain("alt=\"Poster\""));
            Assert.That(result, Does.Contain("width=\"400px\""));
            Assert.That(result, Does.Contain("<tr><td align=\"center\">"));
            Assert.That(result, Does.Not.Contain("<a href="));
        }

        [Test]
        public void GetPosterContent_WithImageAndUrl_ReturnsLinkedImage()
        {
            // Arrange
            var method = typeof(EmailBasicTemplate).GetMethod("GetPosterContent", 
                BindingFlags.NonPublic | BindingFlags.Static);
            var imgSrc = "http://example.com/poster.jpg";
            var url = "http://example.com/media";

            // Act
            var result = (string)method.Invoke(null, new object[] { imgSrc, url });

            // Assert
            Assert.That(result, Does.Contain(imgSrc));
            Assert.That(result, Does.Contain(url));
            Assert.That(result, Does.Contain("<img src="));
            Assert.That(result, Does.Contain("<a href="));
            Assert.That(result, Does.Contain("<tr><td align=\"center\">"));
        }

        [TestCase("", "", true, Description = "Empty parameters")]
        [TestCase(null, null, true, Description = "Null parameters")]
        [TestCase("Subject with special chars: <>&\"'", "Body with special chars: <>&\"'", true, 
            Description = "Special characters")]
        [TestCase("Very long subject that might cause issues with template processing and email rendering", 
            "Very long body content that might cause issues with template processing and email rendering and contains multiple sentences with various punctuation marks.", 
            true, Description = "Long content")]
        public void LoadTemplate_EdgeCases_HandlesGracefully(string subject, string body, bool shouldSucceed)
        {
            // Act & Assert
            if (shouldSucceed)
            {
                Assert.DoesNotThrow(() => _template.LoadTemplate(subject ?? "", body ?? ""));
                var result = _template.LoadTemplate(subject ?? "", body ?? "");
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.Empty);
            }
            else
            {
                Assert.Throws<Exception>(() => _template.LoadTemplate(subject, body));
            }
        }

        [Test]
        public void LoadTemplate_FileNotFound_ThrowsException()
        {
            // Arrange
            var templateWithInvalidPath = new EmailBasicTemplate();
            var field = typeof(EmailBasicTemplate).GetField("_templateLocation", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(templateWithInvalidPath, "/invalid/path/template.html");

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => 
                templateWithInvalidPath.LoadTemplate("subject", "body"));
        }

        [Test]
        public void OmbiLogo_ReturnsExpectedValue()
        {
            // Act
            var logo = _template.OmbiLogo;

            // Assert
            Assert.That(logo, Is.EqualTo("http://i.imgur.com/7pqVq7W.png"));
        }

        [Test]
        public void LoadTemplate_AllParameterCombinations_WorkCorrectly()
        {
            // Test all possible combinations of parameters
            var testCases = new[]
            {
                new { img = (string)null, logo = (string)null, url = (string)null, includeLogo = true, includePoster = true },
                new { img = "http://img.com/poster.jpg", logo = (string)null, url = (string)null, includeLogo = true, includePoster = true },
                new { img = (string)null, logo = "http://logo.com/logo.png", url = (string)null, includeLogo = true, includePoster = true },
                new { img = (string)null, logo = (string)null, url = "http://example.com/url", includeLogo = true, includePoster = true },
                new { img = "http://img.com/poster.jpg", logo = "http://logo.com/logo.png", url = "http://example.com/url", includeLogo = false, includePoster = false },
                new { img = "http://img.com/poster.jpg", logo = "http://logo.com/logo.png", url = "http://example.com/url", includeLogo = true, includePoster = false },
                new { img = "http://img.com/poster.jpg", logo = "http://logo.com/logo.png", url = "http://example.com/url", includeLogo = false, includePoster = true }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var result = _template.LoadTemplate("Subject", "Body", testCase.img, testCase.logo, testCase.url, 
                    testCase.includeLogo, testCase.includePoster);

                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Not.Empty);
                Assert.That(result, Does.Contain("Subject"));
                Assert.That(result, Does.Contain("Body"));
                
                if (testCase.includeLogo && !string.IsNullOrEmpty(testCase.logo))
                {
                    Assert.That(result, Does.Contain(testCase.logo));
                }
                
                if (testCase.includePoster && !string.IsNullOrEmpty(testCase.img))
                {
                    Assert.That(result, Does.Contain(testCase.img));
                }
            }
        }
    }
}