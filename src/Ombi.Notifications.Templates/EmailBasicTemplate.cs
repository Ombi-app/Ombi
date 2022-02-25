using System;
using System.IO;
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
#if DEBUG
                    _templateLocation = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net6.0", "Templates",
                        "BasicTemplate.html");
#else
                _templateLocation = Path.Combine(Directory.GetCurrentDirectory(), "Templates","BasicTemplate.html");
#endif
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

        public string LoadTemplate(string subject, string body, string imgsrc = default(string), string logo = default(string), string url = default(string))
        {
            var sb = new StringBuilder(File.ReadAllText(TemplateLocation));
            sb.Replace(SubjectKey, subject);
            sb.Replace(BodyKey, body);
            sb.Replace(DateKey, DateTime.Now.ToString("f"));
            sb.Replace(Poster, GetPosterContent(imgsrc, url));
            sb.Replace(Logo, string.IsNullOrEmpty(logo) ? OmbiLogo : logo);

            return sb.ToString();
        }

        private string GetPosterContent(string imgsrc, string url) {
            string posterContent;
            
            if (string.IsNullOrEmpty(imgsrc))
            {
                posterContent = string.Empty;
            }
            else
            {
                posterContent = $"<img src=\"{imgsrc}\" alt=\"Poster\" width=\"400px\" text-align=\"center\"/>";
                if (!string.IsNullOrEmpty(url))
                {
                    posterContent = $"<a href=\"{url}\">{posterContent}</a>";
                }
                posterContent = $"<tr><td align=\"center\">{posterContent}</td></tr>";
            }
            return posterContent;
        }
    }
}
