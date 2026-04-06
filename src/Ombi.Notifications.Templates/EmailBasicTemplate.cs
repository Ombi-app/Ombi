using System;
using System.IO;
using System.Net;
using System.Text;

namespace Ombi.Notifications.Templates
{
    public class EmailBasicTemplate : TemplateBase, IEmailBasicTemplate
    {
        public override string TemplateLocation
        {
            get
            {
                if (string.IsNullOrEmpty(_templateLocation))
                {
                    //_templateLocation = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net6.0", "Templates",
                    //    "BasicTemplate.html");
                    _templateLocation = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "BasicTemplate.html");
                }
                return _templateLocation;
            }
        }

        private string _templateLocation;

        private const string SubjectKey = "{@SUBJECT}";
        private const string BodyKey = "{@BODY}";
        private const string Poster = "{@POSTER}";
        private const string DateKey = "{@DATENOW}";
        private const string Logo = "{@LOGO}";


        public string LoadTemplate(string subject, string body, string img = default(string), string logo = default(string), string url = default(string), bool includeLogo = true, bool includePoster = true)
        {
            var sb = new StringBuilder(File.ReadAllText(TemplateLocation));
            sb.Replace(SubjectKey, subject);
            sb.Replace(BodyKey, body);
            sb.Replace(DateKey, DateTime.Now.ToString("f"));
            sb.Replace(Poster, includePoster ? GetPosterContent(img, url) : string.Empty);
            var logoContent = GetLogoContent(logo, includeLogo);
            sb.Replace(Logo, logoContent);

            return sb.ToString();
        }

        private static string GetPosterContent(string img, string url)
        {
            string posterContent;

            if (string.IsNullOrEmpty(img))
            {
                posterContent = string.Empty;
            }
            else
            {
                var encodedImg = WebUtility.HtmlEncode(img);
                posterContent = $"<img src=\"{encodedImg}\" alt=\"Poster\" width=\"400px\" text-align=\"center\"/>";
                if (!string.IsNullOrEmpty(url))
                {
                    var encodedUrl = WebUtility.HtmlEncode(url);
                    posterContent = $"<a href=\"{encodedUrl}\">{posterContent}</a>";
                }
                posterContent = $"<tr><td align=\"center\">{posterContent}</td></tr>";
            }
            return posterContent;
        }

        private string GetLogoContent(string logo, bool includeLogo)
        {
            if (!includeLogo)
            {
                return string.Empty;
            }
            
            return WebUtility.HtmlEncode(string.IsNullOrEmpty(logo) ? OmbiLogo : logo);
        }
    }
}
